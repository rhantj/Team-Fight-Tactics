using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
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
    public bool IsPointerOverSellArea = false;  // 상점 판매용 
    public bool CanDrag = false;

    private void Update()
    {
        camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        CalculateWorldPosition(camRay);
    }

    // 드래그 캔버스 위에 위치
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverSellArea = false;
    }

    // 드래그 캔버스 밖에 위치
    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverSellArea = true;
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

        ChessInfoUI.Instance.ShowInfo(chess);

        chessFirstPos = _worldPos;
        prevGrid = FindGrid(chessFirstPos);
        
        if(prevGrid)
            prevNode = prevGrid.GetNearGridNode(chessFirstPos);

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
        ShopManager shop = ShopManager.Instance;

        // 필드 밖
        if (OutofGrid())
        {
            chess = null;
            if (shop != null)
                shop.ExitSellMode();
            return;
        }

        ClearAllNodeChess(chess);

        bool wasOnField = prevGrid is FieldGrid;
        bool nowOnBench = targetGrid is BenchGrid;

        if (wasOnField && nowOnBench)
        {
            chess.ResetSynergyStats();
        }

        // 원래자리 그대로
        if (OnFirstNode())
        {
            chess = null;
            return;
        }

        // 노드 위에 기물이 있는 경우
        SwapPiece();

        if (shop != null)
            shop.ExitSellMode();
        SellPiece();


        UpdateGridAndNode();
        UpdateSynergy(); //12.09 Kim add
        chess = null;

        foreach (var g in grids)
        {
            g.lineParent.gameObject.SetActive(false);
        }
    }


    private void UpdateSynergy()
    {
        FieldGrid fieldGrid = FindObjectOfType<FieldGrid>();
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
    private void SellPiece()
    {
        if (!IsPointerOverSellArea || !chess) return;

        ShopManager shop = FindObjectOfType<ShopManager>();
        if (!shop) return;

        // reflection
        FieldInfo baseDataField = typeof(ChessStateBase).GetField
            ("baseData", BindingFlags.Instance | BindingFlags.NonPublic);
        ChessStatData chessData = baseDataField.GetValue(chess) as ChessStatData;

        shop.TrySellUnit(chessData, chess.gameObject);
        ClearAllNodeChess(chess);
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
                targetNode = targetGrid.GetNearGridNode(pos);
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
        GridDivideBase grid = null;
        float dist = float.PositiveInfinity;

        if (grids.Length == 0) return null;

        foreach(var g in grids)
        {
            if (!g) continue;
            if (g.IsPositionInGrid(pos))
            {
                float closest = (pos - g.transform.position).sqrMagnitude;
                if(closest < dist)
                {
                    dist = closest;
                    grid = g;
                }
            }
        }

        return grid;
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

}
