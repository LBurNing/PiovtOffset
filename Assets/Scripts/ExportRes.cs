using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum EnumResType
{
    NeiGuan,
    Player,
    Weapon,
    Wing,
}

public class ResData
{
    public string resPath;

    public ResData(string _resPath)
    {
        resPath = _resPath;
    }
}

public class ExportRes
{
    private static Dictionary<bool, string> genderFileNames = new Dictionary<bool, string>
    {
        [false] = "��ģ",
        [true] = "Ůģ"
    };


    private static Dictionary<MirAction, string> eToc = new Dictionary<MirAction, string>
    {
        [MirAction.NeiGuan] = "����",
        [MirAction.NeiGuan1] = "����1",
        [MirAction.NeiGuan2] = "����2",
        [MirAction.NeiGuan3] = "����3",
        [MirAction.NeiGuan4] = "����4",
        [MirAction.NeiGuan5] = "����5",
        [MirAction.NeiGuan6] = "����6",
        [MirAction.NeiGuan7] = "����7",
        [MirAction.NeiGuan8] = "����8",
        [MirAction.NeiGuan9] = "����9",
        [MirAction.NeiGuan10] = "����10",
        [MirAction.NeiGuan11] = "����11",
        [MirAction.NeiGuan12] = "����12",
        [MirAction.NeiGuan13] = "����13",
        [MirAction.NeiGuan14] = "����14",
        [MirAction.NeiGuan15] = "����15",
        [MirAction.NeiGuan16] = "����16",
        [MirAction.NeiGuan17] = "����17",
        [MirAction.NeiGuan18] = "����18",
        [MirAction.NeiGuan19] = "����19",
        [MirAction.ShiFa] = "ʩ��",
        [MirAction.Attack1] = "����",
        [MirAction.Attack2] = "����1",
        [MirAction.Attack3] = "����2",
        [MirAction.Attack4] = "����3",
        [MirAction.Attack5] = "����4",
        [MirAction.BeiJi] = "����",
        [MirAction.BeiZhan] = "��ս",
        [MirAction.Die] = "����",
        [MirAction.Die1] = "����1",
        [MirAction.Running] = "�ܲ�",
        [MirAction.ShiQu] = "ʰȡ",
        [MirAction.Standing] = "����",
        [MirAction.Walking] = "��·",
        [MirAction.Standing1] = "����1",
        [MirAction.Standing2] = "����2",
    };

    public static float frameTime = 0.01f;

    public static IEnumerator ExportCustom(string sourcePath, FrameSet frameSet, string groupName, Action<int, int, string> progress = null, Action complete = null)
    {
        yield return new WaitForSeconds(0.2f);
        if (string.IsNullOrEmpty(sourcePath))
        {
            progress(0, 0, "��Դ·������Ϊ��");
            complete?.Invoke();
            yield break;
        }

        if (!Directory.Exists(sourcePath))
        {
            progress(0, 0, "��Դ·��������");
            complete?.Invoke();
            yield break;
        }

        if (string.IsNullOrEmpty(groupName))
        {
            progress(0, 0, "��������Ϊ��");
            complete?.Invoke();
            yield break;
        }

        Rename(sourcePath);

        int resIndex = 0;
        Dictionary<string, Frame> frames = frameSet.Name2Frames;
        float alphaSize = 0.9f;
        float addColor = 10;

        int maxCount = Utils.FindFiles(sourcePath, "*.png").Count;
        BmpData bmpData = new BmpData();

        foreach (var value in frames)
        {
            string actionName = value.Key;
            string outPngParentPath = sourcePath + "/../�������PNG��Դ/" + Path.GetFileName(sourcePath) + "/" + groupName + @"\" + actionName + @"\";

            if (Directory.Exists(outPngParentPath))
            {
                progress(0, 0, "�����ظ�");
                complete?.Invoke();
                yield break;
            }
            else
            {
                Directory.CreateDirectory(outPngParentPath);
            }

            int frameCount = 0;
            Frame frame = value.Value;
            for (byte Direction = (byte)EnumDirection.Up; Direction <= (byte)frame.direction; Direction++)
            {
                int actionIndex = 10000 * (Direction + frame.Dir);
                for (int FrameIndex = 0; FrameIndex < frame.Count; FrameIndex++)
                {
                    resIndex++;
                    int Frame = frame.Start + (frame.OffSet * Direction) + FrameIndex;
                    string bmpFilePath = sourcePath + @"\" + Frame + @".bmp";
                    if (!File.Exists(bmpFilePath))
                    {
                        bmpFilePath = sourcePath + @"\" + Frame + @".png";
                    }

                    string offsetPath = sourcePath + @"\Placements\" + Frame + @".txt";

                    string pngPath = outPngParentPath + actionIndex + @".png";

                    if (!File.Exists(pngPath) && File.Exists(bmpFilePath))
                    {
                        bmpData.bmpPath = bmpFilePath;
                        bmpData.pngPath = pngPath;
                        bmpData.offsetPath = offsetPath;
                        bmpData.effectAlpha = frame.EffectAlpha;
                        bmpData.effectAlphaSize = alphaSize;
                        bmpData.effectAddColor = addColor;

                        bmpData.alpha = frame.Alpha;
                        bmpData.alphaSize = alphaSize;
                        bmpData.addColor = addColor;
                        BmpToPng.BmpToPngAction(bmpData);

                        progress?.Invoke(resIndex, frameSet.Count, null);
                        yield return new WaitForSeconds(frameTime);
                    }

                    frameCount++;
                    actionIndex++;
                }

                if (frameCount >= frame.MaxCount)
                    break;
            }
        }

        progress?.Invoke(frameSet.Count, frameSet.Count, null);
        complete?.Invoke();
        yield return null;
    }

