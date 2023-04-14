using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class PivotOffset
{
    public static void Load(string path, Vector2Int offset)
    {
        Global.InitTransparentColor();
        byte[] buffer = File.ReadAllBytes(path);
        Texture2D t2D = new Texture2D(1, 1);
        t2D.LoadImage(buffer);
        t2D.Apply();

        int width = t2D.width;
        int height = t2D.height;

        Texture2D pngTexture = new Texture2D(width, height);
        pngTexture.SetPixels(Global.defaultColors.ToArray());

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color color = t2D.GetPixel(i, j);
                if (color == Color.clear)
                    continue;

                int x = i + offset.x;
                int y = j + offset.y;

                if (x > 0 && y > 0)
                {
                    pngTexture.SetPixel(x, y, color);
                }
            }
        }

        pngTexture.Apply();
        SavePng(path, pngTexture);
        GameObject.Destroy(t2D);
        GameObject.Destroy(pngTexture);
        t2D = null;
        pngTexture = null;
    }

    public static void ScaleTexture(string path, string outPath, float proportion, Vector2Int offset)
    {
        byte[] buffer = File.ReadAllBytes(path);
        Texture2D t2D = new Texture2D(1, 1);
        t2D.LoadImage(buffer);
        t2D.Apply();

        bool scale = proportion > 1 || proportion < 0.999f;
        Texture2D newTexture = ScaleTexture(t2D, proportion * t2D.width, proportion * t2D.height, offset, scale);
        newTexture.Apply();

        SavePng(path, newTexture, outPath);
        GameObject.Destroy(t2D);
        GameObject.Destroy(newTexture);
        t2D = null;
        newTexture = null;
    }

    public static void Rotate(string path, float rotationAngle)
    {
        if (rotationAngle == 0 || rotationAngle == 360)
            return;

        string[] filePaths = Directory.GetFiles(path);
        byte[] buffer = File.ReadAllBytes(filePaths[0]);
        Texture2D t2D = new Texture2D(1, 1);
        t2D.LoadImage(buffer);
        t2D.Apply();

        Vector2Int vector2Int = TextureHelper.CalculateBoundingBox(t2D);
        int add_width = 0;
        int add_height = 0;
        int width = t2D.width - 5;
        int height = t2D.height - 5;

        if (vector2Int.y > width)
        {
            //加宽
            add_width = (vector2Int.y - (width - 5)) * 2;
        }

        if (vector2Int.x > height)
        {
            //加高
            add_height = (vector2Int.x * (height - 5)) * 2;
        }
        GameObject.Destroy(t2D);

        if(add_width > 0 || add_height > 0)
        {
            List<Color> defaultColors = new List<Color>();
            for (int i = 0; i < width + add_width; i++)
            {
                for (int j = 0; j < height + add_height; j++)
                {
                    Color color = new Color(0, 0, 0, 0);
                    defaultColors.Add(color);
                }
            }

            foreach (string filePath in filePaths)
            {
                buffer = File.ReadAllBytes(filePath);
                t2D = new Texture2D(1, 1);
                t2D.LoadImage(buffer);
                t2D.Apply();

                Texture2D pngTexture = new Texture2D(width + add_width, height + add_height);
                pngTexture.SetPixels(defaultColors.ToArray());

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Color color = t2D.GetPixel(i, j);
                        if (color == Color.clear)
                            continue;

                        int x = i + (add_width / 2);
                        int y = j + (add_height / 2);

                        if (x > 0 && y > 0)
                        {
                            pngTexture.SetPixel(x, y, color);
                        }
                    }
                }

                pngTexture.Apply();
                SavePng(filePath, pngTexture);
                GameObject.Destroy(t2D);
                GameObject.Destroy(pngTexture);
            }
        }

        CmdUtil.ProcessCommand(Global.rotationImageToolPath, string.Format("{0} {1}", path, (int)rotationAngle));
    }

    // 缩放纹理的方法
    /// <summary>
    /// 缩放Textur2D
    /// </summary>
    /// <param name="source"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    public static Texture2D ScaleTexture(Texture2D source, float targetWidth, float targetHeight, Vector2Int offset, bool scale)
    {
        Texture2D result = new Texture2D((int)targetWidth, (int)targetHeight, source.format, false);

        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixel(j, i);
                if (scale)
                {
                    newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j + offset.x, i + offset.y, newColor);
                }
                else
                {
                    result.SetPixel(j + offset.x, i + offset.y, newColor);
                }
            }
        }

        result.Apply();
        return result;
    }


    public static Vector2Int forecastStartPos(Texture2D source)
    {
        Vector2Int vector2Int = Vector2Int.zero;

        for (int i = 0; i < source.width; i += 50)
        {
            for (int j = 0; j < source.height; j += 50)
            {
                if (source.GetPixel(i, j) != Color.clear)
                    return new Vector2Int(i - 50, j - 50);
            }
        }

        return vector2Int;
    }


    public static void SavePng(string path, Texture2D tex, string outPath = "")
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (tex == null)
            return;

        //FileStream fileStream = null;
        //BinaryWriter binaryWriter = null;
        byte[] bytes = null;

        if (string.IsNullOrEmpty(outPath))
            outPath = path;

        if (!Directory.Exists(Path.GetFullPath(outPath + "/../")))
            Directory.CreateDirectory(Path.GetFullPath(outPath + "/../"));

        try
        {
            bytes = tex.EncodeToPNG();
            File.WriteAllBytes(outPath, bytes);
            //fileStream = File.Open(path, FileMode.Create);
            //binaryWriter = new BinaryWriter(fileStream);
            //binaryWriter.Write(bytes);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            //if (fileStream != null)
            //{
            //    fileStream.Close();
            //    fileStream.Dispose();
            //}

            //if (binaryWriter != null)
            //{
            //    binaryWriter.Close();
            //    binaryWriter.Dispose();
            //}

            bytes = null;
        }
    }
}
