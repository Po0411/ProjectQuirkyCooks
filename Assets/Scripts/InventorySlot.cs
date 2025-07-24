using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [Header("UI Components")]
    public Image backgroundImage;
    public Image itemImage;
    public TextMeshProUGUI amountText;

    [Header("Background Sprites")]
    public Sprite normalBackground;
    public Sprite selectedBackground;

    private ItemData currentItem;
    private int itemCount = 0;

    void Awake()
    {
        itemImage.sprite = null;
        itemImage.gameObject.SetActive(false);

        itemCount = 0;
        UpdateText(); // 꼭 유지
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;
        itemCount = 1;

        itemImage.sprite = item.icon;
        itemImage.gameObject.SetActive(true);
        UpdateText();
    }

    public void AddCount()
    {
        itemCount++;
        UpdateText();
    }

    public void RemoveCount(int count = 1)
    {
        itemCount = Mathf.Max(0, itemCount - count);
        if (itemCount == 0)
            ClearSlot();
        else
            UpdateText();
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemCount = 0;

        itemImage.sprite = null;
        itemImage.gameObject.SetActive(false);
        UpdateText(); // 반드시 유지
    }

    public void SetSelected(bool isSelected)
    {
        backgroundImage.sprite = isSelected ? selectedBackground : normalBackground;
    }

    public bool IsEmpty() => currentItem == null;
    public bool Matches(ItemData item) => currentItem == item;
    public bool IsFull() => currentItem != null && itemCount >= currentItem.maxStack;
    public int GetCount() => itemCount;
    public ItemData GetCurrentItem() => currentItem;

    private void UpdateText()
    {
        amountText.text = "x" + itemCount;
    }
}
