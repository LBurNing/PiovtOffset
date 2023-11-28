using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSettingsUI : MonoBehaviour
{
    private Button exportBtn;
    private Button addBtn;
    private GameObject content;
    private GameObject templete;
    private MainUI mainUI;
    private InputField outPath;

    void Start()
    {
        exportBtn = transform.Find("ExportBtn").GetComponent<Button>();
        addBtn = transform.Find("AddBtn").GetComponent<Button>();
        templete = transform.Find("Group").gameObject;
        content = transform.Find("ScrollView/Viewport/Content").gameObject;
        outPath = transform.Find("ScrollView/Viewport/Content/out").GetComponent<InputField>();

        exportBtn.onClick.AddListener(OnExport);
        addBtn.onClick.AddListener(OnAddMapSetting);
    }

    private void OnExport()
    {
        InitData();
        StartCoroutine(MapTools.ReadMapData(mainUI.ShowProgress));
    }

    private void OnAddMapSetting()
    {
        GameObject go = Instantiate(templete);
        go.SetActive(true);
        go.transform.SetParent(content.transform, false);
    }

    private void InitData()
    {
        MapTools.mapdatas.Clear();
        MapGroupUI[] mapGroups = gameObject.GetComponentsInChildren<MapGroupUI>();
        foreach (MapGroupUI mapGroupUI in mapGroups)
        {
            MapData mapData = mapGroupUI.mapData;
            mapData.outPath = outPath.text;
            MapTools.mapdatas.Add(mapData);
        }
    }

    public MainUI MainUI
    {
        set
        {
            this.mainUI = value;
        }
    }
}
