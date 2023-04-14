using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameUI : MonoBehaviour
{
    private Toggle frameToggle;
    public int index;
    public RectTransform frameSprite;
    public Action<int> callback;

    private void Awake()
    {
        frameToggle = transform.GetComponent<Toggle>();
    }

    void Start()
    {
        frameToggle.onValueChanged.AddListener(OnFrameValueChanged);
    }

    void Update()
    {
        
    }

    private void OnFrameValueChanged(bool isOn)
    {
        if (isOn)
            callback?.Invoke(index);

        frameSprite?.gameObject.SetActive(isOn);
    }

    public ToggleGroup group
    {
        set
        {
            if (frameToggle == null)
                return;

            frameToggle.group = value;
        }
    }

    public bool isOn
    {
        set 
        {
            if (frameToggle == null)
                return;

            frameToggle.isOn = value;
        }
    }

    public Vector2Int offset
    {
        get
        {
            int offsetX = Mathf.RoundToInt(frameSprite.anchoredPosition.x);
            int offsetY = Mathf.RoundToInt(frameSprite.anchoredPosition.y);
            return new Vector2Int(offsetX, offsetY);
        }
    }

    private void OnDestroy()
    {
    }
}
