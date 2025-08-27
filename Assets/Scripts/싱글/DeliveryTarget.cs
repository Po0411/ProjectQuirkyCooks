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

    private void RefreshFromUI()
    {
        if (_ui == null) return;
        requiredItem = _ui.GetItemForColor(regionColor);

        if (bubbleIcon != null)
        {
            bubbleIcon.sprite = requiredItem ? requiredItem.icon : null;
            bubbleIcon.enabled = (requiredItem && requiredItem.icon);
        }
    }

    // ───────────────── IInteractable ─────────────────
    public string GetInteractText()
    {
        if (requiredItem == null) return "주문 없음";
        return $"{requiredItem.itemName} 배달";
    }

    // IInteractable에 매개변수 없는 Interact()가 있을 때를 위한 구현
    public void Interact()
    {
        // 씬에서 내 인벤토리 찾아서 위임
        var inv = FindObjectOfType<InventoryManager>(true);
        Interact(inv);
    }

    // 선택된 아이템을 직접 받는 버전
    public void Interact(InventoryManager inv)
    {
        if (requiredItem == null) return;

        if (inv == null)
        {
            Debug.Log("인벤토리 없음");
            return;
        }

        var selected = inv.GetSelectedItem();
        if (selected == null)
        {
            Debug.Log("선택된 아이템 없음");
            return;
        }

        if (selected == requiredItem)
        {
            inv.RemoveItemLocal(selected, 1);
            Debug.Log("배달 완료!");

            // 왼쪽 주문 중 이 지역색 슬롯만 재랜덤 + 버블 자동 갱신
            if (_ui != null) _ui.CompleteAndRerollForColor(regionColor);
            RefreshFromUI();
        }
        else
        {
            Debug.Log("다른 주문입니다.");
        }
    }
}
