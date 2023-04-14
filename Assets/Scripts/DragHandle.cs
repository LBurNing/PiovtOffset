using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// 继承几个接口，用于拖拽
public class DragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 mousePosition;
    private RectTransform rect;

    public Action onBeginDrag;
    public Action onDrag;
    public Action onEndDrag;

    private void Awake()
    {
        rect = transform.GetComponent<RectTransform>();
        if (rect == null)
        {
            throw new Exception("只能拖拽UI物体");
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        mousePosition = Input.mousePosition;
        if (onBeginDrag != null) onBeginDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += (Vector2)(Input.mousePosition - mousePosition);
        mousePosition = Input.mousePosition;
        if (onDrag != null) onDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null) onEndDrag();
    }
}