using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 시간/패널티/팁 처리 + 현금 반영(리플렉션 헬퍼로 GameStatsManager와 호환)
/// </summary>
public class DeliveryManager : MonoBehaviour
{
    [Header("제한시간(초) / 패널티")]
    public int timeLimitSeconds = 240;   // 4분
    public int wrongItemPenalty = 50;

    [Header("팁 확률(금액→확률%)")]
    public Dictionary<int, int> tipTable = new Dictionary<int, int>()
    {
        { 5, 80 }, { 10, 15 }, { 15, 5 }, { 40, 1 }
    };

    [Header("가격표(예시)")]
    public Dictionary<string, int> priceTable = new Dictionary<string, int>()
    {
        { "사과주스", 30 }, { "스테이크", 30 }, { "감자튀김", 35 },
        { "스프", 60 }, { "샐러드", 70 }, { "햄버거", 150 }
    };

    private DateTime orderStart;
    private string currentOrderKey = null;

    public void StartOrder(string orderKey)
    {
        currentOrderKey = orderKey;
        orderStart = DateTime.UtcNow;
    }

    public void CancelOrder()
    {
        currentOrderKey = null;
    }

    public void TryCompleteOrder(string deliveredKey)
    {
        if (string.IsNullOrEmpty(currentOrderKey)) return;

        bool inTime = (DateTime.UtcNow - orderStart).TotalSeconds <= timeLimitSeconds;
        if (deliveredKey != currentOrderKey)
        {
            ApplyCashDelta(-wrongItemPenalty);
            currentOrderKey = null;
            return;
        }

        int basePrice = priceTable.TryGetValue(deliveredKey, out var price) ? price : 0;
        int tip = inTime ? RollTip() : 0;

        ApplyCashDelta(basePrice + tip);
        currentOrderKey = null;
    }

    int RollTip()
    {
        // 누적확률 방식
        int total = 0;
        foreach (var kv in tipTable) total += kv.Value;
        if (total <= 0) return 0;

        int r = UnityEngine.Random.Range(1, total + 1);
        int acc = 0;
        foreach (var kv in tipTable)
        {
            acc += kv.Value;
            if (r <= acc) return kv.Key;
        }
        return 0;
    }

    // ===== GameStatsManager 현금 반영(반드시 여기만 사용) =====
    void ApplyCashDelta(int delta)
    {
        var gsm = FindObjectOfType<GameStatsManager>();
        if (gsm == null)
        {
            Debug.LogWarning("[DeliveryManager] GameStatsManager를 찾지 못함");
            return;
        }

        var t = gsm.GetType();

        // 1) Cash 프로퍼티 시도 (long/int 모두 대응)
        var cashProp = t.GetProperty("Cash") ?? t.GetProperty("cash");
        if (cashProp != null && cashProp.CanRead && cashProp.CanWrite)
        {
            long cur = Convert.ToInt64(cashProp.GetValue(gsm, null));
            long next = Math.Max(0, cur + delta);
            object boxed = Convert.ChangeType(next, cashProp.PropertyType);
            cashProp.SetValue(gsm, boxed, null);
            return;
        }

        // 2) 증가/감소 메서드 시도
        var add = t.GetMethod("AddCash") ?? t.GetMethod("AddMoney") ?? t.GetMethod("AddRevenue");
        if (add != null) { add.Invoke(gsm, new object[] { delta }); return; }

        var spend = t.GetMethod("AddSpend") ?? t.GetMethod("SpendCash") ?? t.GetMethod("RemoveCash");
        if (spend != null) { spend.Invoke(gsm, new object[] { -delta }); return; }

        Debug.LogWarning("[DeliveryManager] GameStatsManager에 현금 반영 API를 찾지 못했습니다.");
    }
}
