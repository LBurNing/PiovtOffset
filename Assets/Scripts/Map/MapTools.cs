using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class MapResData
{
    public MapResType resType;
    public string path;
    public int index = -1;
 
    public int Index
    {
        get
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            Match match = Regex.Match(fileName, @"\d+");

            int offsetIndex = 0;
            switch(resType)
            {
                case MapResType.obj:
                    offsetIndex = 20;
                    break;
                case MapResType.smtiles:
                    offsetIndex = 10;
                    break;
                case MapResType.tiles:
                    offsetIndex = 0;
                    break;
            }

            return MapTools.DEFAULT_INDEX + offsetIndex + int.Parse(match.Value) - 1;
        }
    }
}

public class MapData
{
    public string outPath;
    public List<string> paths = new List<string>();
    public List<MapResData> mapResList = new List<MapResData>();
}

public static class MapTools
{
    public static int CellWidth = 48;
    public static int CellHeight = 32;
    private static int libIndex, index;
    private static int mapWidth, mapHeight;
    private static MapReader map;
    private static MapInfoJsonData mapInfoJsonData;
    public static CellInfo[,] M2CellInfo;
    private static GameObject mapTiles;
    private static Texture2D thumbnail;
    private static Texture2D mapTexture;
    private static string mapSavePath = "";
    private static string mapJsonSavePath = "";
    private static string mapThumbnailSavePath = "";
    private static List<MapBlock> mapBlocks;

    public static int PROGRESS_INTERVAR = 200;
    public static int DEFAULT_INDEX = 100;
    public static int mapBlockSize_x = 256;
    public static int mapBlockSize_y = 256;
    public static int blockYCount = 0;
    public static int blockXCount = 0;
    public static float widthScaleMultiple = 0;
    public static float heightScaleMultiple = 0;
    public static int thumbnailWidth = 600;
    public static int thumbnailHeight = 400;
    public static int textureMaxSize = 16384;
    public static List<MapData> mapdatas = new List<MapData>();
    public static string outMapRootPath;
    private static Action<int, int, string> progressCallback = null;

    public static IEnumerator ReadMapData(Action<int, int, string> progress = null)
    {
        progressCallback = progress;
        for (int i = 0; i < Libraries.MapLibs.Length; i++)
        {
            Libraries.MapLibs[i] = new MLibrary("");
        }

        foreach (MapData data in mapdatas)
        {
            foreach(MapResData resData in data.mapResList)
            {
                if (string.IsNullOrEmpty(resData.path))
                    continue;

                Libraries.MapLibs[resData.index != -1 ?resData.index : resData.Index] = new MLibrary(resData.path);
                Libraries.ListItems[resData.index != -1 ? resData.index : resData.Index] = new ListItem(resData.path, resData.Index);
            }

            if (string.IsNullOrEmpty(data.outPath))
            {
                Notice.ShowNotice("输出文件夹不能为空..", Color.red, 3);
                yield break;
            }

            outMapRootPath = Path.GetFullPath(data.outPath + "/Maps/");
            int count = data.paths.Count;
            while (count > 0)
            {
                yield return UnzipMap(data.paths[count - 1]);
                count--;
            }
        }

        yield return null;
    }

    public static IEnumerator UnzipMap(string mapPath)
    {
        InitMap(mapPath);
        yield return InitList();
        InitPath();
        yield return DrawBack();
        yield return DrawMidd();
        yield return DrawFront();
        yield return null;

        SaveMapBlockTexture();
        string jpgPath = mapSavePath + map.mapName + ".jpg";
        ImageTools.ColorToblack(jpgPath, jpgPath);
        Dispose();
        progressCallback?.Invoke(0, 0, "导出成功: " + mapSavePath);
        yield return null;
    }

    public static void InitThumbnail()
    {
        thumbnail = new Texture2D(thumbnailWidth, thumbnailHeight);
        widthScaleMultiple = map.PixelWidth / thumbnailWidth;
        heightScaleMultiple = map.PixelHeight / thumbnailHeight;
    }

    public static void InitMap(string mapPath)
    {
        map = new MapReader(mapPath);
        M2CellInfo = map.MapCells;
        SetMapSize(map.Width, map.Height);

        mapInfoJsonData = new MapInfoJsonData();
        mapInfoJsonData.width = map.PixelWidth;
        mapInfoJsonData.height = map.PixelHeight;
        mapInfoJsonData.name = map.mapName;
    }

