using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;

public class Group
{
    public string name;
    public List<Frame> frames = new List<Frame>();
}

public class IrregularUI : MonoBehaviour
{
    private Button exportBtn;
    private Button addGroupBtn;
    private Button descBtn;
    private Button saveBtn;
    private GameObject desc;
    private GameObject content;
    private GameObject templete;
    private ScrollRect groupScrollRect;
    private Coroutine exportCoroutine;
    private MainUI mainUI;
    private int groupIndex = 0;
    private bool refactor = true;

    void Start()
    {
        exportBtn = transform.Find("IrregularExportBtn").GetComponent<Button>();
        addGroupBtn = transform.Find("AddGroupBtn").GetComponent<Button>();
        desc = transform.Find("Desc").gameObject;
        descBtn = transform.Find("DescBtn").GetComponent<Button>();
        saveBtn = transform.Find("SaveBtn").GetComponent<Button>();

        templete = transform.Find("Group").gameObject;
        content = transform.Find("ScrollView/Viewport/Content").gameObject;
        groupScrollRect = transform.Find("ScrollView").GetComponent<ScrollRect>();

        exportBtn.onClick.AddListener(OnExport);
        addGroupBtn.onClick.AddListener(OnAddGroup);
        descBtn.onClick.AddListener(OnDesc);
        saveBtn.onClick.AddListener(OnSave);

        InitGroup();
    }

    private void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
    }

    private void InitGroup()
    {
        if (!File.Exists(Global.frameJsonPath))
            return;

        string frameJson = File.ReadAllText(Global.frameJsonPath);
        List<Group> groups = new List<Group>();
        groups = JsonConvert.DeserializeObject<List<Group>>(frameJson);

        foreach (var group in groups)
        {
            GroupUI groupUI = CreateGroup();
            groupUI.GroupName = group.name;
            groupUI.InitFrames(group.frames);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
        groupScrollRect.verticalNormalizedPosition = 0;
    }

    private void OnSave()
    {
        List<Group> groups = new List<Group>();
        foreach(var vaule in groupUIs)
        {
            Group group = new Group();
            group.name = vaule.GroupName;

            foreach(CreateFrameUI frameUI in vaule.frameUIs)
            {
                group.frames.Add(frameUI.Frame);
            }

            groups.Add(group);
        }

        string frameJson = JsonConvert.SerializeObject(groups);
        File.WriteAllText(Global.frameJsonPath, frameJson);

        Notice.ShowNotice(string.Format("保存成功: {0}", Path.GetFullPath(Global.frameJsonPath)), Color.green, 3);
    }

    private void OnDesc()
    {
        desc.SetActive(!desc.activeSelf);
    }

    private void OnExport()
    {
        if (string.IsNullOrEmpty(mainUI.ModifyPath))
        {
            Notice.ShowNotice("重构路径不能为空", Color.red, 3);
            return;
        }

        if (groupUIs.Count == 0)
        {
            Notice.ShowNotice("组不能为空", Color.red, 3);
            return;
        }

        if (refactor)
        {
            exportCoroutine = StartCoroutine(ExportRes.ExportCustom(mainUI.ModifyPath, curGroup.FrameSet, curGroup.GroupName, curGroup.ShowProgress, OnComoplete));
        }
        else
        {
            StopCoroutine(exportCoroutine);
        }

        refactor = !refactor;
        exportBtn.GetComponent<UpdateBtnText>().text = refactor ? "不规则重构" : "暂停";
    }

    private void OnComoplete()
    {
        groupIndex++;
        refactor = true;
        if (groupIndex >= groupUIs.Count)
        {
            groupIndex = 0;
            exportBtn.GetComponent<UpdateBtnText>().text = refactor ? "不规则重构" : "暂停";
            mainUI.ShowProgress(0, 0);
        }
        else
        {
            OnExport();
        }
    }

    private void OnAddGroup()
    {
        CreateGroup();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
        groupScrollRect.verticalNormalizedPosition = 0;
    }

    private GroupUI CreateGroup()
    {
        GameObject go = Instantiate(templete);
        go.SetActive(true);
        go.transform.SetParent(content.transform, false);

        GroupUI groupUI = go.GetComponent<GroupUI>();
        return groupUI;

    }

    public MainUI MainUI
    {
        set
        {
            this.mainUI = value;
        }
    }

    public GroupUI curGroup
    {
        get
        {
            return groupUIs[groupIndex];
        }
    }

    public List<GroupUI> groupUIs
    {
        get
        {
            List<GroupUI> groups = new List<GroupUI>();
            for (int i = 0; i < content.transform.childCount; i++)
            {
                GroupUI groupUI = content.transform.GetChild(i).GetComponent<GroupUI>();
                groups.Add(groupUI);
            }

            return groups;
        }
    }
}
