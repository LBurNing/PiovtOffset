using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notice
{
    private static NoticeUI _noticeUI;

    private static NoticeUI noticeUI
    {
        get
        {
            if (_noticeUI == null)
            {
                GameObject go = GameObject.Instantiate(LoadAssetHelper.LoadResourcesUI("NoticeUI"));
                _noticeUI = go.GetComponent<NoticeUI>();
            }

            return _noticeUI;
        }
    }


    public static void ShowNotice(string text, Color color, float destroyTime = 2.0f)
    {
        noticeUI.ShowNotice(text, color, destroyTime);
    }
}

public class NoticeUI : MonoBehaviour
{
    public GameObject notice;
    public Transform noticeParent;

    public void Start()
    {
        transform.SetAsLastSibling();
    }

    public void ShowNotice(string text, Color color, float destroyTime = 2.0f)
    {
        GameObject go = Instantiate(notice, noticeParent);
        go.SetActive(true);

        NoticeItem item = go.GetComponent<NoticeItem>();
        item.DestroyTime = destroyTime;
        item.Notice = text;
        item.color = color;
    }
}
