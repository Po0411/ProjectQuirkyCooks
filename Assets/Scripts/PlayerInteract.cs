using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ¿ìÅ¬¸¯
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}