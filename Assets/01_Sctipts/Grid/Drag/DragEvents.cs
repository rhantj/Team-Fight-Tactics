using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragEvents : AutoAdder<DragEvents>, IBeginDragHandler, IEndDragHandler, IDragHandler
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
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
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

        // 기물 드래그 효과음 추가
        SettingsUI.PlaySFX("DragChess",chess.transform.position, 1f);
        chessFirstPos = chess.transform.position;
        prevGrid = FindGrid(chessFirstPos);
        prevNode = prevGrid?.GetGridNode(chessFirstPos);


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
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!chess) return;
        chess.SetPosition(_worldPos);
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!chess) return;

        ShopManager shop = ShopManager.Instance;
        GridDivideBase field = grids[0];

        // 1) 판매 영역이면: 배치 로직 절대 타지 않게 완전 분기
        if (IsPointerOverSellArea)
        {
            // 판매 시도 (성공 여부 반환)
            bool sold = TrySellFromDrag(shop);

            // 판매 성공: 끝
            if (sold)
            {
                chess = null;
                if (shop != null) shop.ExitSellMode();
                HideLines();
                shop.UpdateCountUI(field);
                return;
            }

            // 판매 실패: 원래 자리 복구 후 끝
            RestoreToPrevNode();
            chess = null;
            if (shop != null) shop.ExitSellMode();
            HideLines();
            shop.UpdateCountUI(field);
            return;
        }

        // 2) 필드 밖(유효 드랍 불가) 처리
        if (OutofGrid())
        {
            chess = null;
            if (shop != null) shop.ExitSellMode();
            HideLines();
            shop.UpdateCountUI(field);
            return;
        }

        bool wasOnField = prevGrid is FieldGrid;
        bool nowOnBench = targetGrid is BenchGrid;
        if (wasOnField && nowOnBench)
        {
            chess.SetSynergyBonusStats(0, 0, 0);
        }

        // 3) 원래자리 그대로면 복구 처리 후 종료
        if (OnFirstNode())
        {
            chess = null;
            if (shop != null) shop.ExitSellMode();
            HideLines();
            shop.UpdateCountUI(field);
            return;
        }

        // 4) 정상 배치/스왑 확정 직전에만 이전 노드에서 제거
        ClearAllNodeChess(chess);

        // 5) 스왑/배치
        SwapPiece();
        if (targetGrid is FieldGrid)
        {
            (chess as Chess)?.SetOnField(true);
        }
        else if (targetGrid is BenchGrid)
        {
            (chess as Chess)?.SetOnField(false);
        }

        // 기물 드랍 효과음 추가
        SettingsUI.PlaySFX("DropChess", chess.transform.position, 1f);

        if (shop != null)
            shop.ExitSellMode();

        UpdateGridAndNode();
        UpdateSynergy();


        if (prevNode != null && !prevNode.ChessPiece)
        {
            prevNode.ChessPiece = chess;
        }

        chess = null;
        HideLines();
        shop.UpdateCountUI(field);
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

            ClearAllNodeChess(other);

            targetNode.ChessPiece = chess;
            prevNode.ChessPiece = other;
            if (other is Chess otherChess)
            {
                if (prevGrid is FieldGrid) otherChess.SetOnField(true);
                else if (prevGrid is BenchGrid) otherChess.SetOnField(false);
            }

            if (prevGrid is BenchGrid)
            {
                other.SetSynergyBonusStats(0, 0, 0);
            }
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

        bool sold = shop.TrySellUnit(chess.BaseData, chess.gameObject);
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
}