    private static IEnumerator InitList()
    {
        mapBlockSize_x = map.PixelWidth / 10;
        mapBlockSize_y = map.PixelHeight / 10;
        mapBlocks = new List<MapBlock>();
        blockXCount = Mathf.CeilToInt(((float)map.PixelWidth / (float)mapBlockSize_x));
        blockYCount = Mathf.CeilToInt(((float)map.PixelHeight / (float)mapBlockSize_y));
        int index = 0;
        int totalCount = blockXCount * blockYCount;

        if (map.PixelWidth > textureMaxSize || map.PixelHeight > textureMaxSize)
        {
            for (int y = 0; y < blockYCount; y++)
            {
                for (int x = 0; x < blockXCount; x++)
                {
                    MapBlock mapBlock = new MapBlock(x, blockYCount - y - 1);
                    mapBlocks.Add(mapBlock);

                    index++;
                    progressCallback?.Invoke(index, totalCount, "地图切块");
                    yield return new WaitForSeconds(ExportRes.frameTime);
                }
            }
        }
        else
        {
            mapTexture = new Texture2D(map.PixelWidth, map.PixelHeight);
        }

        yield return null;
    }

    private static void InitPath()
    {
        mapSavePath = outMapRootPath + @"map\tiles\" + map.mapName + @"\";
        if (!Directory.Exists(mapSavePath))
            Directory.CreateDirectory(mapSavePath);

        mapJsonSavePath = outMapRootPath + @"map\Json\";
        if (!Directory.Exists(mapJsonSavePath))
            Directory.CreateDirectory(mapJsonSavePath);
    }

    public static void SetMapSize(int w, int h)
    {
        mapWidth = w;
        mapHeight = h;
    }

    private static void ClearImage()
    {
        for (var i = 0; i < Libraries.MapLibs.Length; i++)
        {
            for (var j = 0; j < Libraries.MapLibs[i].Images.Count; j++)
            {
                Libraries.MapLibs[i].Images[j] = null;
            }
        }
    }