    public static IEnumerator Export(string sourcePath, Vector2Int pivot = new Vector2Int(), int indexOffset = 0, Action<int, int> progress = null)
    {
        int maxCount = Utils.FindFiles(sourcePath, "*.png").Count;
        //if(maxCount > 100)
        //{
        //    progress?.Invoke(0, maxCount);
        //    Debug.LogError("���ļ��в���ʹ���ڹ����͵���");
        //    yield break;
        //}

        Dictionary<MirAction, Frame> frames = FrameSet.NeiGuan.Frames;
        float alphaSize = 0.9f;
        float addColor = 10;
        int resIndex = 0;

        BmpData bmpData = new BmpData();

        foreach (var value in frames)
        {
            string outPngParentPath = sourcePath + @"\����\";
            if (!Directory.Exists(outPngParentPath))
                Directory.CreateDirectory(outPngParentPath);

            int frameCount = 0;
            Frame frame = value.Value;
            for (byte Direction = (byte)EnumDirection.Up; Direction <= (byte)frame.direction; Direction++)
            {
                int actionIndex = 10000 * (Direction + 1) + indexOffset;
                for (int FrameIndex = 0; FrameIndex < frame.Count; FrameIndex++)
                {
                    resIndex++;
                    int Frame = frame.Start + (frame.OffSet * Direction) + FrameIndex;
                    string bmpFilePath = sourcePath + @"\" + Frame + @".bmp";
                    if (!File.Exists(bmpFilePath))
                    {
                        bmpFilePath = sourcePath + @"\" + Frame + @".png";
                    }

                    string offsetPath = sourcePath + @"\Placements\" + Frame + @".txt";
                    string pngPath = outPngParentPath + actionIndex + @".png";

                    if ((!File.Exists(pngPath)) && File.Exists(bmpFilePath))
                    {
                        bmpData.bmpPath = bmpFilePath;
                        bmpData.pngPath = pngPath;
                        bmpData.offsetPath = offsetPath;
                        bmpData.effectAlpha = frame.EffectAlpha;
                        bmpData.effectAlphaSize = alphaSize;
                        bmpData.effectAddColor = addColor;

                        bmpData.alpha = frame.Alpha;
                        bmpData.alphaSize = alphaSize;
                        bmpData.addColor = addColor;
                        bmpData.pivot = pivot;
                        BmpToPng.BmpToPngAction(bmpData);

                        progress?.Invoke(resIndex, maxCount);
                        yield return new WaitForSeconds(frameTime);
                    }

                    frameCount++;
                    actionIndex++;
                }

                if (frameCount >= frame.MaxCount)
                    break;
            }
        }

        progress?.Invoke(maxCount, maxCount);
        yield return null;
    }

