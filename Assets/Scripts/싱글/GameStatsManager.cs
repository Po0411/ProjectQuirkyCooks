// Assets/Scripts/싱글/GameStatsManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    public string displayName = "Me";
    public int deaths;
    public long lossAmount;
    public long spendAmount;
    public int deliveryCount;
    public int craftCount;
}

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    [Header("초기 재정 설정")]
    public bool autoInitAtStart = true;
    public int startingCash = 50_000;   // 인스펙터에서 설정
    public int randomDebtMin = 50_000;   // 인스펙터에서 설정
    public int randomDebtMax = 200_000;  // 인스펙터에서 설정

    [Header("플레이어(싱글은 1명)")]
    public List<PlayerStats> players = new();
    public string localPlayerName = "Me";

    [Header("현재 재정(읽기 전용)")]
    [SerializeField] private long cash;     // 재산
    [SerializeField] private long debt;     // 채무(=목표금액)

    // 라운드 집계(원하면 UI에서 사용)
    [SerializeField] private long roundRevenue;    // 배달로 번 금액 합
    [SerializeField] private int roundDeliveries; // 라운드 내 배달 횟수

    public long Cash => cash;
    public long Debt => debt;
    public long RoundRevenue => roundRevenue;
    public int RoundDeliveries => roundDeliveries;

    public event Action OnStatsChanged;
    public event Action OnRoundEnded;

    private PlayerStats local;
    private bool financeInitialized;

    // ───────────────────────────── Lifecycle ─────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureLocalPlayer();
    }

    void Start()
    {
        if (autoInitAtStart && !financeInitialized)
        {
            SetStartMoney(startingCash, randomDebtMin, randomDebtMax);
            ResetRoundStats();
        }
    }

    // ───────────────────────────── Helpers ─────────────────────────────
    static long ClampLong(long v, long min, long max)
    {
        if (v < min) return min;
        if (v > max) return max;
        return v;
    }
    static int ClampToInt(long v)
    {
        if (v < int.MinValue) return int.MinValue;
        if (v > int.MaxValue) return int.MaxValue;
        return (int)v;
    }

    void EnsureLocalPlayer()
    {
        if (players == null) players = new List<PlayerStats>();
        if (players.Count == 0)
        {
            local = new PlayerStats { displayName = localPlayerName };
            players.Add(local);
        }
        else
        {
            local = players[0];
            if (string.IsNullOrEmpty(local.displayName))
                local.displayName = localPlayerName;
        }
    }

    // ───────────────────────── 재정 초기화/라운드 리셋 ─────────────────────────
    /// <summary>시작 현금/부채(랜덤 범위)를 세팅</summary>
    public void SetStartMoney(long startCash, long debtMin, long debtMax)
    {
        cash = Math.Max(0L, startCash);

        long dMin = Math.Min(debtMin, debtMax);
        long dMax = Math.Max(debtMin, debtMax);
        int iMin = ClampToInt(dMin);
        int iMax = ClampToInt(dMax);

        // UnityEngine.Random.Range(int,int)는 상한 제외 → +1
        debt = UnityEngine.Random.Range(iMin, iMax + 1);

        financeInitialized = true;
        OnStatsChanged?.Invoke();
    }

    /// <summary>라운드 집계 초기화</summary>
    public void ResetRoundStats()
    {
        roundRevenue = 0;
        roundDeliveries = 0;
        OnStatsChanged?.Invoke();
    }

    // ──────────────── ✅ 호환용: 예전 코드에서 부르던 이름 유지 ────────────────
    public void NewRoundInit() => NewRoundInit(null, null);

    public void NewRoundInit(int? overrideStartCash, int? overrideDebt)
    {
        long start = overrideStartCash.HasValue ? overrideStartCash.Value : startingCash;

        long debtVal;
        if (overrideDebt.HasValue)
        {
            debtVal = overrideDebt.Value;
        }
        else
        {
            int iMin = ClampToInt(Math.Min(randomDebtMin, randomDebtMax));
            int iMax = ClampToInt(Math.Max(randomDebtMin, randomDebtMax));
            debtVal = UnityEngine.Random.Range(iMin, iMax + 1);
        }

        cash = Math.Max(0L, start);
        debt = debtVal;

        ResetRoundStats();
        financeInitialized = true;
        OnStatsChanged?.Invoke();
    }
    // ────────────────────────────────────────────────────────────────────

    // ───────────────────────────── 재정 변경 ─────────────────────────────
    public void AddCashLocal(long amount)
    {
        if (amount == 0) return;
        cash += amount;
        OnStatsChanged?.Invoke();
    }
    public void SetCash(long value) { cash = value; OnStatsChanged?.Invoke(); }
    public void SetDebt(long value) { debt = value; OnStatsChanged?.Invoke(); }

    public void PayDebt(long amount)
    {
        amount = ClampLong(amount, 0, debt);
        debt -= amount;
        cash -= amount;
        OnStatsChanged?.Invoke();
    }

    // ───────────────────────────── 스탯 업데이트 ─────────────────────────────
    public void AddDeliveryLocal(int count = 1)
    {
        EnsureLocalPlayer();
        int add = Math.Max(0, count);
        local.deliveryCount += add;
        roundDeliveries += add;
        OnStatsChanged?.Invoke();
    }

    public void AddRevenue(long amount)
    {
        long add = Math.Max(0L, amount);
        roundRevenue += add;
        OnStatsChanged?.Invoke();
    }

    public void AddCraftLocal(int count = 1)
    {
        EnsureLocalPlayer();
        local.craftCount += Math.Max(0, count);
        OnStatsChanged?.Invoke();
    }

    public void AddDeathLocal(int count = 1)
    {
        EnsureLocalPlayer();
        local.deaths += Math.Max(0, count);
        OnStatsChanged?.Invoke();
    }

    public void AddLossLocal(long amount)
    {
        EnsureLocalPlayer();
        long amt = Math.Max(0L, amount);
        local.lossAmount += amt;
        cash -= amt;
        OnStatsChanged?.Invoke();
    }

    public void AddSpendLocal(long amount)
    {
        EnsureLocalPlayer();
        long amt = Math.Max(0L, amount);
        local.spendAmount += amt;
        cash -= amt;
        OnStatsChanged?.Invoke();
    }

    // ───────────────────────────── 라운드 종료 ─────────────────────────────
    public void EndRound()
    {
        OnRoundEnded?.Invoke();
    }
}
