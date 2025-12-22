using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChessInfoItemSlot :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    [SerializeField] private Image iconImage;

    private ItemData currentItem;

    public bool HasItem => currentItem != null;

    // ChessInfoUI에서 호출
    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (item == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        iconImage.sprite = item.icon;
        iconImage.enabled = true;
    }

    public void Clear()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    // ================== 마우스 오버 ==================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null) return;

        ItemInfoUIManager.Instance.Show(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemInfoUIManager.Instance.Hide();
        ItemRecipeUIManager.Instance.Hide();
    }

    // ================== 우클릭 ==================
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ItemInfoUIManager.Instance.Hide();
            ItemRecipeUIManager.Instance.Show(currentItem);
        }
    }
}
