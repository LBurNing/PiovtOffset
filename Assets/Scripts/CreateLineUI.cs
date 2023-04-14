using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLineUI : MonoBehaviour
{
    private GameObject line;
    private GameObject col;
    private RectTransform thisRect;
    private GameObject templete;
    private Vector2 size;

    private void Awake()
    {
        line = transform.Find("Line").gameObject;
        col = transform.Find("col").gameObject;
        templete = transform.Find("lineTemplete").gameObject;
        thisRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (size == thisRect.sizeDelta)
            return;

        UpdateLine();
    }

    private void UpdateLine()
    {
        line.GetComponent<RectTransform>().sizeDelta = new Vector2(thisRect.sizeDelta.x, thisRect.sizeDelta.y);
        col.GetComponent<RectTransform>().sizeDelta = new Vector2(thisRect.sizeDelta.y, thisRect.sizeDelta.x);
        int lineCount = Mathf.RoundToInt(thisRect.sizeDelta.y / 10);
        int rowCount = Mathf.RoundToInt(thisRect.sizeDelta.x / 10);

        for(int i = 0; i < line.transform.childCount; i++)
        {
            Destroy(line.transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < col.transform.childCount; i++)
        {
            Destroy(col.transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < lineCount; i++)
        {
            GameObject lineGo = Instantiate(templete, line.transform);
            lineGo.SetActive(true);
        }

        for (int i = 0; i < rowCount; i++)
        {
            GameObject rowGo = Instantiate(templete, col.transform);
            rowGo.SetActive(true);
        }

        size = thisRect.sizeDelta;
    }
}
