using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public enum EmMirror
{
    none = 0,
    hor,
    ver
}

public class ImageTools
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

    public static void CutTexture(string path, int width, int height)
    {
        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.cutImageToolPath, string.Format("{0} {1} {2} {3}", path, path, width, height));
    }


    public static void BlendTexture(string path)
    {
        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.blendImageToolPath, string.Format("{0} {1}", path, path));
    }

    public static void Bmp2Png(string path)
    {
        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.bmp2pngPath, string.Format("{0}\\ {1}\\", path, path));
    }

    public static void ScaleTexture(string path, float proportion, Vector2Int offset, int dir)
    {
        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.scaleImageToolPath, string.Format("{0} {1} {2} {3} {4} {5}", path, path, proportion, offset.x, offset.y, dir));
    }

    public static void ScaleTexture(string path, string outPath, float proportion, Vector2Int offset)
    {
        if (proportion > 0.999f && proportion <= 1 && offset == Vector2Int.zero)
            return;

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

    public static void Mirror(EmMirror mirror, string path)
    {
        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.rotationImageToolPath, string.Format("{0} {1} {2}", path, 0, (int)mirror));
    }


    public static void Rotate(string path, float rotationAngle)
    {
        if (rotationAngle == 0 || rotationAngle == 360)
            return;

        path = path.Replace(@"\", "//");
        CmdUtil.ProcessCommand(Global.rotationImageToolPath, string.Format("{0} {1} {2}", path, (int)rotationAngle, 0));
    }

    public static void Merge(string path)
    {
        byte[] buffer = File.ReadAllBytes(path);
        Texture2D modify = new Texture2D(1, 1);
        modify.LoadImage(buffer);
        modify.Apply();
        Texture2D templete = Resources.Load<Texture2D>("Merge3");

        int offsetX = (modify.width - templete.width) / 2;
        int offsetY = (modify.height - templete.height) / 2;
        for (int i = 0; i < templete.width; i++)
        {
            for (int j = 0; j < templete.height; j++)
            {
                Color color = templete.GetPixel(i, j);
                if (color != Color.black && color != Color.clear)
                {
                    int tempI = offsetX + i;
                    int tempJ = offsetY + j;
                    Color tempColor = modify.GetPixel(tempI, tempJ);
                    float alpha = 1 - color.r + 0.2f;

                    if (tempColor.a < alpha)
                        alpha = tempColor.a;

                    modify.SetPixel(tempI, tempJ, new Color(tempColor.r, tempColor.g, tempColor.b, alpha));
                }
            }

            modify.Apply();
            Utils.SavePng(path, modify);
        }

        GameObject.DestroyImmediate(modify, true);
        modify = null;
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
