using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 우클릭으로 기물을 선택하여 ChessInfoUI를 표시하는 전용 입력 핸들러.
/// - 좌클릭/드래그 로직과 완전히 분리됨
/// - Enemy 기물 선택 불가
/// - 동일 기물 재선택 시 토글 가능
/// </summary>
public class ChessSelectionHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask chessLayerMask; // Chess가 있는 레이어
    [SerializeField] private bool toggleOnSameChess = true;

    private ChessStateBase currentSelected;

    private void Update()
    {
        // 우클릭만 처리
        if (!Input.GetMouseButtonDown(1))
            return;

        TrySelectChess();
    }

    private void TrySelectChess()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, chessLayerMask))
        {
            // 빈 바닥 우클릭 → 선택 해제
            ClearSelection();
            return;
        }

        Chess chess = hit.transform.GetComponentInChildren<Chess>();
        if (chess == null)
            return;

        // Enemy 기물은 선택 불가
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

    private void SelectChess(ChessStateBase chess)
    {
        currentSelected = chess;

        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.ShowInfo(chess);
        }
    }

    private void ClearSelection()
    {
        currentSelected = null;

        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.Hide();
        }
    }
}
