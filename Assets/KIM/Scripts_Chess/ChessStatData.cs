using UnityEngine;

[CreateAssetMenu(fileName = "ChessStatSO", menuName = "TFT/ChessStatData")]
public class ChessStatData : ScriptableObject
{
    //=====================================================
    [Header("기본 능력치")]
    public string unitName;
    public int maxHP;
    public int armor;
    public int attackDamage;
    public float attackSpeed;
    public int mana;
    //=====================================================
    [Header("메타 정보")]
    public int starLevel;
    public int cost;   
    //=====================================================
    [Header("비주얼")]
    public Sprite icon;        
    public GameObject prefab;  
    //=====================================================
    [Header("스킬")]
    public string skillName;
    [TextArea]
    public string skillDescription;
}
