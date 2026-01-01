using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase
{
    public ItemData Data { get; private set; }
    protected ChessStateBase owner;

    public ItemBase(ItemData data)
    {
        Data = data;
    }

    public virtual void OnEquip(ChessStateBase chess)
    {
        owner = chess;
    }

    public virtual void OnUnequip()
    {
        owner = null;
    }
}
