using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeItem : MonoBehaviour
{
    public Text notice;
    private float destrotyTime = 1.0f;

    public float DestroyTime
    {
        set
        {
            destrotyTime = value;
        }
    }

    public string Notice
    {
        set
        {
            notice.text = value;
            StartCoroutine(AutoDestroy());
        }
    }

    public Color color
    {
        set
        {
            notice.color = value;
        }
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(destrotyTime);
        DestroyImmediate(gameObject);
    }
}
