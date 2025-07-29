using UnityEngine;
using Unity.Netcode;

public class PlayerInteract : NetworkBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
            {
                ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    RequestPickupServerRpc(pickup.NetworkObject, pickup.itemData.itemName);
                    return;
                }

                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    [ServerRpc]
    private void RequestPickupServerRpc(NetworkObjectReference pickupRef, string itemName, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        var player = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject;
        var inv = player.GetComponent<InventoryManager>();

        var item = inv.GetItemByName(itemName);
        if (item != null)
            inv.AddItemServerRpc(itemName, senderId);

        if (pickupRef.TryGet(out NetworkObject pickupObj))
            pickupObj.Despawn(true);
    }
}
