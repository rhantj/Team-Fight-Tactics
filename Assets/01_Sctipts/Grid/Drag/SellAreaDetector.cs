using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SellAreaDetector
{
    public event Action OnEnter;
    public event Action OnExit;

    public bool IsOver { get; private set; }

    private bool prev;
    private readonly List<RaycastResult> results = new();

    private readonly Transform sellAreaRoot;

    public SellAreaDetector(Transform sellAreaRoot)
    {
        this.sellAreaRoot = sellAreaRoot;
    }

    public void Evaluate(Vector2 screenPos)
    {
        results.Clear();

        var ped = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        EventSystem.current.RaycastAll(ped, results);

        bool inZone = false;
        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (go != null && go.transform.IsChildOf(sellAreaRoot))
            {
                inZone = true;
                break;
            }
        }

        if (!prev && inZone) OnEnter?.Invoke();
        else if (prev && !inZone) OnExit?.Invoke();

        IsOver = inZone;
        prev = inZone;
    }
}