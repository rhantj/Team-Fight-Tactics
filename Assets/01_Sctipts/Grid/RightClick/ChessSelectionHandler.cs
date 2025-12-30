using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 이 스크립트는 입력 처리만 담당하며,
/// 선택 결과에 따른 UI 표현은 ChessInfoUI에 위임한다.
/// 우클릭으로 기물을 선택하여 ChessInfoUI를 표시하는 전용 입력 핸들러.
/// - 좌클릭/드래그 로직과 완전히 분리됨
/// - Enemy 기물 선택 불가
/// - 동일 기물 재선택 시 토글 가능
/// </summary>
public class ChessSelectionHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask chessLayerMask; // Chess가 있는 레이어
    [SerializeField] private bool toggleOnSameChess = true; //동일 기물 재선택시 토글여부

    private ChessStateBase currentSelected; // 지금 선택된 기물은 누구인가만 따짐

    private void Update()
    {
        // 우클릭 입력만 감지
        if (!Input.GetMouseButtonDown(1))
            return;

        TrySelectChess();
    }

    // 마우스 위치에 레이를 생성해 기물 레이어에 RayCast하고
    // 맞은 오브젝트에서 Chess를 탐색
    private void TrySelectChess()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, chessLayerMask))
        {
            // 빈 바닥 우클릭  -> 선택 해제
            ClearSelection();
            return;
        }

        Chess chess = hit.transform.GetComponentInChildren<Chess>();
        if (chess == null)
            return;

        // Enemy 기물은 선택 불가 <- 적정보도 불러오기위해 주석처리했습니다.
        //if (chess.team == Team.Enemy)
        //    return;

        // 같은 기물 재선택 처리
        if (currentSelected == chess)
        {
            if (toggleOnSameChess)
                ClearSelection();
            return;
        }

        SelectChess(chess);
    }

    // 상태 변경 UI표시 요청 -> ChessInfoUI에 책임 위임
    private void SelectChess(ChessStateBase chess)
    {
        currentSelected = chess;

        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.ShowInfo(chess);
        }
    }

    // 선택 상태 초기화 & 정보 UI 숨김
    private void ClearSelection()
    {
        currentSelected = null;

        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.Hide();
        }
    }
}
