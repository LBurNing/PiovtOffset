using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global
{
    public static List<Color> defaultColors = new List<Color>();
    public static int maxWidth = 4096;
    public static int maxHeight = 4096;

    public static string root = Application.dataPath;

#if UNITY_EDITOR
    public static string rotationImageToolPath = root + "/../Tools/Rotate.exe";
    public static string scaleImageToolPath = root + "/../Tools/Scale.exe";
    public static string blendImageToolPath = root + "/../Tools/Blend.exe";
    public static string cutImageToolPath = root + "/../Tools/Cut.exe";
    public static string bmp2pngPath = root + "/../Tools/Bmp2Png.exe";
    public static string frameJsonPath = root + "/../frame.json";
    public static string pList2Png = root + "/../Tools/UnTP.exe";
    public static string colorToBlack = root + "/../Tools/205colorToblack.exe";
    public static string txtFormat = root + "/../Tools/TxtFormat.exe";
#elif UNITY_STANDALONE
    public static string rotationImageToolPath = root + "/../Tools/Rotate.exe";
    public static string scaleImageToolPath = root + "/../Tools/Scale.exe";
    public static string blendImageToolPath = root + "/../Tools/Blend.exe";
    public static string cutImageToolPath = root + "/../Tools/Cut.exe";
    public static string bmp2pngPath = root + "/../Tools/Bmp2Png.exe";
    public static string frameJsonPath = root + "/../frame.json";
    public static string pList2Png = root + "/../Tools/UnTP.exe";
    public static string colorToBlack = root + "/../Tools/205colorToblack.exe";
    public static string txtFormat = root + "/../Tools/TxtFormat.exe";
#endif

    public static void InitTransparentColor()
    {
        if (defaultColors.Count > 0)
            return;

        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                Color color = new Color(0, 0, 0, 0);
                defaultColors.Add(color);
            }
        }
    }
}
