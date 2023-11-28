using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MapGroupUI : MonoBehaviour
{
    [HideInInspector]
    public MapDir[] mapTypes;
    private Button delete;

    void Start()
    {
        mapTypes = gameObject.GetComponentsInChildren<MapDir>();
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
            foreach(MapDir type in mapTypes)
            {
                List<string> paths = Utils.GetAllFileList(type.path, ".map");
                if (paths.Count == 0)
                {
                    Notice.ShowNotice(type.path + ", 文件夹下没有.map文件无法输出..", Color.red, 5);
                    continue;
                }

                foreach(string mapPath in paths)
                {
                    mapData.paths.Add(mapPath);
                }

                paths = Utils.GetAllFileList(type.path, ".wzl");
                if (paths.Count == 0)
                {
                    Notice.ShowNotice(type.path + ", 文件夹下没有.wzl文件无法输出..", Color.red, 5);
                    continue;
                }

                foreach(string dataPath in paths)
                {
                    MapResData resData = new MapResData();
                    resData.path = dataPath.Split('.')[0];
                    if (dataPath.ToLower().Contains("objects"))
                    {
                        resData.resType = MapResType.obj;
                    }
                    else if (dataPath.ToLower().Contains("smtiles"))
                    {
                        resData.resType = MapResType.smtiles;
                    }
                    else if (dataPath.ToLower().Contains("tiles"))
                    {
                        resData.resType = MapResType.tiles;
                    }

                    mapData.mapResList.Add(resData);
                }
            }

            return mapData;
        }
    }
}
