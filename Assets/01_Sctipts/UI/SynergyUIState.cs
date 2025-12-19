/// <summary>
/// 시너지 UI에 표시하기 위한 단일 시너지 상태 데이터.
/// 
/// - 특정 특성의 현재 보유 개수와
/// - 활성화 여부(임계값 도달 여부)를 함께 관리한다.
/// - SynergyManager에서 계산된 결과를 UI 계층으로 전달하는 용도로 사용된다.
/// </summary>
public class SynergyUIState
{
    public TraitType trait;            // 시너지의 종류 (특성 타입)
    public int count;                  // 필드에 배치된 기물 기준 중복 제거된 개수
    public SynergyThreshold active;    // 활성화된 시너지 단계, 활성화되지 않았으면 null
}
