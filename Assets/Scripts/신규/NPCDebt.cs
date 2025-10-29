using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 가진 현금 한도로 빚을 갚는다. GameStatsManager의 Cash/Debt가
/// int 또는 long이어도 동작하도록 리플렉션으로 처리.
/// </summary>
public class NPCDebt : MonoBehaviour, IInteractable
{
    public TextMeshProUGUI result_text;

    public string GetInteractText() => "빚 상환";

    public string Get_Result_Text() => "상환 완료!";

    public void Interact()  // 편의
    {
        var inv = FindObjectOfType<InventoryManager>();
        Interact(inv);
    }

    public void Interact(InventoryManager _)
    {
        result_text.gameObject.SetActive(true);
        Invoke("Inveke", 1);
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
    }
    private void Text_off()
    {
        result_text.gameObject.SetActive(false);

    }
}
