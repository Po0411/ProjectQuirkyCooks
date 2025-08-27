// Assets/Scripts/ItemData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 4;

    [Header("판매/배달 가격")]
    public int price = 10;                 // ✅ 배달 시 들어오는 금액

    public GameObject worldPrefab;         // 드랍될 실제 프리팹
}
