using UnityEngine;
using Unity.Netcode;

public class PlayerInventoryUI : NetworkBehaviour
{
    [Header("UI 프리팹 (슬롯 포함)")]
    public GameObject inventoryUIPrefab;
    private GameObject myUI;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 내 전용 UI 생성
            myUI = Instantiate(inventoryUIPrefab);
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
                myUI.transform.SetParent(canvas.transform, false);

            // 슬롯 연결
            var inv = GetComponent<InventoryManager>();
            if (inv != null)
            {
                var slots = myUI.GetComponentsInChildren<InventorySlot>();
                inv.InitializeSlots(slots);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && myUI != null)
        {
            Destroy(myUI);
        }
    }
}
