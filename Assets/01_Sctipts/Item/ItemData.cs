using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "TFT/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    //나중에
    //itemType : 재료/완성 구분
    //조합표 
    //스탯 추가 해야함
}
