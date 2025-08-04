using UnityEngine;
using Unity.Netcode;

public enum CookingType { Boil, Fry, Chop, Blend }

public class CookingStation : NetworkBehaviour, IInteractable
{
    [Header("조리 도구 설정")]
    public CookingType type;
    public string requiredItemName;   // 필요한 재료 (예: "사과")
    public string resultItemName;     // 결과 아이템 (예: "썬 사과")

    public string GetInteractText()
    {
        switch (type)
        {
            case CookingType.Boil: return "끓이기";
            case CookingType.Fry: return "튀기기";
            case CookingType.Chop: return "썰기";
            case CookingType.Blend: return "갈기기";
        }
        return "사용하기";
    }

    // 인터페이스 1 : InventoryManager 전달
    public void Interact(InventoryManager inventory)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            RequestCookingServerRpc(requiredItemName, resultItemName);
        }
        else
        {
            ProcessCooking(inventory, requiredItemName, resultItemName);
        }
    }

    // 인터페이스 2 : InventoryManager 자동 검색
    public void Interact()
    {
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
        {
            Interact(inv);
        }
        else
        {
            Debug.LogError("❌ InventoryManager를 찾을 수 없음");
        }
    }

    // 클라이언트 → 서버 상호작용 요청
    [ServerRpc(RequireOwnership = false)]
    private void RequestCookingServerRpc(string inputItem, string outputItem, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client))
            return;

        var player = client.PlayerObject;
        if (player == null) return;

        var inv = player.GetComponent<InventoryManager>();
        if (inv != null)
            ProcessCooking(inv, inputItem, outputItem);
    }

    // 실제 처리 (서버에서만 실행)
    private void ProcessCooking(InventoryManager inv, string inputItem, string outputItem)
    {
        ItemData input = inv.GetItemByName(inputItem);
        if (input == null)
        {
            Debug.Log($"❌ {inputItem} 없음");
            return;
        }

        if (!inv.RemoveItemLocal(input, 1))
        {
            Debug.Log($"❌ {inputItem} 제거 실패");
            return;
        }

        // 모든 클라에 제거 반영
        inv.RemoveItemClientRpc(inputItem, 1);

        ItemData result = inv.GetItemByName(outputItem);
        if (result != null)
        {
            inv.AddItemLocal(result);
            inv.AddItemClientRpc(outputItem);
            Debug.Log($"✅ {inputItem} → {outputItem} 변환 성공");
        }
        else
        {
            Debug.LogError($"❌ {outputItem} ItemData 없음");
        }
    }
}
