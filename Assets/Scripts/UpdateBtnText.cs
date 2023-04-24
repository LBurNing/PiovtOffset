using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateBtnText : MonoBehaviour
{
    public Text textCom;
    public GameObject Go;

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

    public Vector3 scale
    {
        set
        {
            if (Go == null) return;
            Go.transform.localScale = value;
        }

        get
        {
            if (Go == null) 
                return Vector3.one;

            return Go.transform.localScale;
        }
    }
}
