using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Material, //재료템
    Combined  //완성템
}

[CreateAssetMenu(fileName = "ItemData", menuName = "TFT/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    public int addAttack;       //공격력
    public int addDefense;      //방어력
    public int addHp;           //체력
    public int addMp;           //마나
    public int addAttackSpeed;  //공속

    public ItemData combineA;     //재료 A  
    public ItemData combineB;     //재료 B
}

    
