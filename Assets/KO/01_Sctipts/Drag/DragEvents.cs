using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] GridDivideBase[] grids;
    TestingCube chess;
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
        var ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out var hit))
        {
            var pos = ray.GetPoint(hit);

            var targetGrid = FindGrid(pos);
            if (targetGrid) _worldPos = targetGrid.GetNearGridPosition(pos);
            else _worldPos = pos;
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

    GridDivideBase FindGrid(Vector3 pos)
    {
        GridDivideBase grid = null;
        float dist = float.PositiveInfinity;

        if (grids.Length == 0) return null;

        foreach(var g in grids)
        {
            if (!g) continue;
            if (g.IsContainPos(pos))
            {
                float closest = (pos - g.transform.position).sqrMagnitude;
                if(closest < dist)
                {
                    closest = dist;
                    grid = g;
                }
            }
        }

        return grid;
    }

    public TestingCube Chess
    {
        get { return chess; }
        set { chess = value; }
    }
}
