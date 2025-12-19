using System;
using System.Reflection;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] GridDivideBase[] grids;    // 어떤 그리드 인지
    [SerializeField] ChessStateBase chess;      // 잡고있는 기물
    [SerializeField] Vector3 chessFirstPos;     // 기물의 첫 위치
    GridNode targetNode;                        // 옮기고자 하는 노드
    [SerializeField] GridDivideBase targetGrid; // 옮기고자 하는 그리드
    GridNode prevNode;                          // 전에 위치한 노드
    [SerializeField] GridDivideBase prevGrid;   // 전에 위치한 그리드
    [SerializeField] Vector3 _worldPos;         // 마우스 위치를 월드 위치로 바꾼 값
    [SerializeField] Ray camRay;                // 레이

    RectZone sellzone = new RectZone { minX = 310, maxX = 1610, minY = 0, maxY = 210 };
    public bool IsPointerOverSellArea = false;  // 상점 판매용 
    public bool CanDrag = false;
    public int playerLevel;

    private void Update()
    {
        camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        CalculateWorldPosition(camRay);
        CalculatePointerPosition();
    }

    void CalculatePointerPosition()
    {
        bool isInside = sellzone.IsInside(Input.mousePosition);
        IsPointerOverSellArea = isInside;
    }


    // 드래그 시작시
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance)
        {
            CanDrag = GameManager.Instance.roundState == RoundState.Preparation;
            if (!CanDrag) return;
        }

        CalculateWorldChess(camRay);
        if (!chess) return;

        Chess chessComponent = chess as Chess;
        if (chessComponent != null && chessComponent.team == Team.Enemy)
        {
            chess = null;
            return;
        }
        ChessInfoUI.Instance.ShowInfo(chess);

        chessFirstPos = _worldPos;
        prevGrid = FindGrid(chessFirstPos);
        
        if(prevGrid)
            prevNode = prevGrid.GetGridNode(chessFirstPos);

        if (prevNode != null && !prevNode.ChessPiece)
        {
            prevNode.ChessPiece = chess;
        }

        ShopManager shop = ShopManager.Instance;
        if (shop != null)
        {
            FieldInfo baseDataField = typeof(ChessStateBase).GetField
                ("baseData", BindingFlags.Instance | BindingFlags.NonPublic);

            ChessStatData chessData = baseDataField.GetValue(chess) as ChessStatData;

            if (chessData != null)
                shop.EnterSellMode(shop.CalculateSellPrice(chessData, chess.StarLevel));
        }

        foreach(var g in grids)
        {
            g.lineParent.gameObject.SetActive(true);
        }
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {

        if (!chess) return;
        chess.SetPosition(_worldPos);
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!chess) return;

        var shop = ShopManager.Instance;

        // 그리드 밖이면 롤백
        if (targetGrid == null || targetNode == null)
        {
            RollbackToPrev();
            CleanupAfterDrag(shop);
            return;
        }

        // 판매면 판매 처리 (배치 로직 건드리지 말고)
        if (IsPointerOverSellArea)
        {
            TrySellPiece();
            CleanupAfterDrag(shop);
            return;
        }

        // ✅ 배치/스왑은 이걸로만 처리
        bool ok = ForcePlaceOrSwap(prevNode, targetNode, chess);
        if (!ok)
        {
            RollbackToPrev();
            CleanupAfterDrag(shop);
            return;
        }

        // ✅ 최후 안전장치: 같은 위치에 다른 말이 남아있으면 정리
        FixOverlapAtNode(targetNode, chess);

        UpdateGridAndNode();
        UpdateSynergy();

        chess = null;
        CleanupLines();
        if (shop != null) shop.ExitSellMode();
    }

    void RollbackToPrev()
    {
        if (prevNode != null)
        {
            prevNode.ChessPiece = chess;
            chess.SetPosition(prevNode.worldPosition);
        }
        else
        {
            chess.SetPosition(chessFirstPos);
        }
    }

    void FixOverlapAtNode(GridNode node, ChessStateBase keeper)
    {
        // 같은 칸 위치에 있는 ChessStateBase를 찾아서,
        // keeper가 아닌 애가 있으면 prevNode로 돌려보냄
        var hits = Physics.OverlapSphere(node.worldPosition + Vector3.up * 0.5f, 0.2f);
        foreach (var h in hits)
        {
            var other = h.GetComponentInParent<ChessStateBase>();
            if (other != null && other != keeper)
            {
                // 다른 말이 같은 칸에 남아있으면 강제로 한 칸 밀어냄
                // 1순위: other가 점유한 노드를 찾아서 그걸 다시 세팅
                RestoreOtherToNearestNode(other);
            }
        }
    }

    void RestoreOtherToNearestNode(ChessStateBase other)
    {
        // 겹침 상황이니까, 가장 가까운 그리드/노드를 다시 찾아 넣어줌
        foreach (var g in grids)
        {
            if (g == null) continue;
            if (g.GetGridNode(other.transform.position) == null) continue;

            var n = g.GetGridNode(other.transform.position); // (핫픽스니까 기존 함수 사용)
            if (n != null && n.ChessPiece == null)
            {
                n.ChessPiece = other;
                other.SetPosition(n.worldPosition);
                return;
            }
        }

        // 못 찾으면 그냥 현재 위치 유지(최악 방지용)
    }


    void CleanupAfterDrag(ShopManager shop)
    {
        if (shop != null) shop.ExitSellMode();
        chess = null;
        CleanupLines();
    }

    void CleanupLines()
    {
        foreach (var g in grids)
            if (g != null && g.lineParent != null)
                g.lineParent.gameObject.SetActive(false);
    }


    private void UpdateSynergy()
    {
        FieldGrid fieldGrid = grids[0] as FieldGrid;
        if (fieldGrid == null) return;
        if (SynergyManager.Instance == null) return;
        var fieldUnits = fieldGrid.GetAllFieldUnits();
        SynergyManager.Instance.RecalculateSynergies(fieldUnits);
    }

    // 기물이 필드 밖으로 나갔을 떄
    private bool OutofGrid()
    {
        if (OutOfGridCondition())
        {
            if (prevNode != null)
            {
                chess.SetPosition(prevNode.worldPosition);
                prevNode.ChessPiece = chess;
            }
            else
            {
                chess.SetPosition(chessFirstPos);
                //prevNode.ChessPiece = chess;
            }

            return true;
        }
        return false;
    }


    // 필드에 드랍 가능한지 판단
    bool CanDrop()
    {
        // 필드 식별 불가
        if (!targetGrid || targetNode == null) return false;

        // 판매 영역
        if (IsPointerOverSellArea) return true;

        bool targetField = targetGrid is FieldGrid;
        bool prevField = prevGrid is FieldGrid;
        bool enemyField = targetGrid is EnemyGrid;

        // 필드 밖이나 벤치
        if (!targetField && !enemyField) return true;

        // 필드-> 필드로 이동 가능
        if (prevField && !enemyField) return true;

        int level = PlayerLevel();
        if (!targetNode.ChessPiece) return !targetGrid.IsFull(level);
        return true;
    }

    bool OutOfGridCondition() =>
        (targetGrid == null || targetNode == null || !CanDrop()) && 
        !IsPointerOverSellArea;

    // 기물이 처음 노드 위에 있을 때
    private bool OnFirstNode()
    {
        if (prevNode != null && targetNode == prevNode && targetGrid == prevGrid && !IsPointerOverSellArea)
        {
            chess.SetPosition(targetNode.worldPosition);
            targetNode.ChessPiece = chess;

            UpdateGridAndNode();
            return true;
        }
        return false;
    }

    // 다른 기물이 노드위에 있는 경우
    private void SwapPiece()
    {
        if (!targetGrid) return;
        ChessStateBase other = targetNode.ChessPiece;
        if (other != null && other != chess && !IsPointerOverSellArea)
        {
            var to = targetNode.worldPosition;
            var from = prevNode.worldPosition;

            chess.SetPosition(to);
            other.SetPosition(from);

            targetNode.ChessPiece = chess;
            prevNode.ChessPiece = other;
        }
        else
        {
            chess.SetPosition(targetNode.worldPosition);
            targetNode.ChessPiece = chess;
        }
    }
        
    // 그리드 업데이트
    private void UpdateGridAndNode()
    {
        prevGrid = targetGrid;
        prevNode = targetNode;
    }

    // 기물 판매
  

    // 마우스 위치를 월드 위치로 변환
    void CalculateWorldPosition(Ray ray)
    {
        var ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out var hit))
        {
            var pos = ray.GetPoint(hit);

            targetGrid = FindGrid(pos);
            if (targetGrid)
            {
                targetNode = targetGrid.GetGridNode(pos);
                _worldPos = targetNode.worldPosition;
            }
            else
            {
                targetNode = null;
                _worldPos = pos;
            }
        }
    }

    // 드래그 시 마우스 포인터 앞에 있는 기물 잡기
    void CalculateWorldChess(Ray ray)
    {
        if(Physics.Raycast(ray, out var hit, 1000f))
        {
            Chess = hit.transform.GetComponentInChildren<Chess>();
            return;
        }

        Chess = null;
    }

    // 마우스 위치가 현재 어떤 그리드 위에 있는지
    GridDivideBase FindGrid(Vector3 pos)
    {
        foreach(var g in grids)
        {
            if (!g) continue;
            if (g.GetGridNode(pos) != null)
            {
                return g;
            }
        }

        return null;
    }

    // 드래그 시 노드 위의 기물 정보 제거
    void ClearAllNodeChess(ChessStateBase piece)
    {
        foreach(var g in grids)
        {
            g.ClearChessPiece(piece);
        }
    }

    // 기물 프로퍼티
    public ChessStateBase Chess
    {
        get { return chess; }
        set { chess = value; }
    }

    // 플레이어 레벨 가져오기
    int PlayerLevel()
    {
        FieldInfo field = typeof(ShopManager).GetField(
            "playerLevel", (BindingFlags.Instance | BindingFlags.NonPublic) );
        int level = (int)field.GetValue(ShopManager.Instance);

        return level;
    }


    private bool TrySellFromDrag(ShopManager shop)
    {
        if (!chess) return false;
        if (shop == null) shop = FindObjectOfType<ShopManager>();
        if (!shop) return false;

        FieldInfo baseDataField = typeof(ChessStateBase).GetField(
            "baseData", BindingFlags.Instance | BindingFlags.NonPublic);

        ChessStatData chessData = baseDataField.GetValue(chess) as ChessStatData;
        if (chessData == null)
        {
            Debug.LogError("[TrySellFromDrag] chessData is null");
            return false;
        }

        bool sold = shop.TrySellUnit(chessData, chess.gameObject);
        if (!sold) return false;

        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.NotifyChessSold(chess);
        }

        // 판매 성공일 때만 그리드/노드에서 제거
        ClearAllNodeChess(chess);
        UpdateSynergy();
        return true;
    }

    private void RestoreToPrevNode()
    {
        if (!chess) return;

        if (prevNode != null)
        {
            chess.SetPosition(prevNode.worldPosition);
            prevNode.ChessPiece = chess;
        }
        else
        {
            chess.SetPosition(chessFirstPos);
        }
    }

    private void HideLines()
    {
        foreach (var g in grids)
        {
            if (g != null && g.lineParent != null)
                g.lineParent.gameObject.SetActive(false);
        }
    }

    bool ForcePlaceOrSwap(GridNode from, GridNode to, ChessStateBase move)
    {
        if(move == null) return false;

        var other = to.ChessPiece;

        if(other == null || other == move)
        {
            to.ChessPiece = move;
            move.SetPosition(to.worldPosition);
            return true;
        }

        if (from == null) return false;

        to.ChessPiece = move;
        from.ChessPiece = other;

        Vector3 toPos = to.worldPosition;
        Vector3 fromPos = from.worldPosition;

        move.SetPosition(toPos);
        other.SetPosition(fromPos);

        return true;
    }
}
