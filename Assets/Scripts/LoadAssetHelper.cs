using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadAssetHelper
{
    public static List<string> GetFilePaths(string path)
    {
        List<Texture2D> texture2Ds = new List<Texture2D>();
        List<string> filePaths = Utils.GetAllFileList(path, ".png");
        return filePaths;
    }

    public static List<Texture2D> Load(string path, int maxFrame = 0)
    {
        List<Texture2D> texture2Ds = new List<Texture2D>();
        List<string> filePaths = Utils.GetAllFileList(path, ".png");
        if (maxFrame == 0) maxFrame = filePaths.Count;

        for (int i = 0; i < maxFrame; i++)
        {
            string filePath = filePaths[i];
            byte[] buffer = File.ReadAllBytes(filePath);
            Texture2D t2D = new Texture2D(1, 1);
            t2D.name = Path.GetFileName(filePath);
            t2D.LoadImage(buffer);
            t2D.Apply();

            texture2Ds.Add(t2D);
        }

        return texture2Ds;
    }

    public static Texture2D LoadTex(string filePath)
    {
        byte[] buffer = File.ReadAllBytes(filePath);
        Texture2D t2D = new Texture2D(1, 1);
        t2D.LoadImage(buffer);
        t2D.Apply();

        return t2D;
    }

    public static void Dispose(List<Texture2D> texture2Ds)
    {
        if (texture2Ds == null)
            return;

        foreach (Texture2D t2D in texture2Ds)
        {
            GameObject.DestroyImmediate(t2D, true);
        }

        texture2Ds.Clear();
    }

    public static GameObject LoadResourcesUI(string path)
    {
        return Resources.Load<GameObject>(path);
    }
}
