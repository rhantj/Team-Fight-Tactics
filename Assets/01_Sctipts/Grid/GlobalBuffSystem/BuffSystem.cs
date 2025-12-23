using System.Collections.Generic;
using UnityEngine;

public class BuffSystem : MonoBehaviour
{
    [SerializeField] FieldGrid field;
    [SerializeField] float buffMultiplier = 1.2f;
    List<IBuffApply> buffApply = new();
    bool lazy;

    private void Awake()
    {
        GetComponentsInChildren(true,buffApply);
    }

    private void OnEnable()
    {
        field.OnGridChessPieceChanged += GridPieceChanged;
        lazy = true;
    }

    private void OnDisable()
    {
        field.OnGridChessPieceChanged -= GridPieceChanged;
    }

    private void LateUpdate()
    {
        if (!lazy) return;
        lazy = false;
        CalculateBuff();
    }

    private void GridPieceChanged(GridDivideBase arg1, GridNode arg2, ChessStateBase arg3, ChessStateBase arg4)
    {
        lazy = true;
    }


    public void CalculateBuff()
    {
        if(field == null) return;
        
        foreach(var node in field.FieldGrid)
        {
            if(node.ChessPiece is Chess c)
            {
                c.ClearAllBuffs();
            }
        }

        int n = 1;
        foreach (var ba in buffApply)
            ba.ApplyBuffs(field, n, buffMultiplier);
    }
}