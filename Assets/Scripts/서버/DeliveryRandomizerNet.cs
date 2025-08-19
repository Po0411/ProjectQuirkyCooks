// Assets/Scripts/서버/DeliveryRandomizerNet.cs
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬 진입 시 서버가 랜덤 인덱스를 뽑아 NetworkList로 공유.
/// 클라들은 변경 이벤트를 받아 UI에 반영.
/// </summary>
public class DeliveryRandomizerNet : NetworkBehaviour
{
    [Header("Bindings")]
    public DeliveryUIManager ui;                // DeliveryPanel 쪽 매니저
    [Tooltip("게임 플레이 씬 이름 (예: TextScene)")]
    public string gameplaySceneName = "TextScene";
    [Tooltip("슬롯 개수(비우면 UI의 슬롯 길이 사용)")]
    public int slotCountOverride = 0;

    private NetworkList<int> _indices;

    void Awake()
    {
        _indices = new NetworkList<int>();
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        _indices.OnListChanged += OnListChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        _indices.OnListChanged -= OnListChanged;
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!ui) ui = FindObjectOfType<DeliveryUIManager>(true);

        // 씬이 이미 게임 씬이면 즉시 시도
        if (IsServer && IsGameplay(SceneManager.GetActiveScene().name))
            ServerFillIfEmpty();

        if (!IsServer) OnListChanged(default); // 클라 초기 반영
    }

    void OnSceneChanged(Scene from, Scene to)
    {
        if (IsServer && IsGameplay(to.name))
            ServerFillIfEmpty();
    }

    bool IsGameplay(string sceneName) =>
        string.Equals(sceneName, gameplaySceneName, System.StringComparison.Ordinal);

    void ServerFillIfEmpty()
    {
        if (!IsServer) return;
        if (!ui) ui = FindObjectOfType<DeliveryUIManager>(true);
        if (!ui)
        {
            Debug.LogWarning("[DeliveryRand] DeliveryUIManager가 씬에 없습니다.");
            return;
        }

        // 카탈로그 최신화(리소스 자동 수집 등)
        ui.BuildCatalog();

        if (_indices.Count == 0)
        {
            int count = slotCountOverride > 0 ? slotCountOverride : (ui.slots?.Length ?? 0);
            var pick = ui.GenerateRandomIndices(count);
            _indices.Clear();
            for (int i = 0; i < pick.Count; i++) _indices.Add(pick[i]);
            Debug.Log($"[DeliveryRand] 서버 선택 인덱스: [{string.Join(",", pick)}]");
        }
    }

    void OnListChanged(NetworkListEvent<int> _)
    {
        if (!ui) ui = FindObjectOfType<DeliveryUIManager>(true);
        if (!ui) return;

        // 🔧 NetworkList<int> → List<int>로 수동 복사
        var copy = new List<int>(_indices.Count);
        for (int i = 0; i < _indices.Count; i++) copy.Add(_indices[i]);

        ui.ApplyByIndices(copy);
    }

    // 디버그용: 서버에서 재추첨
    [ContextMenu("Server Reroll (Debug)")]
    public void ServerReroll()
    {
        if (!IsServer || !ui) return;
        int count = slotCountOverride > 0 ? slotCountOverride : (ui.slots?.Length ?? 0);
        var pick = ui.GenerateRandomIndices(count);
        _indices.Clear();
        for (int i = 0; i < pick.Count; i++) _indices.Add(pick[i]);
        Debug.Log($"[DeliveryRand] 서버 재추첨: [{string.Join(",", pick)}]");
    }
}
