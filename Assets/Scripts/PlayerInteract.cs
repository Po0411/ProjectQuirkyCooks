using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
            {
                // 1. 아이템인지 먼저 확인
                ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    InventoryManager inv = FindObjectOfType<InventoryManager>();
                    if (inv != null)
                    {
                        inv.AddItem(pickup.itemData);
                        Destroy(pickup.gameObject);
                        return;
                    }
                }

                // 2. 그 외 일반 상호작용
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
