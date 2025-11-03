using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;
//using Unity.Netcode;

public enum CookingType { Boil, Fry, Chop, Blend, Grill }

public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("조리 도구 설정")]
    public CookingType type;
    public List<string> requiredItemName = new List<string>();//리스트로 변경
    public List<string> resultItemName = new List<string>();
    public string fail_result_item_name;
    public CookingMiniGame mini_game_sc;
    public float col_on_time;

    private InventoryManager In_Mg;
    public TextMeshProUGUI result_text;


    public string GetInteractText()
    {
        switch (type)
        {
            case CookingType.Boil: return "● 끓이기";
            case CookingType.Fry: return "● 튀기기";
            case CookingType.Chop: return "● 썰기";
            case CookingType.Blend: return "● 갈기";
            case CookingType.Grill: return "● 굽기(미완성)";
        }
        return "사용하기";
    }

    public string Get_Result_Text() => "조리 완료";

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
        string result_item = "";
        bool name_input = false;

        Debug.Log(input.itemName);

        if(result_text != null)result_text.gameObject.SetActive(true);
        Invoke("Text_off", 1f);

        if (is_result)
        {
            Debug.Log("성공");

            if (input == null)
            {
                Debug.Log($"❌ {requiredItemName} 없음");
                return;
            }

            for (int i = 0; i < requiredItemName.Count; i++)
            {
                Debug.Log(requiredItemName[i]);
                if (requiredItemName.Count != resultItemName.Count)
                {
                    Debug.LogWarning("결과 아이템과 요구 아이템의 갯수가 다릅니다! 변수를 확인해 주십시오.");
                    result_item = fail_result_item_name;
                    break;
                }

                if (input.itemName == requiredItemName[i])
                {
                    Debug.Log("통과");
                    result_item = resultItemName[i];
                    name_input = true;
                    break;
                }
            }
            if (!name_input)
            {
                Debug.Log("실패");
                result_item = fail_result_item_name;
            }

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

        Invoke("Collider_OFF", col_on_time);
    }


    private void Collider_OFF()
    {
        gameObject.GetComponentInChildren<BoxCollider>().enabled = true;
    }
    private void Text_off()
    {
        if (result_text != null) result_text.gameObject.SetActive(false);

    }
    //시작 -> 작동 -> 마무리를 나눠놓자
}
