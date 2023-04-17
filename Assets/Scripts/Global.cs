using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global
{
    public static List<Color> defaultColors = new List<Color>();
    public static int maxWidth = 2048;
    public static int maxHeight = 2048;

    public static string root = Application.dataPath;
#if UNITY_EDITOR
    public static string rotationImageToolPath = root + "/../Rotate.exe";
#elif UNITY_STANDALONE
    public static string rotationImageToolPath = root + "/../Rotate.exe";
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
