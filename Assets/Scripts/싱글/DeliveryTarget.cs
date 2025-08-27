using UnityEngine;
using UnityEngine.UI;

public class DeliveryTarget : MonoBehaviour, IInteractable
{
    [Header("이 손님이 위치한 지역 색")]
    public RegionColor regionColor = RegionColor.Red;

    [Header("요구 아이템(자동 설정)")]
    public ItemData requiredItem;

    [Header("요구 아이콘(선택)")]
    public Image bubbleIcon;

    private DeliveryUIManager _ui;

    void Start()
    {
        _ui = FindObjectOfType<DeliveryUIManager>(true);
        RefreshFromUI();
        if (_ui != null) _ui.OnRefreshed += RefreshFromUI;
    }

    void OnDestroy()
    {
        if (_ui != null) _ui.OnRefreshed -= RefreshFromUI;
    }

    void RefreshFromUI()
    {
        if (_ui == null) return;
        requiredItem = _ui.GetItemForColor(regionColor);
        if (bubbleIcon != null)
        {
            bubbleIcon.sprite = requiredItem ? requiredItem.icon : null;
            bubbleIcon.enabled = (requiredItem && requiredItem.icon);
        }
    }

    public string GetInteractText() =>
        requiredItem == null ? "주문 없음" : $"{requiredItem.itemName} 배달";

    public void Interact() => Interact(FindObjectOfType<InventoryManager>(true));

    public void Interact(InventoryManager inv)
    {
        if (requiredItem == null) return;
        if (inv == null) { Debug.Log("인벤토리 없음"); return; }

        var selected = inv.GetSelectedItem();
        if (selected == null) { Debug.Log("선택된 아이템 없음"); return; }

        if (selected == requiredItem)
        {
            inv.RemoveItemLocal(selected, 1);
            var gsm = GameStatsManager.Instance;
            if (gsm != null)
            {
                gsm.AddRevenue(requiredItem.price);   // 라운드 수익
                gsm.AddCashLocal(requiredItem.price); // 현금도 증가
                gsm.AddDeliveryLocal(1);              // 횟수 증가
            }

            // 슬롯 재랜덤 + 거품아이콘 갱신
            _ui?.CompleteAndRerollForColor(regionColor);
            RefreshFromUI();
            Debug.Log("배달 완료!");
        }
        else
        {
            Debug.Log("다른 주문입니다.");
        }
    }
}
