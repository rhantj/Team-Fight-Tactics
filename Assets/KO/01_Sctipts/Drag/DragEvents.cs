using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public TestingCube chess;
    Vector3 _worldPos;
    Ray camRay;

    private void Update()
    {
        camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        CalculateWorldPosition(camRay);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CalculateWorldChess(camRay);

    }

    public void OnDrag(PointerEventData eventData)
    {
        chess.SetPosition(_worldPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    void CalculateWorldPosition(Ray ray)
    {
        var ground = new Plane(Vector3.up, Vector3.up);
        if (ground.Raycast(ray, out var hit))
        {
            _worldPos = ray.GetPoint(hit);
        }
    }

    void CalculateWorldChess(Ray ray)
    {
        if(Physics.Raycast(ray, out var hit, 1000f))
        {
            if(hit.transform.TryGetComponent<TestingCube>(out var ch))
            {
                Chess = ch;
            }
        }
    }

    public TestingCube Chess
    {
        get { return chess; }
        set { chess = value; }
    }
}
