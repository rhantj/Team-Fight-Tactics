using UnityEngine;

/// <summary>
/// 전투 시작 버튼의 클릭 이벤트를 처리하는 컴포넌트.
/// 
/// - UI 버튼과 연결되어 사용된다.
/// - 버튼 클릭 시 GameManager에 전투 시작 요청을 전달한다.
/// </summary>
public class BattleStartButton : MonoBehaviour
{
    /// <summary>
    /// 전투 시작 버튼 클릭 시 호출되는 이벤트 메서드.
    /// GameManager에 전투 시작 요청을 위임한다.
    /// </summary>
    public void OnClickBattleStart()
    {
        UIActionButtonController.Instance
            ?.RequestBattleStartFromUI();
    }
}
