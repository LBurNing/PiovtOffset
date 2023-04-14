using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateBtnText : MonoBehaviour
{
    public Text textCom;

    void Awake()
    {
        if (textCom == null)
        {
            Transform tf = transform.Find("Text");
            textCom = tf.GetComponent<Text>();
        }
    }

    public string text
    {
        set
        {
            if (textCom == null) return;
            textCom.text = value;
        }
    }
}
