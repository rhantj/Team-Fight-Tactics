using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChessInfoItemSlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image iconImage;

    [Header("Default Slot")]
    [SerializeField] private Sprite defaultSlotSprite;

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (item == null)
        {
            // 빈 슬롯
            iconImage.sprite = defaultSlotSprite;
            iconImage.color = Color.white;
        }
        else
        {
            // 아이템 있음
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }
    }

    public void Clear()
    {
        SetItem(null);
    }

    // 마우스 오버 / 우클릭은 item != null 일 때만 반응
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
