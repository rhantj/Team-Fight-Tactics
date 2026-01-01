using UnityEngine;

/// <summary>
/// 특정 코스트의 등장 확률을 정의하는 데이터 구조
/// 하나의 레벨에서 각 코스트가 등장할 비율을 나타낸다.
/// </summary>
[System.Serializable]
public class CostRate
{
    [Tooltip("기물 코스트 값(예: 1 ~ 3)")]
    public int cost;

    [Tooltip("해당 코스트가 등장 확률 값")]
    public float rate;
}

/// <summary>
/// 하나의 플레이어 레벨에 적용되는 게임 규칙 데이터
/// 등장확률, 배치 제한, 레벨업 조건을 함께 정의한다.
/// </summary>
[System.Serializable]
public class LevelData
{
    [Tooltip("플레이어 레벨 (1부터 시작)")]
    public int level;

    [Tooltip("해당 레벨에서의 코스트별 등장 확률 목록")]
    public CostRate[] rates;

    [Tooltip("해당 레벨에서 필드에 배치 가능한 최대 기물 수")]
    public int boardUnitLimit;

    [Tooltip("다음 레벨로 올라가기 위해 필요한 총 경험치량")]
    public int requiredExp;
}

/// <summary>
/// 플레이어 레벨별 게임 규칙을 관리하는 SO
/// ShopManager가 이 데이터를 참조하여
/// 유닛 등장 확률 계산, 레벨업 조건 판정, 배치 제한 설정을 수행한다.
/// </summary>
[CreateAssetMenu(fileName = "LevelDataTable", menuName = "TFT/Level Data Table")]
public class LevelDataTable : ScriptableObject
{
    [Tooltip("플레이어 레벨별 설정 데이터 배열")]
    public LevelData[] levels;
}

/// 이 데이터는 런타임 중 수정되지 않으며,
/// 게임 밸런스 조정은 인스펙터를 통해서만 이루어진다.