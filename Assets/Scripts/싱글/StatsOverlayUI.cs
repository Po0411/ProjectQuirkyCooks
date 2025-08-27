using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Reflection;

/// TAB으로 현황판을 보였다/숨기는 간단 UI.
/// - GameStatsManager.Instance.players 가 배열이든 List든 모두 지원
/// - PlayerStats의 필드/프로퍼티 이름이 달라도(예: Name/playerName/displayName)
///   아래 후보 이름들 중에서 자동으로 찾아 표시
public class StatsOverlayUI : MonoBehaviour
{
    [Header("패널 루트(비워두면 자기 자신)")]
    public GameObject panel;

    [Header("핫키 설정")]
    public KeyCode hotkey = KeyCode.Tab;
    [Tooltip("키를 누르고 있는 동안만 표시")]
    public bool holdToShow = true;
    [Tooltip("시작 시 패널 숨기기")]
    public bool hideOnStart = true;

    [Header("Per-Player 텍스트(선택)")]
    public TMP_Text[] nameTexts;
    public TMP_Text[] deathTexts;     // 사망 횟수
    public TMP_Text[] lossTexts;      // 손실 금액
    public TMP_Text[] spendTexts;     // 사용 금액
    public TMP_Text[] deliveryTexts;  // 배달 횟수
    public TMP_Text[] craftTexts;     // 제작 횟수

    [Header("재정 텍스트(선택)")]
    public TMP_Text cashText;
    public TMP_Text debtText;

    void Awake()
    {
        if (panel == null) panel = gameObject;
        if (hideOnStart) panel.SetActive(false);
    }

    void Update()
    {
        if (holdToShow)
        {
            bool show = Input.GetKey(hotkey);
            if (panel.activeSelf != show)
            {
                panel.SetActive(show);
                if (show) RefreshTexts();
            }
        }
        else
        {
            if (Input.GetKeyDown(hotkey))
            {
                panel.SetActive(!panel.activeSelf);
                if (panel.activeSelf) RefreshTexts();
            }
        }

        // 패널이 열려 있을 때마다 계속 갱신하고 싶으면 주석 해제
        // if (panel.activeSelf) RefreshTexts();
    }

    void RefreshTexts()
    {
        var gs = GameStatsManager.Instance; // 프로젝트 쪽 매니저(없으면 그냥 리턴)
        if (gs == null) return;

        // ───────────────── 플레이어 목록 꺼내기(배열/리스트 둘 다 지원) ─────────────────
        object playersObj = GetFieldOrProperty(gs, "players", "Players", "playerList", "PlayerList");
        if (playersObj == null) return;

        if (playersObj is IList list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                object p = list[i];

                // 이름
                string name = GetString(p, "displayName", "DisplayName", "playerName", "PlayerName", "name", "Name");
                Set(nameTexts, i, name);

                // 사망 횟수
                int deaths = GetInt(p, "deaths", "Deaths", "deathCount", "DeathCount");
                Set(deathTexts, i, deaths.ToString());

                // 손실 금액
                long loss = GetLong(p, "lossAmount", "LossAmount", "loss", "Loss", "lossMoney", "LossMoney");
                Set(lossTexts, i, FormatWon(loss));

                // 사용 금액
                long spend = GetLong(p, "spendAmount", "SpendAmount", "spend", "Spend", "usedMoney", "UsedMoney");
                Set(spendTexts, i, FormatWon(spend));

                // 배달 횟수
                int delivery = GetInt(p, "deliveryCount", "DeliveryCount", "deliveries", "Deliveries");
                Set(deliveryTexts, i, delivery.ToString());

                // 제작 횟수
                int craft = GetInt(p, "craftCount", "CraftCount", "madeCount", "MadeCount", "produceCount", "ProduceCount");
                Set(craftTexts, i, craft.ToString());
            }
        }

        // ───────────────── 재정 ─────────────────
        long cash = GetLong(gs, "cash", "Cash", "money", "Money", "property", "Property");
        long debt = GetLong(gs, "debt", "Debt", "liability", "Liability");

        if (cashText) cashText.text = FormatWon(cash);
        if (debtText) debtText.text = FormatWon(debt);
    }

    // ───────────────────── 헬퍼 ─────────────────────
    void Set(TMP_Text[] arr, int idx, string val)
    {
        if (arr == null || idx < 0 || idx >= arr.Length) return;
        if (arr[idx]) arr[idx].text = val ?? string.Empty;
    }

    static string FormatWon(long v) => $"{v:N0}원";

    static object GetFieldOrProperty(object obj, params string[] names)
    {
        if (obj == null) return null;
        var type = obj.GetType();
        BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var n in names)
        {
            // 필드
            var fi = type.GetField(n, f);
            if (fi != null) return fi.GetValue(obj);

            // 대소문자 무시 필드
            foreach (var fi2 in type.GetFields(f))
                if (string.Equals(fi2.Name, n, StringComparison.OrdinalIgnoreCase))
                    return fi2.GetValue(obj);

            // 프로퍼티
            var pi = type.GetProperty(n, f);
            if (pi != null && pi.CanRead) return pi.GetValue(obj);

            // 대소문자 무시 프로퍼티
            foreach (var pi2 in type.GetProperties(f))
                if (string.Equals(pi2.Name, n, StringComparison.OrdinalIgnoreCase) && pi2.CanRead)
                    return pi2.GetValue(obj);
        }
        return null;
    }

    static string GetString(object obj, params string[] names)
    {
        var v = GetFieldOrProperty(obj, names);
        return v?.ToString() ?? string.Empty;
    }

    static int GetInt(object obj, params string[] names)
    {
        var v = GetFieldOrProperty(obj, names);
        if (v == null) return 0;
        if (v is int i) return i;
        if (v is long l) return (int)l;
        if (v is float f) return Mathf.RoundToInt(f);
        if (v is double d) return (int)Math.Round(d);
        if (int.TryParse(v.ToString(), out var p)) return p;
        return 0;
    }

    static long GetLong(object obj, params string[] names)
    {
        var v = GetFieldOrProperty(obj, names);
        if (v == null) return 0L;
        if (v is long l) return l;
        if (v is int i) return i;
        if (v is float f) return (long)Mathf.RoundToInt(f);
        if (v is double d) return (long)Math.Round(d);
        if (long.TryParse(v.ToString(), out var p)) return p;
        return 0L;
    }
}
