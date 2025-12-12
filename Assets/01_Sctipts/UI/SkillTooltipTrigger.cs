using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private ChessStatData chessData;

    public void SetData(ChessStatData data)
    {
        chessData = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("SkillIcon Pointer Enter");

        if (chessData == null)
        {
            Debug.Log("ChessData is NULL");
            return;
        }

        SkillTooltipUI.Instance.Show(
            chessData.skillIcon,
            chessData.skillName,
            chessData.skillDescription
        );
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        SkillTooltipUI.Instance.Hide();
    }
}
