using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class IrregularUI : MonoBehaviour
{
    private Button exportBtn;
    private Button addFrameBtn;
    private GameObject content;
    private GameObject templete;
    private Coroutine exportCoroutine;
    private MainUI mainUI;
    private int exportIndex = 1000;
    private bool refactor = true;

    void Start()
    {
        exportBtn = transform.Find("IrregularExportBtn").GetComponent<Button>();
        addFrameBtn = transform.Find("AddFrameBtn").GetComponent<Button>();
        templete = transform.Find("Frame").gameObject;
        content = transform.Find("ScrollView/Viewport/Content").gameObject;
        exportBtn.onClick.AddListener(OnExport);
        addFrameBtn.onClick.AddListener(OnAddFrame);
    }

    private void OnExport()
    {
        if (refactor)
        {
            ExportRes.Rename(mainUI.ModifyPath);
            exportCoroutine = StartCoroutine(ExportRes.ExportCustom(mainUI.ModifyPath, FrameSet, exportIndex, mainUI.ShowProgress, OnComoplete));
        }
        else
        {
            StopCoroutine(exportCoroutine);
        }

        exportBtn.GetComponent<UpdateBtnText>().text = refactor ? "不规则重构" : "暂停";
        refactor = !refactor;
    }

    private void OnComoplete(int index)
    {
        exportIndex = index;
    }

    private void OnAddFrame()
    {
        GameObject go = Instantiate(templete);
        go.SetActive(true);
  
        go.transform.SetParent(content.transform);
    }

    public MainUI MainUI
    {
        set
        {
            this.mainUI = value;
        }
    }

    public FrameSet FrameSet
    {
        get
        {
            FrameSet frame = new FrameSet();
            for (int i=0;i< content.transform.childCount; i++)
            {
                CreateFrameUI frameUI = content.transform.GetChild(i).GetComponent<CreateFrameUI>();
                frame.Frames.Add(frameUI.mirAction, frameUI.Frame);
            }

            return frame;
        }
    }
}
