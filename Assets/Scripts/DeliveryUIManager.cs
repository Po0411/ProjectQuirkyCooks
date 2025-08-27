using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryUIManager : MonoBehaviour
{
    [Header("Item Source (둘 중 하나)")]
    public List<ItemData> manualItems = new();      // 인스펙터에서 직접 넣기
    public string resourcesFolder = "Items";        // Resources/<path>/* 자동 수집(비우면 미사용)

    [Header("Slots")]
    public Transform slotsRoot;                     // Delivery(부모)
    public bool autoAddSlotIfMissing = true;        // 자식에 DeliverySlot 자동 추가
    public DeliverySlot[] slots;                    // 비우면 자동 수집

    [Header("Options")]
    public bool uniqueItems = true;                 // 슬롯 간 중복 허용 여부
    public bool fillLocalRandomOnStart = true;      // 네트워크 미사용/싱글에서 자동 채우기

    [Header("Slot Colors (왼쪽 위부터 순서대로)")]
    public RegionColor[] slotColors;                // 각 슬롯의 색상(=지역) 매핑

    // 내부 상태
    private List<ItemData> _catalog;
    private int[] _indices;                         // 각 슬롯이 참조하는 카탈로그 인덱스

    public event Action OnRefreshed;                // UI가 재적용될 때 알림(손님이 아이콘 갱신 등에 사용)

    void Awake()
    {
        CollectOrCreateSlots();
        BuildCatalog();
    }

    void Start()
    {
        if (fillLocalRandomOnStart && !IsNetRunning())
            ApplyLocalRandom();
    }

    bool IsNetRunning()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        return Unity.Netcode.NetworkManager.Singleton != null
            && Unity.Netcode.NetworkManager.Singleton.IsListening;
#else
        return false;
#endif
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // 슬롯 수집/생성
    public void CollectOrCreateSlots()
    {
        var found = GetComponentsInChildren<DeliverySlot>(true).ToList();

        if ((found == null || found.Count == 0) && slotsRoot != null && autoAddSlotIfMissing)
        {
            var list = new List<DeliverySlot>();
            for (int i = 0; i < slotsRoot.childCount; i++)
            {
                var child = slotsRoot.GetChild(i);
                var slot = child.GetComponent<DeliverySlot>();
                if (slot == null) slot = child.gameObject.AddComponent<DeliverySlot>();

                if (slot.targetImage == null)
                {
                    var icon = child.Find("Image") ? child.Find("Image").GetComponent<UnityEngine.UI.Image>() : null;
                    if (icon == null && child.Find("Icon"))
                        icon = child.Find("Icon").GetComponent<UnityEngine.UI.Image>();
                    if (icon == null) icon = child.GetComponent<UnityEngine.UI.Image>();
                    if (icon == null) icon = child.GetComponentInChildren<UnityEngine.UI.Image>(true);
                    slot.targetImage = icon;
                }
                list.Add(slot);
            }
            found = list;
        }

        if (slots == null || slots.Length == 0)
            slots = found.ToArray();

        // 색상 배열 사이즈 보정
        if (slotColors == null || slotColors.Length != slots.Length)
        {
            slotColors = new RegionColor[slots.Length];
            for (int i = 0; i < slotColors.Length; i++)
                slotColors[i] = (RegionColor)(i % 4); // 기본 순환 배치
        }
    }

    // 카탈로그 구성
    public void BuildCatalog()
    {
        var list = new List<ItemData>();

        if (manualItems != null && manualItems.Count > 0)
            list.AddRange(manualItems.Where(i => i != null));

        if (!string.IsNullOrWhiteSpace(resourcesFolder))
        {
            var loaded = Resources.LoadAll<ItemData>(resourcesFolder);
            if (loaded != null && loaded.Length > 0)
                list.AddRange(loaded.Where(i => i != null));
        }

        _catalog = list
            .Where(i => i.icon != null)
            .GroupBy(i => i.itemName)        // 이름 중복 제거
            .Select(g => g.First())
            .OrderBy(i => i.itemName)        // 서버/클라 동일 순서 보장
            .ToList();

        if (_catalog.Count == 0)
            Debug.LogWarning("[DeliveryUI] 사용할 ItemData가 없습니다.");
    }

    public int CatalogCount => _catalog?.Count ?? 0;

    // ──────────────────────────────────────────────────────────────────────────────
    // 랜덤 생성/적용
    public void ApplyLocalRandom()
    {
        if (slots == null) return;
        var indices = GenerateRandomIndices(slots.Length);
        ApplyByIndices(indices);
    }

    public List<int> GenerateRandomIndices(int count)
    {
        var result = new List<int>(count);
        if (CatalogCount == 0 || count <= 0) return result;

        if (uniqueItems && CatalogCount < count)
        {
            Debug.LogWarning($"[DeliveryUI] 아이템 수({CatalogCount}) < 슬롯 수({count}), 중복 허용으로 전환합니다.");
            uniqueItems = false;
        }

        if (uniqueItems)
        {
            var pool = Enumerable.Range(0, CatalogCount).ToList();
            for (int i = pool.Count - 1; i > 0; --i)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            for (int i = 0; i < count; i++)
                result.Add(pool[i]);
        }
        else
        {
            for (int i = 0; i < count; i++)
                result.Add(UnityEngine.Random.Range(0, CatalogCount));
        }
        return result;
    }

    public void ApplyByIndices(IReadOnlyList<int> indices)
    {
        if (slots == null) return;

        if (_indices == null || _indices.Length != slots.Length)
            _indices = new int[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            int idx = (indices != null && i < indices.Count) ? indices[i] : -1;
            _indices[i] = idx;

            ItemData item = (idx >= 0 && idx < CatalogCount) ? _catalog[idx] : null;
            slots[i]?.SetItem(item);
        }

        OnRefreshed?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // 색상(지역) 기반 조회/재롤
    public int GetSlotIndexByColor(RegionColor color)
    {
        if (slotColors == null || slots == null) return -1;
        for (int i = 0; i < slots.Length; i++)
            if (slotColors[i] == color) return i;
        return -1;
    }

    public ItemData GetItemForColor(RegionColor color)
    {
        int i = GetSlotIndexByColor(color);
        if (i < 0 || i >= slots.Length) return null;
        return slots[i]?.AssignedItem;
    }

    public void RerollForColor(RegionColor color)
    {
        int i = GetSlotIndexByColor(color);
        if (i < 0) return;

        int newIdx = -1;
        if (CatalogCount > 0)
        {
            if (uniqueItems)
            {
                var used = new HashSet<int>(_indices.Where((x, k) => k != i && x >= 0));
                var candidates = Enumerable.Range(0, CatalogCount).Where(x => !used.Contains(x)).ToList();
                if (candidates.Count == 0) candidates = Enumerable.Range(0, CatalogCount).ToList();
                newIdx = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }
            else
            {
                newIdx = UnityEngine.Random.Range(0, CatalogCount);
            }
        }
        ApplyOne(i, newIdx);
    }

    void ApplyOne(int slotIndex, int catalogIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length) return;

        if (_indices == null || _indices.Length != slots.Length)
            _indices = new int[slots.Length];

        _indices[slotIndex] = catalogIndex;
        ItemData item = (catalogIndex >= 0 && catalogIndex < CatalogCount) ? _catalog[catalogIndex] : null;
        slots[slotIndex]?.SetItem(item);

        OnRefreshed?.Invoke();
    }

    // 배달 완료 후 호출하면 해당 색 슬롯만 새로 뽑아 UI 반영
    public void CompleteAndRerollForColor(RegionColor color) => RerollForColor(color);
}
