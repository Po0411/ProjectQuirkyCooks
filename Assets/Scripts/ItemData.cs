using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 4;

    public GameObject worldPrefab; // ✅ 반드시 GameObject 로 바꿔야 프리팹 등록 가능
}