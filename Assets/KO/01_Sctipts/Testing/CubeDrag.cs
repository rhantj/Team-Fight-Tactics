using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubeDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        var camPos = Camera.main;
        float dist = Vector3.Dot(Vector3.zero - camPos.transform.position, camPos.transform.forward);

        mousePos.z = dist;
        Vector3 world = camPos.ScreenToWorldPoint(mousePos);

        Debug.Log(world);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
