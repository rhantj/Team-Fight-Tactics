using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase
{
    public ItemData Data { get; private set; }

    public ItemBase(ItemData data)
    {
        Data = data;
    }

    //유닛 장착시 효과 계산 여기서
}
