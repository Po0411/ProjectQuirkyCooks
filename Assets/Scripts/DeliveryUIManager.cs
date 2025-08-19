using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

public class DeliveryUIManager : MonoBehaviour
{
    [Header("Item Source (둘 중 하나)")]
    public List<ItemData> manualItems = new();      // 인스펙터에서 넣는 방식
    public string resourcesFolder = "Items";        // Resources/<path>/* 자동 수집(비우면 미사용)

    [Header("Slots")]
    [Tooltip("슬롯들을 담고 있는 부모(예: Delivery). 지정하면 그 자식들을 자동으로 슬롯화합니다.")]
    public Transform slotsRoot;
    [Tooltip("씬에 DeliverySlot이 없다면 자식들에 자동으로 컴포넌트를 붙입니다.")]
    public bool autoAddSlotIfMissing = true;
    [Tooltip("슬롯들을 여기 수집합니다. 비워두면 자동 수집/생성합니다.")]
    public DeliverySlot[] slots;

    [Header("Options")]
    public bool uniqueItems = true;
    [Tooltip("네트워크가 실행중이 아닐 때, Start에서 자동으로 랜덤 채우기")]
    public bool fillLocalRandomOnStart = true;

    // 내부 카탈로그(정렬/중복 제거)
    private List<ItemData> _catalog;

    void Awake()
    {
        CollectOrCreateSlots();
        BuildCatalog();
    }

    void Start()
    {
        // 오프라인/싱글 테스트일 때 자동 채우기
        if (fillLocalRandomOnStart && !IsNetRunning())
        {
            ApplyLocalRandom();
        }
    }

    bool IsNetRunning()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
#else
        return false;
#endif
    }

    // ====== 슬롯 수집/생성 ======
    void CollectOrCreateSlots()
    {
        // 1) 우선 자식에서 DeliverySlot 수집
        var found = GetComponentsInChildren<DeliverySlot>(true).ToList();

        // 2) slotsRoot가 있고, 슬롯이 하나도 없으면 자동으로 붙이기
        if ((found == null || found.Count == 0) && slotsRoot != null && autoAddSlotIfMissing)
        {
            var list = new List<DeliverySlot>();
            for (int i = 0; i < slotsRoot.childCount; i++)
            {
                var child = slotsRoot.GetChild(i);
                // Label_* 같은 각 칸에 DeliverySlot이 없다면 추가
                var slot = child.GetComponent<DeliverySlot>();
                if (slot == null) slot = child.gameObject.AddComponent<DeliverySlot>();

                // 타겟 Image 자동 지정: 우선 "Image" 또는 "Icon" 자식, 없으면 본인 Image
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

        // 3) 인스펙터에 지정된 slots가 없으면 자동 수집 결과 사용
        if (slots == null || slots.Length == 0)
            slots = found.ToArray();
    }

    // ====== 아이템 카탈로그 구성 ======
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
            .GroupBy(i => i.itemName)     // 이름 중복 제거
            .Select(g => g.First())
            .OrderBy(i => i.itemName)     // 서버/클라 동일 순서 보장
            .ToList();

        if (_catalog.Count == 0)
            Debug.LogWarning("[DeliveryUI] 사용할 ItemData가 없습니다.");
    }

    public int CatalogCount => _catalog?.Count ?? 0;

    // ====== 랜덤 뽑기/적용 ======
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
            var idxs = Enumerable.Range(0, CatalogCount).ToList();
            for (int i = idxs.Count - 1; i > 0; --i)
            {
                int j = Random.Range(0, i + 1);
                (idxs[i], idxs[j]) = (idxs[j], idxs[i]);
            }
            for (int i = 0; i < count; i++) result.Add(idxs[i]);
        }
        else
        {
            for (int i = 0; i < count; i++)
                result.Add(Random.Range(0, CatalogCount));
        }
        return result;
    }

    public void ApplyLocalRandom()
    {
        var n = slots != null ? slots.Length : 0;
        ApplyByIndices(GenerateRandomIndices(n));
    }

    public void ApplyByIndices(IReadOnlyList<int> indices)
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            Sprite sp = null;
            if (indices != null && i < indices.Count && indices[i] >= 0 && indices[i] < CatalogCount)
                sp = _catalog[indices[i]].icon;
            slots[i]?.SetSprite(sp);
        }
    }

    // 에디터에서 즉시 테스트
    [ContextMenu("Apply Local Random (Now)")]
    void CM_ApplyLocalRandom() => ApplyLocalRandom();
}
