using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapResType
{
    map,
    obj,
    smtiles,
    tiles
}

public class MapDir : MonoBehaviour
{

    public string path
    {
        get
        {
            return GetComponent<InputField>().text;
        }
    }
}
