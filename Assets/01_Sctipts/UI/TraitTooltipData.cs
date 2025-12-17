using UnityEngine;

/// <summary>
/// 특정 특성의 툴팁 정보를 담는 ScriptableObject.
/// 
/// - 시너지 UI에서 마우스 오버 시 표시되는 텍스트 데이터이다.
/// - 특성 설명과 보유 개수에 따른 추가 설명을 함께 관리한다.
/// </summary>
[CreateAssetMenu(
    menuName = "Synergy/Trait Tooltip Data",
    fileName = "TraitTooltipData"
)]
public class TraitTooltipData : ScriptableObject
{
    public TraitType trait;   // 해당 툴팁 데이터가 대응하는 특성 타입

    [TextArea]
    public string description;        // 특성 자체에 대한 기본 설명 텍스트

    [TextArea]
    public string countDescription;   // 특성 보유 개수에 따른 단계별 설명 텍스트
}
