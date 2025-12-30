using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 스킬 아이콘 위에 마우스를 올렸을 때
/// 스킬 툴팁 UI를 표시하기 위한 트리거 컴포넌트.
///
/// - UI 이벤트(IPointerEnter / Exit)를 통해 동작
/// - 실제 툴팁 표시 로직은 SkillTooltipUI에 위임
/// - 자신은 "언제 보여줄지"만 판단하는 역할
/// </summary>
public class SkillTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    /// <summary>
    /// 현재 툴팁에 표시할 기물 데이터.
    /// ChessInfoUI에서 SetData를 통해 주입된다.
    /// </summary>
    [SerializeField] private ChessStatData chessData;

    /// <summary>
    /// 외부에서 기물 데이터를 주입하기 위한 메서드.
    /// 보통 ChessInfoUI.ShowInfo 호출 시 함께 설정된다.
    /// </summary>
    public void SetData(ChessStatData data)
    {
        chessData = data;
    }

    /// <summary>
    /// 마우스 포인터가 스킬 아이콘 영역에 진입했을 때 호출된다.
    /// 스킬 툴팁 UI를 표시한다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (chessData == null)
        {
            return;
        }

        SkillTooltipUI.Instance.Show(
            chessData.skillIcon,
            chessData.skillName,
            chessData.skillDescription
        );
    }

    /// <summary>
    /// 마우스 포인터가 스킬 아이콘 영역을 벗어났을 때 호출된다.
    /// 스킬 툴팁 UI를 숨긴다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        SkillTooltipUI.Instance.Hide();
    }
}
