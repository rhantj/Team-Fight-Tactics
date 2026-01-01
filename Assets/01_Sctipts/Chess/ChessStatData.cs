using UnityEngine;

[CreateAssetMenu(fileName = "ChessStatSO", menuName = "TFT/ChessStatData")]

// 10개 기물에 대한 SO 파일을 만들어주고.
// ID값을 string값으로 config로 쓸수있게끔. 풀링
// SO는 10개 만들어주고.
// 슬롯에서 참조할것 . 
// 아이콘 이름 시너지,코스트? 
// 풀 아이디 안다르게 하나 통일.
// 기물담을 풀 이름, 첫글자 대문자,나중에 config랑 똑같이?

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
    [Header("공격 사거리")]
    public float attackRange = 1.8f; 

    [Header("이동 설정")]
    public float moveSpeed = 3.5f; 

    //=====================================================
    [Header("메타 정보")]
    public int starLevel;
    public int cost;

    //=====================================================
    [Header("특성")]
    public TraitType[] traits;   

    //=====================================================
    [Header("비주얼")]
    public Sprite icon;
    public GameObject prefab;

    //=====================================================
    [Header("스킬")]
    public Sprite skillIcon;
    public string skillName;
    [TextArea]
    public string skillDescription;
    //=====================================================
    [Header("풀 설정")]
    [Tooltip("PoolConfig.id랑 맞추면 됩니당")]
    public string poolID;
    //=====================================================
    [Header("서브스테이트머신 설정")]
    [Tooltip("서브스테이트머신으로 전이될지를 정합니다.")]
    public bool useBattleState = false;
    //=====================================================
    [Header("종료 패널 전용 비주얼")]
    public Sprite gameOverPortrait;
    //=====================================================
    [Header("승리시 대사")]
    public AudioClip victoryVoice;

}
