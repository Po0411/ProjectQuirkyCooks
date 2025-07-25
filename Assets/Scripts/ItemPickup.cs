using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            InventoryManager inv = FindObjectOfType<InventoryManager>();
            if (inv != null)
            {
                inv.AddItem(itemData);
                Destroy(gameObject); // 씹어서 먹음
            }
        }
    }
}