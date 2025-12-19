using UnityEngine;

/// <summary>
/// 하나의 특성에 대해 단계별 시너지 아이콘 스프라이트를 묶는 데이터 클래스.
/// 
/// - 동일 특성에 대해
///   비활성 / 1단계 / 2단계 / 3단계 아이콘을 한 세트로 관리한다.
/// - ScriptableObject에서 배열 형태로 사용된다.
/// </summary>
[System.Serializable]
public class TraitSynergyIconSet
{
    public TraitType trait;   // 해당 아이콘 세트가 대응하는 특성 타입
    public Sprite gray;       // 비활성 또는 최소 조건 미달 상태 아이콘
    public Sprite bronze;     // 1단계 활성 아이콘
    public Sprite silver;     // 2단계 활성 아이콘
    public Sprite gold;       // 3단계 이상 활성 아이콘
}

/// <summary>
/// 특성별 시너지 아이콘 세트를 관리하는 데이터베이스 ScriptableObject.
/// 
/// - 시너지 UI에서 특성 타입과 현재 개수에 따라
///   표시할 아이콘 스프라이트를 조회하는 용도로 사용된다.
/// </summary>
[CreateAssetMenu(
    fileName = "TraitSynergyIconDatabase",
    menuName = "TFT/Trait Synergy Icon Database")]
public class TraitSynergyIconDatabase : ScriptableObject
{
    public TraitSynergyIconSet[] sets;   // 특성별 시너지 아이콘 세트 목록

    // 특성 타입과 보유 개수에 따라 적절한 시너지 아이콘을 반환
    public Sprite GetIcon(TraitType trait, int count)
    {
        foreach (var set in sets)
        {
            if (set.trait != trait) continue;

            if (count <= 1) return set.gray;
            if (count == 2) return set.bronze;
            if (count == 3) return set.silver;
            return set.gold;
        }

        return null;
    }
}
