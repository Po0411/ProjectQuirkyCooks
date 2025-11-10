using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

/// <summary>
/// 가진 현금 한도로 빚을 갚는다. GameStatsManager의 Cash/Debt가
/// int 또는 long이어도 동작하도록 리플렉션으로 처리.
/// </summary>
public class NPCDebt : MonoBehaviour, IInteractable
{
    public string GetInteractText() => "빚 상환";

    public string sceneName = "Clear";
    public TextMeshProUGUI result_text;
    private int loaded = 0;

    public string Get_Result_Text() => "빛 상환 완료";
    void Update()
    {   
        if(loaded == 0)
        {
            CheckForVictory();
        }
    }

    public void Interact()  // 편의
    {
        var inv = FindObjectOfType<InventoryManager>();
        Interact(inv);
    }

    public void Interact(InventoryManager _)
    {
        var gsm = FindObjectOfType<GameStatsManager>();
        if (gsm == null) { Debug.LogWarning("[NPCDebt] GameStatsManager 없음"); return; }

        var t = gsm.GetType();
        var cashProp = t.GetProperty("Cash") ?? t.GetProperty("cash");
        var debtProp = t.GetProperty("Debt") ?? t.GetProperty("debt");

        if (cashProp == null || debtProp == null) { Debug.LogWarning("[NPCDebt] Cash/Debt 프로퍼티 없음"); return; }

        long cash = Convert.ToInt64(cashProp.GetValue(gsm));
        long debt = Convert.ToInt64(debtProp.GetValue(gsm));
        if (cash <= 0 || debt <= 0) return;

        long pay = Math.Min(cash, debt);

        // Cash 감소
        if (cashProp.CanWrite)
        {
            long nextCash = Math.Max(0, cash - pay);
            cashProp.SetValue(gsm, Convert.ChangeType(nextCash, cashProp.PropertyType));
        }
        else
        {
            var spend = t.GetMethod("AddSpend") ?? t.GetMethod("SpendCash") ?? t.GetMethod("RemoveCash");
            if (spend != null) spend.Invoke(gsm, new object[] { (int)pay });
        }

        // Debt 감소 (읽기전용이면 메서드 탐색)
        if (debtProp.CanWrite)
        {
            long nextDebt = Math.Max(0, debt - pay);
            debtProp.SetValue(gsm, Convert.ChangeType(nextDebt, debtProp.PropertyType));
        }
        else
        {
            var reduce = t.GetMethod("PayDebt") ?? t.GetMethod("ReduceDebt") ?? t.GetMethod("AddDebt") /* 음수 허용 가정 */;
            if (reduce != null)
            {
                // AddDebt(-pay) 같은 시그니처 가정
                var p = reduce.GetParameters();
                if (p.Length == 1)
                    reduce.Invoke(gsm, new object[] { (int)(reduce.Name == "AddDebt" ? -pay : pay) });
            }
            else
            {
                Debug.LogWarning("[NPCDebt] Debt가 읽기전용이고 조정 메서드를 찾지 못했습니다.");
            }
        }
        if (result_text != null) result_text.text = Get_Result_Text();
        Invoke("Text_off", 1f);
    }

    // --- 추가된 함수: 빚이 0인지 확인 ---
    private void CheckForVictory()
    {
        if(GameStatsManager.debtReduce == true)
        {
            Debug.Log("🎉 빚을 모두 갚았습니다! 클리어 씬을 로드합니다.");
            LoadClearScene();
            loaded++;
        }
    }

    // --- 추가된 함수: 클리어 씬 로드 및 게임 중지 ---
    private void LoadClearScene()
    {
        // 2. 클리어 씬을 현재 씬 위에 추가(Additive) 로드하여 오버랩
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[NPCDebt] clearSceneName이 설정되지 않았습니다. 씬 이름을 Inspector에서 설정하세요.");
            return;
        }

        // 씬 빌드 설정에 'clearSceneName' 씬이 추가되어 있어야 합니다.
        SceneManager.LoadScene(sceneName);
        loaded++;
    }
    private void Text_off()
    {
        if (result_text != null) result_text.gameObject.SetActive(false);

    }
}
