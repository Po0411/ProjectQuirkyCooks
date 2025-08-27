using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DropManager : NetworkBehaviour
{
    public InventoryManager inventory;
    public Transform dropPoint;

    [Header("Drop Settings")]
    public Vector3 dropOffset = new Vector3(0f, 0.2f, 0.8f); // 카메라 앞·조금 위
    public float forwardForce = 6f;
    public float upwardForce = 4f;
    public float lifetime = 30f;

    // 네트워크가 실제로 돌고 있는지(Host/Server/Client)
    bool NetActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    // 네트워크가 꺼져 있으면(싱글) 입력 허용, 켜져 있으면 Owner만 입력 허용
    bool HasLocalAuthority => !NetActive || (IsSpawned && IsOwner);

    void Start()
    {
        // Inventory 자동 할당(같은 오브젝트에 있을 때만)
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        // dropPoint 자동 생성(카메라 기준)
        if (dropPoint == null && Camera.main != null)
        {
            var dp = new GameObject("DropPoint").transform;
            dp.SetParent(Camera.main.transform);
            dp.localPosition = dropOffset; // 카메라 앞쪽/조금 위
            dp.localRotation = Quaternion.identity;
            dropPoint = dp;
        }
    }

    void Update()
    {
        if (!HasLocalAuthority) return;
        if (Input.GetKeyDown(KeyCode.Q))
            TryDropSelectedItem();
    }

    void TryDropSelectedItem()
    {
        if (inventory == null)
        {
            Debug.LogError("[DropManager] InventoryManager가 비었습니다.");
            return;
        }

        var item = inventory.GetSelectedItem();
        if (item == null)
        {
            Debug.Log("[DropManager] 선택된 아이템이 없습니다.");
            return;
        }

        // 먼저 로컬 인벤에서 1개 차감
        if (!inventory.RemoveItemLocal(item, 1))
        {
            Debug.Log("[DropManager] 인벤토리에서 제거 실패");
            return;
        }

        // 드롭 위치/힘 계산
        if (dropPoint == null)
        {
            Debug.LogWarning("[DropManager] dropPoint가 없습니다.");
            return;
        }

        Vector3 pos = dropPoint.position + dropPoint.forward * dropOffset.z
                                        + dropPoint.right * dropOffset.x
                                        + dropPoint.up * dropOffset.y;
        Vector3 force = dropPoint.forward * forwardForce + Vector3.up * upwardForce;

        if (!NetActive)
        {
            // 싱글: 로컬로 바로 생성
            SpawnDroppedObjectLocal(item, pos, force);
        }
        else
        {
            // 멀티: 서버로 요청
            DropItemServerRpc(item.itemName, pos, force);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DropItemServerRpc(string itemName, Vector3 position, Vector3 force)
    {
        // 서버에서 아이템 데이터 찾기 (inventory가 null일 수도 있으므로 보강)
        var sourceInv = inventory ?? FindFirstObjectByType<InventoryManager>();
        if (sourceInv == null)
        {
            Debug.LogError("[DropManager] 서버에서 InventoryManager를 찾을 수 없습니다.");
            return;
        }

        var item = sourceInv.GetItemByName(itemName);
        if (item == null || item.worldPrefab == null)
        {
            Debug.LogError($"[DropManager] '{itemName}'의 worldPrefab이 비었습니다.");
            return;
        }

        var obj = Instantiate(item.worldPrefab, position, Quaternion.identity);

        var pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null) pickup.itemData = item;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn(true);

        StartCoroutine(DestroyAfterLifetime(obj, lifetime));
    }

    void SpawnDroppedObjectLocal(ItemData item, Vector3 position, Vector3 force)
    {
        if (item.worldPrefab == null)
        {
            Debug.LogError($"[DropManager] '{item.itemName}' worldPrefab이 없습니다.");
            return;
        }

        var obj = Instantiate(item.worldPrefab, position, Quaternion.identity);

        var pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null) pickup.itemData = item;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        StartCoroutine(DestroyAfterLifetime(obj, lifetime));
    }

    IEnumerator DestroyAfterLifetime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (obj == null) yield break;

        if (NetActive && obj.TryGetComponent(out NetworkObject no) && no.IsSpawned)
            no.Despawn(true);
        else
            Destroy(obj);
    }
}
