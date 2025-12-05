using UnityEngine;

/// <summary>
/// 코스트별 등장 확률 정보
/// </summary>
[System.Serializable]
public class CostRate
{
    [Tooltip("기물 코스트 (1~5 등)")]
    public int cost;

    [Tooltip("해당 코스트의 등장 확률 (합계는 LevelData에서 관리)")]
    public float rate;
}

/// <summary>
/// 특정 레벨에서 사용되는 모든 설정값을 담는 데이터 구조
/// </summary>
[System.Serializable]
public class LevelData
{
    [Tooltip("플레이어 레벨 (1부터 시작)")]
    public int level;

    [Tooltip("코스트별 등장 확률 데이터 리스트")]
    public CostRate[] rates;

    [Tooltip("필드에 배치 가능한 최대 기물 수")]
    public int boardUnitLimit;

    [Tooltip("다음 레벨업까지 필요한 총 경험치량")]
    public int requiredExp;
}

/// <summary>
/// 전체 레벨 정보를 보관하는 ScriptableObject
/// ShopManager가 이 데이터를 참조하여
/// - 확률 계산
/// - 레벨업 조건 체크
/// 등을 수행한다.
/// </summary>
[CreateAssetMenu(fileName = "LevelDataTable", menuName = "TFT/Level Data Table")]
public class LevelDataTable : ScriptableObject
{
    [Tooltip("레벨별 설정 데이터 리스트")]
    public LevelData[] levels;
}
