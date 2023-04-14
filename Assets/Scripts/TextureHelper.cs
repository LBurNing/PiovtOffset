using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ThreadData
{
    public int minX = int.MaxValue;
    public int minY = int.MaxValue;
    public int maxX = 0;
    public int maxY = 0;
    public Color32[] pixels;
    public AutoResetEvent finishedEvent;
}

public class TextureHelper
{
    public static int MAX_TEXTURE_SIZE = 2048;
    private static List<Color> _defaultColors = new List<Color>();

    public static Vector2Int CalculateBoundingBox(Texture2D texture)
    {
        int numThreads = Environment.ProcessorCount;
        int width = texture.width;
        int height = texture.height;
        Color32[] pixels = texture.GetPixels32();

        // Split the pixels into multiple tasks
        int numPixelsPerTask = (height + numThreads - 1) / numThreads;
        List<ThreadData> threadDataList = new List<ThreadData>();
        for (int i = 0; i < numThreads; i++)
        {
            ThreadData threadData = new ThreadData();
            threadData.pixels = pixels;
            threadData.finishedEvent = new AutoResetEvent(false);
            int startY = i * numPixelsPerTask;
            int endY = Mathf.Min(startY + numPixelsPerTask, height);
            ThreadPool.QueueUserWorkItem(CalculateBoundingBoxThread, new object[] { threadData, width, startY, endY });
            threadDataList.Add(threadData);
        }

        // Wait for all threads to finish
        foreach (ThreadData threadData in threadDataList)
        {
            threadData.finishedEvent.WaitOne();
        }

        // Calculate the final bounding box
        int minX = width;
        int minY = height;
        int maxX = 0;
        int maxY = 0;
        foreach (ThreadData threadData in threadDataList)
        {
            if (threadData.minX < minX)
            {
                minX = threadData.minX;
            }
            if (threadData.minY < minY)
            {
                minY = threadData.minY;
            }
            if (threadData.maxX > maxX)
            {
                maxX = threadData.maxX;
            }
            if (threadData.maxY > maxY)
            {
                maxY = threadData.maxY;
            }
        }
        return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
    }

    private static void CalculateBoundingBoxThread(object state)
    {
        object[] stateArray = (object[])state;
        ThreadData threadData = (ThreadData)stateArray[0];
        int width = (int)stateArray[1];
        int startY = (int)stateArray[2];
        int endY = (int)stateArray[3];

        for (int y = startY; y < endY; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (threadData.pixels[x + y * width].a > 0) // alpha 自己定义是否为背景颜色。
                {
                    if (x < threadData.minX)
                    {
                        threadData.minX = x;
                    }
                    else if (x > threadData.maxX)
                    {
                        threadData.maxX = x;
                    }

                    if (y < threadData.minY)
                    {
                        threadData.minY = y;
                    }
                    else if (y > threadData.maxY)
                    {
                        threadData.maxY = y;
                    }
                }
            }
        }

        threadData.finishedEvent.Set();
    }

    public static Vector2Int GetImageSize(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        Color32[] pixels = texture.GetPixels32();

        int minX = width;
        int minY = height;
        int maxX = 0, maxY = 0;

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (pixels[x + y * width].a > 0) // alpha 自己定义是否为背景颜色。
                {
                    if (x < minX)
                    {
                        minX = x;
                    }
                    else if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (y < minY)
                    {
                        minY = y;
                    }
                    else if (y > maxY)
                    {
                        maxY = y;
                    }
                }
            }
        }

        Vector2Int vector2Int = new Vector2Int(maxX - minX + 1, maxY - minY + 1);
        return vector2Int;
    }

    public static void SavePng(string path, Texture2D tex)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (tex == null)
            return;

        FileStream fileStream = null;
        BinaryWriter binaryWriter = null;
        byte[] bytes = null;

        try
        {
            bytes = tex.EncodeToPNG();
            fileStream = File.Open(path, FileMode.Create);
            binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(bytes);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }

            if (binaryWriter != null)
            {
                binaryWriter.Close();
            }

            bytes = null;
        }
    }

    public static Color[] defaultColors
    {
        get
        {
            if (_defaultColors.Count > 0)
                return _defaultColors.ToArray();

            for (int i = 0; i < MAX_TEXTURE_SIZE; i++)
            {
                for (int j = 0; j < MAX_TEXTURE_SIZE; j++)
                {
                    Color color = new Color(0, 0, 0, 0);
                    _defaultColors.Add(color);
                }
            }

            return _defaultColors.ToArray();
        }
    }
}
