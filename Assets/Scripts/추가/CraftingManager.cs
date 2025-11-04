using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingManager : MonoBehaviour
{
    [System.Serializable]
    public class Recipe
    {
        public string recipeName;
        public ItemData result;                 // 완성품
        public List<ItemData> ingredients;      // 필요한 재료 리스트
    }

    [Header("Recipes")]
    public List<Recipe> recipes;

    [Header("References")]
    public InventoryManager inventoryManager;   // 인벤토리 매니저
    public TextMeshProUGUI alertText;           // 알림용 텍스트
    public ItemData trashItem;                  // 실패 시 생성할 쓰레기

    [Header("Control Keys")]
    public KeyCode depositKey = KeyCode.E;      // 재료 투입
    public KeyCode craftKey = KeyCode.C;      // 제작 시도

    private bool isInZone = false;
    private List<ItemData> containerItems = new List<ItemData>();

    void Start()
    {
        if (alertText != null)
            alertText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isInZone) return;

        // 1) 재료 투입 (E)
        if (Input.GetKeyDown(depositKey))
            DepositItem();

        // 2) 제작 시도 (C)
        if (Input.GetKeyDown(craftKey) && recipes.Count > 0)
            TryCraft(recipes[0]);
    }

    void DepositItem()
    {
        var item = inventoryManager.GetSelectedItem();
        if (item == null)
        {
            ShowAlert("투입할 재료가 없습니다.");
            return;
        }
        if (inventoryManager.RemoveItemLocal(item, 1))
        {
            containerItems.Add(item);
            ShowAlert(item.itemName + " 투입 완료");
        }
        else
        {
            ShowAlert("재료 수량 부족");
        }
    }

    void TryCraft(Recipe recipe)
    {
        if (containerItems.Count == 0)
        {
            ShowAlert("재료를 투입하세요!");
            return;
        }

        foreach (var item in containerItems)
        {
            if (item.itemName == "햄버거" || item.itemName == "옥수수 수프" || item.itemName == "감자튀김" || item.itemName == "주스" || item.itemName == "샐러드" || item.itemName == "스테이크")
            {
                ShowAlert("완성된 음식은 조리할 수 없습니다!");
                ReturnAll();
                return;
            }

            if (item.itemName == "실패음식")
            {
                ShowAlert("쓰레기는 조리할 수 없습니다!");
                ReturnAll();
                return;
            }
        }

        // 투입된 개수 체크
        // if (containerItems.Count != recipe.ingredients.Count)
        // {
        //     ShowAlert("잘못된 조합! 쓰레기 생성");
        //     inventoryManager.AddItemLocal(trashItem);
        // }

        // 재료 매칭 체크
        var temp = new List<ItemData>(containerItems);
        bool match = true;
        foreach (var ing in recipe.ingredients)
        {
            if (!temp.Remove(ing))
            {
                match = false;
                break;
            }
        }

        // 결과 처리
        if (match)
        {
            inventoryManager.AddItemLocal(recipe.result);
            ShowAlert(recipe.result.itemName + " 제작 완료!");
        }
        else
        {
            inventoryManager.AddItemLocal(trashItem);
            ShowAlert("잘못된 조합! 쓰레기 생성");
        }

        containerItems.Clear();
    }

    void ReturnAll()
    {
        // 투입해둔 재료 전부 반환
        foreach (var ing in containerItems)
            inventoryManager.AddItemLocal(ing);
        containerItems.Clear();
    }

    void ShowAlert(string msg)
    {
        if (alertText == null)
        {
            Debug.LogWarning("CraftingManager: alertText 미할당 – 메시지: " + msg);
            return;
        }
        StopAllCoroutines();
        alertText.text = msg;
        alertText.gameObject.SetActive(true);
        StartCoroutine(ClearAlert());
    }

    IEnumerator ClearAlert()
    {
        yield return new WaitForSeconds(2f);
        if (alertText != null)
            alertText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CraftingZone"))
        {
            isInZone = true;
            ShowAlert("E : 재료투입 / C : 요리시작");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CraftingZone"))
        {
            isInZone = false;
            ReturnAll();
        }
    }
}