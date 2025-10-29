using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class CookingRecipe
{
    public ItemData rawIngredient;
    public ItemData cookedDish;
    public CookingMiniGameType miniGame;
}

public class AdvancedCookingStation : NetworkBehaviour, IInteractable
{
    [Header("Recipes")]
    public List<CookingRecipe> recipes = new List<CookingRecipe>();

    [Header("Mini-game Prefab")]
    [Tooltip("CookingMiniGame 컴포넌트를 포함한 프리팹. UI/수명주기는 자체 처리.")]
    public CookingMiniGame miniGamePrefab;

    private CookingMiniGame activeMiniGame;

    // ==== IInteractable ====
    public string GetInteractText() => "요리하기";
    public string Get_Result_Text() => "요리 완성!";

    // 무인자 Interact: InventoryManager를 찾아 위임
    public void Interact()
    {
        var inv = GetComponentInParent<InventoryManager>() ?? FindObjectOfType<InventoryManager>();
        if (inv == null)
        {
            Debug.LogWarning("[AdvancedCookingStation] InventoryManager를 찾지 못했습니다.");
            return;
        }
        Interact(inv);
    }

    // 인자있는 Interact: 실제 처리
    public void Interact(InventoryManager inv)
    {
        if (inv == null) return;
        if (activeMiniGame != null) return; // 이미 실행 중

        // 선택 아이템 조회
        var selected = GetSelectedItem(inv);
        if (selected == null) return;

        // 레시피 매칭
        var recipe = recipes.Find(r => r != null && r.rawIngredient == selected);
        if (recipe == null) return;

        // 미니게임 인스턴스 생성 및 시작
        activeMiniGame = Instantiate(miniGamePrefab);
        activeMiniGame.miniGameType = recipe.miniGame;
        activeMiniGame.OnCompleted.AddListener(success => OnMiniGameFinished(inv, recipe, success));
        activeMiniGame.StartMiniGame();
    }

    // ==== 미니게임 완료 처리 ====
    private void OnMiniGameFinished(InventoryManager inv, CookingRecipe recipe, bool success)
    {
        // 선택 재료 1개 소모
        ConsumeSelectedOne(inv, recipe.rawIngredient);

        if (success && recipe.cookedDish != null)
        {
            AddItemLocal(inv, recipe.cookedDish);
        }

        // 미니게임 정리
        if (activeMiniGame != null)
        {
            activeMiniGame.OnCompleted.RemoveAllListeners();
            Destroy(activeMiniGame.gameObject);
            activeMiniGame = null;
        }
    }

    // ==== 유틸: 선택 아이템 얻기 ====
    private ItemData GetSelectedItem(InventoryManager inv)
    {
        // 1) 명시적 GetSelectedItem()
        var m = inv.GetType().GetMethod("GetSelectedItem");
        if (m != null) return (ItemData)m.Invoke(inv, null);

        // 2) slots[selectedIndex].GetCurrentItem() 패턴
        try
        {
            var slotsF = inv.GetType().GetField("slots");
            var idxF = inv.GetType().GetField("selectedIndex");
            if (slotsF != null && idxF != null)
            {
                var slots = slotsF.GetValue(inv) as Array;
                var idx = (int)idxF.GetValue(inv);
                if (slots != null && idx >= 0 && idx < slots.Length)
                {
                    var slot = slots.GetValue(idx);
                    var getItemM = slot.GetType().GetMethod("GetCurrentItem");
                    if (getItemM != null) return (ItemData)getItemM.Invoke(slot, null);

                    var itemF = slot.GetType().GetField("item");
                    if (itemF != null) return (ItemData)itemF.GetValue(slot);
                }
            }
        }
        catch { /* fallthrough */ }

        return null;
    }

    // ==== 유틸: 선택 아이템 1개 소모 ====
    private void ConsumeSelectedOne(InventoryManager inv, ItemData item)
    {
        // 1) RemoveItemLocal(ItemData,int)
        var rmItemAmt = inv.GetType().GetMethod("RemoveItemLocal", new Type[] { typeof(ItemData), typeof(int) });
        if (rmItemAmt != null) { rmItemAmt.Invoke(inv, new object[] { item, 1 }); return; }

        // 2) RemoveItemLocal(int selectedIndex)
        var rmByIndex = inv.GetType().GetMethod("RemoveItemLocal", new Type[] { typeof(int) });
        var idxF = inv.GetType().GetField("selectedIndex");
        if (rmByIndex != null && idxF != null)
        {
            int idx = (int)idxF.GetValue(inv);
            rmByIndex.Invoke(inv, new object[] { idx });
            return;
        }

        Debug.LogWarning("[AdvancedCookingStation] InventoryManager.RemoveItemLocal 호출 실패(시그니처 불일치).");
    }

    // ==== 유틸: 아이템 추가 ====
    private void AddItemLocal(InventoryManager inv, ItemData item)
    {
        var add = inv.GetType().GetMethod("AddItemLocal", new Type[] { typeof(ItemData) });
        if (add != null) add.Invoke(inv, new object[] { item });
        else Debug.LogWarning("[AdvancedCookingStation] InventoryManager.AddItemLocal(ItemData) 를 찾지 못했습니다.");
    }
}
