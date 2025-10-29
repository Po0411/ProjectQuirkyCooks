using UnityEngine;

/// <summary>
/// 간단 업그레이드: 체력 슬롯 증가(최대치 up) + 풀회복.
/// 필요하면 스태미나/이속 등은 별도 스크립트에 맞춰 확장.
/// </summary>
public class NPCUpgrade : MonoBehaviour, IInteractable
{
    [Header("체력 슬롯 업그레이드 비용(단계별)")]
    public int[] hpSlotCosts = new int[] { 300, 600, 1000 };
    public int maxHpSlotsCap = 10;

    public string GetInteractText() => "업그레이드(체력 슬롯)";

    public void Interact() { Interact(FindObjectOfType<InventoryManager>()); }

    public void Interact(InventoryManager _)
    {
        var player = FindObjectOfType<PlayerHealth>();
        var gsm = FindObjectOfType<GameStatsManager>();
        if (player == null || gsm == null) { Debug.LogWarning("[NPCUpgrade] PlayerHealth/GameStatsManager 없음"); return; }

        // 현재 레벨: (현재 최대 슬롯 - 기본 5)로 추정 (원하면 별도 저장 로직 사용)
        int baseSlots = 5;
        int level = Mathf.Clamp(player.maxHpSlots - baseSlots, 0, hpSlotCosts.Length);
        if (level >= hpSlotCosts.Length)
        {
            Debug.Log("최대 업그레이드 도달");
            return;
        }

        int cost = hpSlotCosts[level];

        // 현금 체크/차감 (GameStatsManager 반영 헬퍼)
        if (!TrySpendCash(gsm, cost))
        {
            Debug.Log("골드 부족");
            return;
        }

        player.maxHpSlots = Mathf.Min(maxHpSlotsCap, player.maxHpSlots + 1);
        player.RestoreFullHp();
        Debug.Log($"체력 슬롯 업! 현재 최대 슬롯: {player.maxHpSlots}");
    }

    bool TrySpendCash(object gsm, int amount)
    {
        var t = gsm.GetType();
        var cashProp = t.GetProperty("Cash") ?? t.GetProperty("cash");
        if (cashProp == null) return false;

        long cash = System.Convert.ToInt64(cashProp.GetValue(gsm));
        if (cash < amount) return false;

        if (cashProp.CanWrite)
        {
            long next = cash - amount;
            cashProp.SetValue(gsm, System.Convert.ChangeType(next, cashProp.PropertyType));
            return true;
        }

        var spend = t.GetMethod("AddSpend") ?? t.GetMethod("SpendCash") ?? t.GetMethod("RemoveCash");
        if (spend != null) { spend.Invoke(gsm, new object[] { amount }); return true; }

        return false;
    }
}
