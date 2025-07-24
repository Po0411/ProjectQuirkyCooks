using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // ¿ìÅ¬¸¯
        {
            InventoryManager inv = FindObjectOfType<InventoryManager>();
            if (inv != null)
            {
                inv.AddItem(itemData);
                Destroy(gameObject);
            }
        }
    }
}