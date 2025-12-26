using UnityEngine;

/// <summary>
/// 하나의 특성(Trait)에 대응되는 UI 표현 정보를 담는 데이터 구조.
/// 
/// - trait      : 특성 enum 값
/// - icon       : UI에 표시할 아이콘 스프라이트
/// - displayName: UI에 표시할 이름 (비어 있을 경우 enum 이름 사용)
/// </summary>
[System.Serializable]
public class TraitIconEntry
{
    public TraitType trait;
    public Sprite icon;
    public string displayName;
}

/// <summary>
/// TraitType(enum)과 UI 표현 정보를 매핑해주는 SO
///
/// 이 클래스는 특성의 게임 로직을 다루지 않으며,
/// 오직 UI 표현(아이콘, 표시 이름)만을 책임진다.
///
/// ShopSlot, 시너지 UI 등에서 TraitType을 기반으로
/// 아이콘과 텍스트를 조회하는 용도로 사용된다.
/// </summary>
[CreateAssetMenu(fileName = "TraitIconDataBase", menuName = "TFT/Trait Icon Database")]
public class TraitIconDataBase : ScriptableObject
{
    public TraitIconEntry[] entries;

    public Sprite GetIcon(TraitType trait)
    {
        foreach (var e in entries)
        {
            if (e.trait == trait)
                return e.icon;
        }
        return null;
    }

    /// <summary>
    /// 지정된 TraitType에 대응되는 UI 표시 이름을 반환한다.
    /// 
    /// - displayName이 비어 있을 경우 enum 이름을 그대로 사용한다.
    /// - 데이터가 존재하지 않을 경우에도 enum 이름을 반환한다.
    /// </summary>
    public string GetDisplayName(TraitType trait)
    {
        foreach (var e in entries)
        {
            if (e.trait == trait)
                return string.IsNullOrEmpty(e.displayName)
                    ? trait.ToString()
                    : e.displayName;
        }
        return trait.ToString();
    }
}