    private static IEnumerator DrawBack()
    {
        int tileIndex = 0;
        int tileCount = map.Height * map.Width;
        int drawY = 0;
        int drawX = 0;
        int progress = 0;

        for (int y = map.Height; y >= -1; y--)
        {
            if (y % 2 != 0)
            {
                tileIndex += map.Width;
                continue;
            }

            if (y >= mapHeight)
            {
                tileIndex += map.Width;
                continue;
            }

            drawY = y * CellHeight;
            for (int x = -1; x <= map.Width; x++)
            {
                tileIndex++;
                if (x % 2 != 0)
                    continue;

                if (x >= mapWidth)
                    continue;

                index = (M2CellInfo[x, y].BackImage & 0x1FFFFFFF) - 1;
                libIndex = M2CellInfo[x, y].BackIndex;
                drawX = x * CellWidth;

                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length)
                    continue;

                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count)
                    continue;
             
                var size = Libraries.MapLibs[libIndex].GetSize(index);
                Draw(libIndex, index, drawX, drawY);

                progress++;
                progressCallback?.Invoke(tileIndex, tileCount, "地图: ");

                if (progress % PROGRESS_INTERVAR == 0)
                    yield return new WaitForSeconds(ExportRes.frameTime);
            }
        }

        yield return null;
    }

    private static IEnumerator DrawMidd()
    {
        byte animation;
        int drawX = 0;
        int drawY = 0;
        int tileIndex = 0;
        int tileCount = map.Height * map.Width;
        int progress = 0;

        for (var y = - 1; y <= map.Height; y++)
        {
            if (y >= mapHeight || y < 0)
                continue;

            for (var x = - 1; x <= map.Width; x++)
            {
                tileIndex++;
                if (x >= mapWidth || x < 0)
                    continue;

                drawX = x * CellWidth;
                index = M2CellInfo[x, y].MiddleImage - 1;
                libIndex = M2CellInfo[x, y].MiddleIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                animation = M2CellInfo[x, y].MiddleAnimationFrame;

                if ((animation > 0) && (animation < 255))
                {
                    if ((animation & 0x0f) > 0)
                    {

                        animation &= 0x0f;
                    }
                    if (animation > 0)
                    {
                        var animationTick = M2CellInfo[x, y].MiddleAnimationTick;
                        index += 1 % (animation + animation * animationTick) / (1 + animationTick);
                    }
                }

                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                {
                    drawY = (y + 1) * CellHeight;
                    DrawMidd(libIndex, index, drawX, drawY - s.Height);
                }
                else
                {
                    drawY = y * CellHeight;
                    DrawMidd(libIndex, index, drawX, drawY);
                }

                progress++;
                progressCallback?.Invoke(tileIndex, tileCount, "地图: ");

                if (progress % PROGRESS_INTERVAR == 0)
                    yield return new WaitForSeconds(ExportRes.frameTime);
            }
        }

        yield return null;
    }

    private static IEnumerator DrawFront()
    {
        byte animation;
        bool blend;
        int drawY = 0;
        int tileIndex = 0;
        int tileCount = map.Height * map.Width;
        int progress = 0;

        for (var y = 0; y <= map.Height; y++)
        {
            if (y >= mapHeight || y < 0) 
                continue;

            for (var x = 0; x <= map.Width; x++)
            {
                tileIndex++;
                if (x >= mapWidth || x < 0) 
                    continue;

                int drawX = x * CellWidth;
                index = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;
                libIndex = M2CellInfo[x, y].FrontIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length)
                    continue;

                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count)
                    continue;

                CellInfo cellInfo = M2CellInfo[x, y];
                animation = cellInfo.FrontAnimationFrame;
                if ((animation & 0x80) > 0)
                {
                    blend = true;
                    animation &= 0x7F;
                }
                else
                {
                    blend = false;
                }

                if (animation > 0)
                {
                    var animationTick = M2CellInfo[x, y].FrontAnimationTick;
                    index += 1 % (animation + animation * animationTick) / (1 + animationTick);
                }

                var doorOffset = M2CellInfo[x, y].DoorOffset;
                var s = Libraries.MapLibs[libIndex].GetSize(index);
                //不是 48*32 或96*64 的地砖 是大物体
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                {
                    drawY = (y + 1) * CellHeight;
                    //如果有动画
                    if (animation > 0)
                    {
                        //如果需要混合
                        if (blend)
                        {
                            //新盛大地图
                            if ((libIndex > 99) & (libIndex < 199))
                            {
                                //DrawFront(libIndex, index, drawX, drawY - 3 * CellHeight, true);
                            }
                            //老地图灯柱 index >= 2723 && index <= 2732
                            else
                            {
                                //DrawFront(libIndex, index, drawX, drawY - s.Height, true);
                            }
                        }
                        //不需要混合
                        else
                        {
                            DrawFront(libIndex, index, drawX, drawY - s.Height);
                        }
                    }
                    //如果没动画 
                    else
                    {
                        DrawFront(libIndex, index, drawX, drawY - s.Height);
                    }
                }
                //是 48*32 或96*64 的地砖
                else
                {
                    drawY = y * CellHeight;
                    DrawFront(libIndex, index, drawX, drawY);
                }

                //显示门打开
                if ((doorOffset > 0))
                {
                    drawY = (y + 1) * CellHeight;
                    DrawFront(libIndex, index + doorOffset, drawX, drawY - s.Height);
                }

                progress++;
                progressCallback?.Invoke(tileIndex, tileCount, "地图: ");

                if (progress % PROGRESS_INTERVAR == 0)
                    yield return new WaitForSeconds(ExportRes.frameTime);
            }
        }

        yield return null;
    }

    private static void DrawFront(int libIndex, int index, int x, int y, bool isAlpha = false)
    {
        string mapTilesName = "MapFrontTiles";
        mapTiles = GameObject.Find(mapTilesName);
        if (mapTiles == null)
            mapTiles = new GameObject(mapTilesName);

        MLibrary.MImage mImage = Libraries.MapLibs[libIndex].GetMImage(index);
        if (mImage.Image == null)
        {
            return;
        }

        MemoryStream ms = new MemoryStream();
        mImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        var buffer = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(buffer, 0, buffer.Length);
        Texture2D t2D = new Texture2D(mImage.Width, mImage.Height);
        t2D.LoadImage(buffer);
        t2D.Apply();

        if (isAlpha)
        {
            for (int colori = 0; colori < t2D.width; colori++)
            {
                for (int colorj = 0; colorj < t2D.height; colorj++)
                {
                    Color color = t2D.GetPixel(colori, colorj);
                    float r = color.r;
                    float g = color.g;
                    float b = color.b;
                    float a = color.a;
                    float addColorValue = 0;

                    float alpha = color.grayscale;
                    if (a > 0)
                    {
                        alpha = alpha + alpha / 1.2f;
                        addColorValue = color.grayscale / 5;
                    }

                    a = alpha;

                    color = new Color(r + addColorValue, g + addColorValue, b + addColorValue, a);
                    t2D.SetPixel(colori, colorj, color);
                }
            }

            t2D.Apply();
        }

        SetMapColor(x, y, t2D, mImage);
        UnityEngine.Object.DestroyImmediate(t2D);
        ms.Close();
        ms.Dispose();
    }

    private static void DrawMidd(int libIndex, int index, int x, int y)
    {
        string mapTilesName = "MapMiddTiles";
        mapTiles = GameObject.Find(mapTilesName);
        if (mapTiles == null)
            mapTiles = new GameObject(mapTilesName);

        MLibrary.MImage mImage = Libraries.MapLibs[libIndex].GetMImage(index);
        if (mImage.Image == null)
        {
            return;
        }

        MemoryStream ms = new MemoryStream();
        mImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        var buffer = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(buffer, 0, buffer.Length);
        Texture2D t2D = new Texture2D(mImage.Width, mImage.Height);
        t2D.LoadImage(buffer);
        t2D.Apply();

        //DrawTileTexture(libIndex, index, x, y, t2D, mImage);

        SetMapColor(x, y, t2D, mImage);

        UnityEngine.Object.DestroyImmediate(t2D);
        ms.Close();
        ms.Dispose();
    }

    private static void Draw(int libIndex, int index, int x, int y)
    {
        string mapTilesName = "MapTiles";
        mapTiles = GameObject.Find(mapTilesName);
        if (mapTiles == null)
            mapTiles = new GameObject(mapTilesName);

        MLibrary.MImage mImage = Libraries.MapLibs[libIndex].GetMImage(index);
        if (mImage.Image == null)
        {
            return;
        }

        MemoryStream ms = new MemoryStream();
        mImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        var buffer = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(buffer, 0, buffer.Length);
        Texture2D t2D = new Texture2D(mImage.Width, mImage.Height);
        t2D.LoadImage(buffer);
        t2D.Apply();

        //DrawTileTexture(libIndex, index, x, y, t2D, mImage);
        SetMapColor(x, y, t2D, mImage);

        UnityEngine.Object.DestroyImmediate(t2D);
        ms.Close();
        ms.Dispose();
    }

    private static void SetMapColor(int x, int y, Texture2D t2D, MLibrary.MImage mImage)
    {
        y = map.PixelHeight - y - mImage.Height;

        for (int i = 0; i < mImage.Width; i++)
        {
            for (int j = 0; j < mImage.Height; j++)
            {
                Color color = t2D.GetPixel(i, j);
                if (color != Color.clear)
                {
                    int pixelX = x + i;
                    int pixelY = y + j;

                    if (map.PixelHeight > textureMaxSize || map.PixelWidth > textureMaxSize)
                    {
                        if (pixelX > map.PixelWidth || pixelY > map.PixelHeight)
                            continue;

                        int indexX = pixelX / mapBlockSize_x;
                        int indexY = blockYCount - pixelY / mapBlockSize_y - 1;

                        if (indexY <= 0)
                            indexY = 0;

                        int index = indexY * blockXCount + indexX;

                        MapBlock mapBlock = mapBlocks[index];

                        if (mapBlock != null)
                        {
                            mapBlock.SetColor(pixelX, pixelY, color);
                        }
                    }
                    else
                    {
                        mapTexture.SetPixel(pixelX, pixelY, color);
                    }
                }
            }
        }
    }

    private static void SaveThumbnail()
    {
        if (thumbnail == null)
            return;

        int sizeX = thumbnailWidth / blockXCount;
        int sizeY = thumbnailHeight / blockYCount;
        int offsetY = (mapHeight - blockYCount * sizeY);

        for (int i = 0; i < thumbnailWidth; i++)
        {
            for (int j = 0; j < thumbnailHeight; j++)
            {
                thumbnail.SetPixel(i, j, Color.black);
            }
        }

        for (int y = blockYCount - 1; y >= 0; y--)
        {
            for (int x = 0; x < blockXCount; x++)
            {
                int index = y * blockXCount + x;
                MapBlock mapBlock = mapBlocks[index];

                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        ColorEx color = mapBlock._blocks[i * (mapBlockSize_x / sizeX), j * (mapBlockSize_y / sizeY)];
                        thumbnail.SetPixel(x * sizeX + i, thumbnailHeight - (y + 1) * sizeY + j - offsetY, new Color(color.r, color.g, color.b, 1));
                    }
                }
            }
        }

        thumbnail.Apply();
        //DrawMapTexture(thumbnail, "thumbnail", Vector3.zero);
        string thumbnailPath = mapThumbnailSavePath + map.mapName + ".jpg";
        Utils.SavePng(thumbnailPath, thumbnail);
        UnityEngine.Object.DestroyImmediate(thumbnail);
    }

    private static void SaveMapBlockTexture()
    {
        float index = 0;
        float totalCount = mapBlocks.Count;

        if (map.PixelWidth > textureMaxSize || map.PixelHeight > textureMaxSize)
        {
            foreach (MapBlock mapBlock in mapBlocks)
            {
                //DrawMapTexture(mapBlock.GetTexture2D, mapBlock.GetName, mapBlock.GetPosition);
                SaveMapTexture(mapBlock.GetTexture2D, mapBlock.GetName);
                index++;
            }
        }
        else
        {
            mapTexture.Apply();
            //DrawMapTexture(mapTexture, map.mapName, Vector3.zero);
            SaveMapTexture(mapTexture, map.mapName);
        }

        SaveMapJson();
    }

    private static void SaveMapJson()
    {
        string mapJson = JsonUtility.ToJson(mapInfoJsonData);
        string jsonPath = mapJsonSavePath + map.mapName + ".json";
        File.WriteAllText(jsonPath, mapJson);
    }

    private static void SaveMapTexture(Texture2D t2D, string name)
    {
        if (t2D == null)
            return;

        string savePath = mapSavePath + name + ".jpg";
        Utils.SavePng(savePath, t2D);

        UnityEngine.Object.DestroyImmediate(t2D);
    }

    private static MapBlock Exist(int pixelX, int pixelY)
    {
        for (int i = 0; i < mapBlocks.Count; i++)
        {
            if (mapBlocks[i].Exist(pixelX, pixelY))
                return mapBlocks[i];
        }

        return null;
    }

    private static void DrawTileTexture(int libIndex, int index, int x, int y, Texture2D t2D, MLibrary.MImage mImage)
    {
        GameObject go = new GameObject(index.ToString() + "  " + libIndex.ToString());
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 0;
        spriteRenderer.sprite = Sprite.Create(t2D, new Rect(0, 0, mImage.Width, mImage.Height), new Vector2(0, 1));
        spriteRenderer.size = new Vector2(mImage.Width, mImage.Height);
        go.transform.parent = mapTiles.transform;
        go.transform.position = new Vector3((float)x / 100.0f, (float)map.PixelHeight - (y) / 100.0f, 0);
    }

    private static void DrawMapTexture(Texture2D t2D, string name, Vector3 pos)
    {
        if (t2D == null)
            return;

        GameObject mapBlockGo = GameObject.Find("MapBlocks");
        if (mapBlockGo == null)
        {
            mapBlockGo = new GameObject("MapBlocks");
        }

        GameObject mapTextureGo = GameObject.Find(name);
        if (mapTextureGo == null)
        {
            mapTextureGo = new GameObject(name);
            mapTextureGo.transform.parent = mapBlockGo.transform;
            mapTextureGo.AddComponent<SpriteRenderer>();
        }

        mapTextureGo.transform.position = pos;
        SpriteRenderer spriteRenderer = mapTextureGo.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(t2D, new Rect(0, 0, t2D.width, t2D.height), new Vector2(0.5f, 0.5f));
    }

    // 垂直翻转
    private static Texture2D VerticalFlipTexture(Texture2D texture)
    {
        //得到图片的宽高
        int width = texture.width;
        int height = texture.height;

        Texture2D flipTexture = new Texture2D(width, height);
        for (int i = 0; i < height; i++)
        {
            flipTexture.SetPixels(0, i, width, 1, texture.GetPixels(0, height - i - 1, width, 1));
        }
        flipTexture.Apply();
        return flipTexture;
    }

    private static void Dispose()
    {
        map.MapCells = null;
        map = null;
        M2CellInfo = null;

        if (mapBlocks != null)
        {
            foreach (MapBlock mapBlock in mapBlocks)
            {
                mapBlock.Dispose();
            }

            mapBlocks.Clear();
        }

        if (thumbnail != null)
        {
            UnityEngine.Object.DestroyImmediate(thumbnail);
            thumbnail = null;
        }

        if (mapTexture != null)
        {
            UnityEngine.Object.DestroyImmediate(mapTexture);
            mapTexture = null;
        }

        ClearImage();
        GC.Collect();
    }
}


