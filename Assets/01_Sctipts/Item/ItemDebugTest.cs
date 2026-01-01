using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDebugTest : MonoBehaviour
{
    [SerializeField] private ItemData[] testItems;

    public void SpawnRandomItem()
    {
        //Debug.Log("버튼 눌림");
        if (ItemSlotManager.Instance == null)
        {
            //Debug.LogError("ItemSlotManager.Instance 없음");
            return;
        }

        if (testItems == null || testItems.Length == 0)
        {
            //Debug.LogError("testItems 비어있음");
            return;
        }

        ItemData item = testItems[Random.Range(0, testItems.Length)];

        bool success = ItemSlotManager.Instance.AddItem(item);
        if (!success)
        {
            //Debug.Log("빈 슬롯이 없습니다.");
        }
    }
}
