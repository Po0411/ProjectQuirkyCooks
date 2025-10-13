using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("UI 참조")]
    public Image icon;                  // 아이템 아이콘
    public TextMeshProUGUI countText;   // 아이템 수량
    public Image background;            // 슬롯 배경

    [Header("Background Sprites")]
    public Sprite normalBackground;     // 기본 흰색
    public Sprite selectedBackground;   // 선택 노란색

    public ItemData currentItem;
    private int count = 0;
    private const int maxStack = 4;
    private bool isSelected = false;

    void Awake()
    {
        if (background != null && normalBackground != null)
            background.sprite = normalBackground;

        if (icon != null)
            icon.enabled = false;
    }

    public void SetItem(ItemData newItem)
    {
        if (icon == null || countText == null || background == null)
        {
            Debug.LogError("❌ InventorySlot UI 참조 누락 (Inspector 연결 확인)");
            return;
        }

        currentItem = newItem;
        count = 1;

        icon.sprite = newItem.icon;
        icon.enabled = true;

        Refresh();
    }

    public void AddCount()
    {
        count++;
        Refresh();
    }

    public void RemoveCount(int amount)
    {
        count -= amount;
        if (count <= 0)
        {
            Clear();
        }
        else
        {
            Refresh();
        }
    }

    public void Clear()
    {
        currentItem = null;
        count = 0;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (countText != null)
            countText.text = "x0";

        // 선택 여부에 따라 배경 유지
        UpdateBackground();
    }

    public bool IsEmpty() => currentItem == null;
    public bool IsFull() => currentItem != null && count >= maxStack;
    public bool Matches(ItemData item) => currentItem != null && currentItem == item;
    public int GetCount() => count;
    public ItemData GetCurrentItem() => currentItem;

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        if (background != null)
        {
            background.sprite = isSelected ? selectedBackground : normalBackground;
        }
    }

    private void Refresh()
    {
        if (countText != null)
            countText.text = "x" + count;
    }
}
