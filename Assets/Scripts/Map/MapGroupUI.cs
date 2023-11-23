using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGroupUI : MonoBehaviour
{
    [HideInInspector]
    public MapType[] mapTypes;
    private Button delete;

    void Start()
    {
        mapTypes = gameObject.GetComponentsInChildren<MapType>();
        delete = transform.Find("map/Delete").GetComponent<Button>();

        delete.onClick.AddListener(OnDelete);
    }

    private void OnDelete()
    {
        Destroy(gameObject);
    }

    public MapData mapData
    {
        get
        {
            MapData mapData = new MapData();
            foreach(MapType type in mapTypes)
            {
                if(type.resType == MapResType.map)
                {
                    mapData.path = type.path;
                }
                else
                {
                    MapResData resData = new MapResData();
                    resData.path = type.path;
                    resData.resType = type.resType;
                    mapData.mapResList.Add(resData);
                }
            }

            return mapData;
        }
    }
}
