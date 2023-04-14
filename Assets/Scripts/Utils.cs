using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
public class TextureSize
{
    public int width;
    public int height;
    public int buffer;
}

public class Utils
{
    public static List<string> GetAllFileList(string path, string mark = ".cs", string mark1 = "")
    {
        List<string> files = new List<string>();
        string[] paths = Directory.GetFiles(path);
        foreach (string _p in paths)
        {
            if (_p.ToLower().EndsWith(mark.ToLower()) || (mark1 != "" && _p.ToLower().EndsWith(mark1.ToLower())))
                //if (_p.ToLower().EndsWith(mark.ToLower()) || (String.IsNullOrEmpty(mark1) && _p.ToLower().EndsWith(mark1.ToLower())))
                files.Add(_p);
        }
        string[] dirs = Directory.GetDirectories(path);
        foreach (string _d in dirs)
        {
            files.AddRange(GetAllFiles(_d, mark));
        }
        return files;
    }

    public static string[] GetAllFiles(string path, string mark = ".cs", string mark1 = "")
    {
        List<string> files = new List<string>();
        string[] paths = Directory.GetFiles(path);
        foreach (string _p in paths)
        {
            if (_p.ToLower().EndsWith(mark.ToLower()) || (mark1 != "" && _p.ToLower().EndsWith(mark1.ToLower())))
                //if (_p.ToLower().EndsWith(mark.ToLower()) || (String.IsNullOrEmpty(mark1) && _p.ToLower().EndsWith(mark1.ToLower())))
                files.Add(_p);
        }
        string[] dirs = Directory.GetDirectories(path);
        foreach (string _d in dirs)
        {
            files.AddRange(GetAllFiles(_d, mark));
        }
        return files.ToArray();
    }

    public static string[] GetAllFilesWithSubDir(string path)
    {
        List<string> files = new List<string>();
        string[] paths = Directory.GetFiles(path);
        foreach (string _p in paths)
        {
            files.Add(_p);
        }

        string[] dirs = Directory.GetDirectories(path);
        foreach (string _dir in dirs)
        {
            string[] dirFiles = GetAllFilesWithSubDir(_dir);
            foreach (string _p in dirFiles)
            {
                files.Add(_p);
            }
        }

        return files.ToArray();
    }

    public static void DeleteFolder(string dir)
    {
        if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                    File.Delete(d); //直接删除其中的文件 
                else
                    DeleteFolder(d); //递归删除子文件夹 
            }

            Directory.Delete(dir, true); //删除已空文件夹 
        }
    }

    public static void ExeCommand(string cmd)
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = false;//true表示不显示黑框，false表示显示dos界面
        try
        {
            p.Start();
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");

            p.Close();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public static string ArtworkPath2SystemPath(string path)
    {
        path = path.Replace("\\", "/");
        var p = Application.dataPath + "/" + path;
        return p.Replace("Assets/Assets", "Assets");
    }

    public static string SystemPath2ArtworkPath(string path)
    {
        path = path.Replace("\\", "/");
        return path.Replace(Application.dataPath, "Assets");
    }

    public static string GetPathParent(string path)
    {
        path = path.Replace("\\", "/");
        return path.Substring(0, path.LastIndexOf("/") + 1);
    }

    public static string GetPathNameExt(string path)
    {
        path = path.Replace("\\", "/");
        return path.Substring(path.LastIndexOf("/") + 1);
    }

    public static string GetPathName(string path)
    {
        var fname = GetPathNameExt(path);
        return fname.Substring(0, fname.IndexOf("."));
    }

    static readonly int MAX_DYNAMIC_BATCH_VERTEX_ATTR = 900;

    // check http://docs.unity3d.com/Manual/DrawCallBatching.html
    public static bool CheckDynamicBatchCondition(GameObject go, out string error, bool checkScale = true, bool autoAdjustScale = false)
    {
        error = null;
        if (go.isStatic)
        {
            return true;
        }

        List<string> errors = new List<string>();

        SkinnedMeshRenderer[] skinnedMeshRenders = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (skinnedMeshRenders.Length > 0)
        {
            string[] names = new string[skinnedMeshRenders.Length];
            for (int i = 0; i < skinnedMeshRenders.Length; ++i)
            {
                names[i] = skinnedMeshRenders[i].name;
            }

            errors.Add(string.Format("{0} 包含骨骼动画, 不能被动态合并", go.name));
        }

        ParticleSystem[] particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        float errorness = Vector3.kEpsilon;

        for (int i = 0; i < particleSystems.Length; ++i)
        {
            if (checkScale)
            {
                Vector3 worldScale = particleSystems[i].transform.lossyScale;
                float diffXZ = Mathf.Abs(worldScale.x - worldScale.z);
                float diffYZ = Mathf.Abs(worldScale.y - worldScale.z);
                float diffXY = Mathf.Abs(worldScale.x - worldScale.y);

                if (diffXZ <= errorness && diffYZ <= errorness && diffXY <= errorness)
                {
                    if (autoAdjustScale)
                    {
                        Vector3 localScale = particleSystems[i].transform.localScale;
                        localScale.y += 0.01f;
                        particleSystems[i].transform.localScale = localScale;

                        errors.Add(string.Format("{0} 是等比例缩放, 可能不被动态合并, 已微调至非等比例缩放", particleSystems[i].name));
                    }
                    else
                    {
                        errors.Add(string.Format("{0} 是一致的缩放, 可能不被动态合并", particleSystems[i].name));
                    }
                }
            }
        }

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; ++i)
        {
            MeshFilter meshFilter = meshFilters[i];
            Mesh mesh = meshFilter.sharedMesh;
            MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();
            if (!mesh || !renderer)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;

            if (mesh.subMeshCount < materials.Length)
            {
                errors.Add(string.Format("{0} 单个子物件会被渲染多次", renderer.name));
            }

            // TODO: get attribute count
            int attributeCount = 3;
            if (attributeCount * mesh.vertexCount > MAX_DYNAMIC_BATCH_VERTEX_ATTR)
            {
                errors.Add(string.Format("{0} 超过了{1}个顶点属性", renderer.name, MAX_DYNAMIC_BATCH_VERTEX_ATTR));
            }

            if (renderer.receiveShadows)
            {
                errors.Add(string.Format("{0} 接受阴影", renderer.name));
            }
        }

        if (errors.Count > 0)
        {
            error = go.name + " 不能被动态合并批次:\r\n\t" + string.Join("\r\n\t", errors.ToArray());
        }

        return string.IsNullOrEmpty(error);
    }

    private static TextureSize textureSize = new TextureSize();

    public static Texture2D PngToTexture2D(string path)
    {
        byte[] buffer = File.ReadAllBytes(path);

        Texture2D t2D = new Texture2D(1, 1);
        t2D.LoadImage(buffer);
        t2D.Apply();

        return t2D;
    }

    public static TextureSize GetPngWidthHeight(string path)
    {
        byte[] buffer = File.ReadAllBytes(path);
        Texture2D texture2D = new Texture2D(100, 100);
        texture2D.LoadImage(buffer);
        textureSize = new TextureSize();
        textureSize.width = texture2D.width;
        textureSize.height = texture2D.height;
        textureSize.buffer = buffer.Length;
        GameObject.DestroyImmediate(texture2D);
        return textureSize;
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
            UnityEngine.Debug.LogError(e.Message);
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
                binaryWriter.Dispose();
            }

            bytes = null;
        }
    }

    public static List<string> FindFiles(string path, string filter = "*")
    {
        List<string> filePaths = new List<string>();
        if (Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                filePaths.Add(files[i].FullName);
            }
        }

        return filePaths;
    }
}