    public static IEnumerator ExportType(EnumResType enumResType, ResData resData, Action<int, int> progress = null)
    {
        string sourcePath = resData.resPath;
        Dictionary<MirAction, Frame> playerFrames = FrameSet.Players.Frames;
        int maxCount = Utils.FindFiles(sourcePath, "*.png").Count;
        if (enumResType == EnumResType.NeiGuan)
        {
            progress?.Invoke(0, maxCount);
            Debug.LogError("����ʹ���ڹ����͵������ļ���");
            yield break;
        }


        bool alpha = false;
        float alphaSize = 0;
        float addColor = 0;

        if (enumResType == EnumResType.Wing)
        {
            alpha = true;
            alphaSize = 0.9f;
            addColor = 5;
            playerFrames = FrameSet.Wings.Frames;
        }

        int resMaxFrame = FrameSet.Players.resMaxFrame;
        if (enumResType == EnumResType.Wing)
            resMaxFrame = FrameSet.Wings.resMaxFrame;

        int resIndex = 0;
        bool gender = false; //0�� 1Ů
        int maleResId = 1000;
        int femaleRedId = 999;
        int startFrameIndex = 0;
        string genderFileName = "";
        bool end = false;

        BmpData bmpData = new BmpData();

        while (!end)
        {
            genderFileName = genderFileNames[gender] + @"\";
            foreach (var value in playerFrames)
            {
                string actionName = eToc[value.Key];
                int index = gender ? femaleRedId : maleResId;
                string outPngParentPath = resData.resPath + "/../�������PNG��Դ/" + Path.GetFileName(resData.resPath) + "/" + genderFileName + @"\" + index + @"\" + actionName + @"\";

                if (!Directory.Exists(outPngParentPath))
                    Directory.CreateDirectory(outPngParentPath);

                Frame frame = value.Value;
                for (byte Direction = (byte)EnumDirection.Up; Direction <= (byte)EnumDirection.UpLeft; Direction++)
                {
                    int actionIndex = 10000 * (Direction + 1);
                    for (int FrameIndex = 0; FrameIndex < frame.Count; FrameIndex++)
                    {
                        resIndex++;

                        int Frame = frame.Start + (frame.OffSet * Direction) + FrameIndex + startFrameIndex;
                        string bmpFilePath = sourcePath + @"\" + Frame + @".bmp";
                        string offsetPath = sourcePath + @"\Placements\" + Frame + @".txt";
                        if (!File.Exists(bmpFilePath))
                        {
                            bmpFilePath = sourcePath + @"\" + Frame + @".png";
                        }

                        if (!File.Exists(bmpFilePath))
                        {
                            end = true;
                            break;
                        }

                        string pngPath = outPngParentPath + actionIndex + @".png";

                        if (!File.Exists(pngPath) && File.Exists(bmpFilePath))
                        {
                            bmpData.bmpPath = bmpFilePath;
                            bmpData.pngPath = pngPath;
                            bmpData.offsetPath = offsetPath;
                            bmpData.effectAlpha = alpha;
                            bmpData.effectAlphaSize = alphaSize;
                            bmpData.effectAddColor = addColor;
                            BmpToPng.BmpToPngAction(bmpData);

                            progress?.Invoke(resIndex, maxCount);
                            yield return new WaitForSeconds(frameTime);
                        }

                        actionIndex++;
                    }

                    if (end)
                        break;
                }

                if (end)
                    break;
            }

            if (resIndex >= maxCount)
                break;

            gender = !gender;
            if (gender)
            {
                femaleRedId++;
            }
            else
            {
                maleResId++;
            }

            startFrameIndex += resMaxFrame;
            end = startFrameIndex >= maxCount;
        }

        progress?.Invoke(maxCount, maxCount);
        yield return null;
    }

    public static void Rename(string sourcePath)
    {
        int index = 0;
        List<string> pointPaths = Utils.GetAllFileList(sourcePath, ".txt");
        if (pointPaths.Count == 0)
        {
            List<string> imagePaths = Utils.GetAllFileList(sourcePath, ".png");
            if (imagePaths.Count == 0)
                return;

            foreach (string filePath in imagePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
                string bmpParentPath = directoryInfo.Parent.FullName;

                string path = bmpParentPath + @"\" + fileName + @".png";
                string newPath = bmpParentPath + @"\" + index + @".png";

                if (!File.Exists(newPath))
                    File.Move(path, newPath);

                index++;
            }

            return;
        }

        foreach (string filePath in pointPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
            string txtParentPath = directoryInfo.Parent.FullName;
            string txtPath = txtParentPath + @"\" + index + @".txt";

            if (!File.Exists(txtPath))
                File.Move(filePath, txtPath);

            string bmpParentPath = directoryInfo.Parent.Parent.FullName;
            string path = bmpParentPath + @"\" + fileName + @".bmp";
            string newPath = bmpParentPath + @"\" + index + @".bmp";

            if (!File.Exists(path))
            {
                path = bmpParentPath + @"\" + fileName + @".png";
                newPath = bmpParentPath + @"\" + index + @".png";
            }

            if (!File.Exists(path))
                continue;

            if (!File.Exists(newPath))
                File.Move(path, newPath);

            index++;
        }
    }
}
