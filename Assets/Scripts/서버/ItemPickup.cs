// Assets/Scripts/서버/ItemPickup.cs
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class ItemPickup : NetworkBehaviour
{
    [Header("무슨 아이템인지")]
    public ItemData itemData;

    [Header("상호작용")]
    public float maxDistance = 3f;          // 너무 멀면 못 줍게
    public KeyCode interactKey = KeyCode.E; // E키도 허용

    // 네트워크가 켜져 있는지(Host/Server/Client)
    bool NetActive =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    void Awake()
    {
        // OnMouseOver가 동작하려면 Collider가 반드시 있어야 함
        if (!TryGetComponent<Collider>(out _))
            Debug.LogError("[ItemPickup] Collider가 필요합니다.");
    }

    void OnMouseOver()
    {
        // 아이템 미세팅 방지
        if (itemData == null) return;

        // 입력(클릭 또는 E)
        if (!(Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey)))
            return;

        // 로컬 플레이어 Transform 찾기 (싱글/멀티 공용)
        var playerTr = GetLocalPlayerTransform();
        if (playerTr == null) return;

        // 너무 멀면 무시
        if (Vector3.Distance(playerTr.position, transform.position) > maxDistance)
            return;

        // 로컬 인벤토리 찾기
        var inv = playerTr.GetComponent<InventoryManager>();
        if (inv == null)
        {
            Debug.LogWarning("[ItemPickup] InventoryManager를 로컬 플레이어에서 찾지 못했습니다.");
            return;
        }

        // 우선 로컬 인벤에 추가 시도
        bool added = inv.AddItemLocal(itemData);
        if (!added)
        {
            Debug.Log("⚠ 인벤토리 가득 참");
            return;
        }

        Debug.Log($"✅ {itemData.itemName} 획득");

        // 싱글이면 그냥 파괴, 멀티면 서버에 디스폰 요청
        if (!NetActive)
        {
            Destroy(gameObject);
        }
        else
        {
            RequestDespawnServerRpc(NetworkObject);
        }
    }

    Transform GetLocalPlayerTransform()
    {
        if (NetActive)
        {
            var po = NetworkManager.Singleton.LocalClient?.PlayerObject;
            return po != null ? po.transform : null;
        }

        // 싱글: 씬에 있는 플레이어(또는 인벤 매니저)를 기준으로 찾기
        var inv = FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Exclude);
        return inv ? inv.transform : null;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDespawnServerRpc(NetworkObjectReference itemRef)
    {
        if (itemRef.TryGet(out NetworkObject netObj))
            netObj.Despawn(true);
    }
}
