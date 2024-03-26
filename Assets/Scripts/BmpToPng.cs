using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Net;
using System.Text;

public class Offset
{
    public int x;
    public int y;

    public bool IsEmpty()
    {
        return x == 0 && y == 0;
    }
}

public class BmpData
{
    public string bmpPath;
    public string pngPath;
    public string offsetPath;
    public bool alpha;
    public float alphaSize = 0;
    public float addColor = 0;

    public bool effectAlpha;
    public float effectAlphaSize = 0;
    public float effectAddColor = 0;
    public Vector2Int pivot;
}

public class BmpToPng
{
    public static void BmpToPngAction(BmpData bmpData)
    {
        Global.InitTransparentColor();
        Offset offset = new Offset();
        if (File.Exists(bmpData.offsetPath))
        {
            string text = File.ReadAllText(bmpData.offsetPath);
            string[] offsets = text.Split(new char[2] { '\r', '\n' });

            offset.x = int.Parse(offsets[0]);
            offset.y = int.Parse(offsets[2]);
        }

        Texture2D t2D = Utils.PngToTexture2D(bmpData.bmpPath);
        #region 重心偏移
        int offsetx = -25 + bmpData.pivot.x;
        int offsety = 19 + bmpData.pivot.y;

        int width = t2D.width * 2 + Math.Abs(offset.x) + Math.Abs(offsetx) + 100;
        int height = t2D.height * 2 + Math.Abs(offset.y) + Math.Abs(offsety) + 100;
        #endregion

        #region 起始点计算
        //该资源坐标系左上角为0,0点
        //unity中左下角为0,0点

        //x起始位置计算大图的中心点+offset.x即可
        //y起始位置计算 由于资源的坐标0,0和unity中的0,0 y轴向相反 图片的高度+offset.y 大图的中心点-减去偏移值
        int startX = width / 2 + offset.x + offsetx;
        int StartY = height / 2 - (t2D.height + offset.y) + offsety;
        #endregion

        //放到一张大图上
        Texture2D pngTexture = new Texture2D(width, height);

        pngTexture.SetPixels(Global.defaultColors.ToArray());

        for (int colori = 0; colori < t2D.width; colori++)
        {
            for (int colorj = 0; colorj < t2D.height; colorj++)
            {
                Color color = t2D.GetPixel(colori, colorj);
                if (color == Color.clear)
                    continue;

                float r = color.r;
                float g = color.g;
                float b = color.b;
                float a = color.a;

                if (r == 0 && g == 0 && b == 0)
                {
                    a = 0;
                }

                color = new Color(r, g, b, a);
                pngTexture.SetPixel(startX + colori, StartY + colorj, color);
            }
        }

        pngTexture.Apply();
        Utils.SavePng(bmpData.pngPath, pngTexture);

        UnityEngine.Object.DestroyImmediate(t2D, true);
        UnityEngine.Object.DestroyImmediate(pngTexture, true);
    }

    static byte Max(params byte[] values)
    {
        if (values == null || values.Length == 0)
            return 0;
        var max = values[0];
        for (var i = 1; i < values.Length; ++i)
        {
            max = Math.Max(max, values[i]);
        }
        return max;
    }

    static byte Min(params byte[] values)
    {
        if (values == null || values.Length == 0)
            return 0;
        var min = values[0];
        for (var i = 1; i < values.Length; ++i)
        {
            min = Math.Min(min, values[i]);
        }
        return min;
    }
}
