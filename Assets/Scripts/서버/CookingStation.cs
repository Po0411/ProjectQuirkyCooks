using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Windows;
//using Unity.Netcode;

public enum CookingType { Boil, Fry, Chop, Blend, Grill }

public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("조리 도구 설정")]
    public CookingType type;
    public string requiredItemName;//리스트로 변경
    public string resultItemName;
    public string fail_result_item_name;
    public CookingMiniGame mini_game_sc;

    private InventoryManager In_Mg;
    public string GetInteractText()
    {
        switch (type)
        {
            case CookingType.Boil: return "● 끓이기";
            case CookingType.Fry: return "● 튀기기";
            case CookingType.Chop: return "● 썰기";
            case CookingType.Blend: return "● 갈기";
            case CookingType.Grill: return "● 굽기";
        }
        return "사용하기";
    }

    // InventoryManager를 매개변수로 받는 Interact
    public void Interact(InventoryManager inventory)
    {
        if (inventory == null) return;
        if (inventory.slots[inventory.selectedIndex].currentItem == null) return;

        mini_game_sc.OnCompleted.AddListener(Result_Chake);//이벤트 구독
        gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
        mini_game_sc.StartMiniGame(); 

        In_Mg = inventory;

    }

    // 매개변수 없는 Interact (인터페이스 요구사항 충족)
    public void Interact()
    {
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
            Interact(inv);
        else
            Debug.LogError("❌ InventoryManager를 찾을 수 없음");
    }

    void Result_Chake(bool is_result)
    {

        //ItemData input = In_Mg.GetItemByName(requiredItemName);
        ItemData input = In_Mg.slots[In_Mg.selectedIndex].currentItem;

        Debug.Log(input);
        string result_item;


        if (is_result)
        {
            Debug.Log("성공");

            if (input == null)
            {
                Debug.Log($"❌ {requiredItemName} 없음");
                return;
            }

            if (input.itemName != requiredItemName) result_item = fail_result_item_name;
            else result_item = resultItemName;

            if (!In_Mg.RemoveItemLocal(input, 1))
            {
                Debug.Log($"❌ {requiredItemName} 제거 실패");
                return;
            }

            ItemData output = In_Mg.GetItemByName(result_item);
            if (output != null)
            {
                In_Mg.AddItemLocal(output);
                Debug.Log($"✅ {requiredItemName} → {result_item} 변환 완료");
            }
            else
            {
                Debug.LogError($"❌ {result_item} 아이템 데이터를 찾을 수 없음");
            }
        }
        else Debug.Log("실패");

        gameObject.GetComponentInChildren<BoxCollider>().enabled = true;
    }

    //시작 -> 작동 -> 마무리를 나눠놓자
}
