using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplyRow01BuffOnButton : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FieldGrid field;
    [SerializeField] Button applyButton;

    [Header("Buff")]
    [SerializeField] private float buffMultiplier = 1.2f;

    [Header("Target Rows")]
    [SerializeField] private int rowA = 0;
    [SerializeField] private int rowB = 1;

    private void Start()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(Apply);
        }
    }

    // 버튼 OnClick에 이 함수 연결
    public void Apply()
    {
        foreach (var node in field.FieldGrid)
        {
            node.ChessPiece?.ClearAllBuffs();
        }

        ApplyRow(rowA);
        ApplyRow(rowB);
    }

    private void ApplyRow(int row)
    {
        List<ChessStateBase> units = new();
        units = field.GetRowUnits(row);

        if (units == null || units.Count == 0) return;

        for (int i = 0; i < units.Count; i++)
        {
            units[i].GlobalBuffApply(buffMultiplier);
        }
    }
}
