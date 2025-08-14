using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyReadyManager : NetworkBehaviour
{
    [Header("Scene Names")]
    public string lobbySceneName = "ChannelScene";
    public string gameplaySceneName = "GameMulti";

    // 로비 준비 상태 항목
    public struct Entry : INetworkSerializable, IEquatable<Entry>
    {
        public ulong ClientId;
        public bool Ready;

        public Entry(ulong id, bool ready) { ClientId = id; Ready = ready; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Ready);
        }

        public bool Equals(Entry other) => ClientId == other.ClientId;
    }

    public event Action OnEntriesChanged;

    private NetworkList<Entry> _entries;

    // ✅ NetworkList 그대로 노출 (인터페이스 캐스팅 하지 않음)
    public NetworkList<Entry> Entries => _entries;

    private void Awake()
    {
        _entries = new NetworkList<Entry>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _entries.OnListChanged += OnEntriesListChanged;

        if (IsServer)
        {
            // 현재 접속자 반영
            foreach (var id in NetworkManager.ConnectedClientsIds)
                AddOrUpdate(id, false);

            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (_entries != null)
            _entries.OnListChanged -= OnEntriesListChanged;

        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    private void OnEntriesListChanged(NetworkListEvent<Entry> _)
    {
        OnEntriesChanged?.Invoke();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        AddOrUpdate(clientId, false);
        TryStartGameIfAllReady();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        Remove(clientId);
    }

    private void OnSceneChanged(Scene from, Scene to)
    {
        if (!IsServer) return;
        // 게임 씬 진입 시 초기화가 필요하면 아래 주석 해제
        // if (to.name == gameplaySceneName) _entries.Clear();
    }

    // -------- Public API --------

    public bool TryGetLocalReady(out bool ready)
    {
        var localId = NetworkManager.LocalClientId;
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].ClientId == localId)
            {
                ready = _entries[i].Ready;
                return true;
            }
        }
        ready = false;
        return false;
    }

    public int ReadyCount
    {
        get { int c = 0; for (int i = 0; i < _entries.Count; i++) if (_entries[i].Ready) c++; return c; }
    }

    public int TotalCount => _entries.Count;

    public bool AllReady => (_entries.Count > 0) && (ReadyCount == _entries.Count);

    // 클라이언트 → 서버: 내 준비 토글
    public void ToggleReadyFromUI()
    {
        bool cur; TryGetLocalReady(out cur);
        SetReadyServerRpc(!cur);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(bool ready, ServerRpcParams rpc = default)
    {
        var sender = rpc.Receive.SenderClientId;
        AddOrUpdate(sender, ready);
        TryStartGameIfAllReady();
    }

    // -------- Server helpers --------

    private void AddOrUpdate(ulong clientId, bool ready)
    {
        if (!IsServer) return;

        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].ClientId == clientId)
            {
                _entries[i] = new Entry(clientId, ready);
                return;
            }
        }
        _entries.Add(new Entry(clientId, ready));
    }

    private void Remove(ulong clientId)
    {
        if (!IsServer) return;

        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].ClientId == clientId)
            {
                _entries.RemoveAt(i);
                return;
            }
        }
    }

    private void TryStartGameIfAllReady()
    {
        if (!IsServer) return;
        if (AllReady && IsInLobbyScene())
        {
            NetworkManager.SceneManager.LoadScene(
                gameplaySceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }

    private bool IsInLobbyScene()
        => string.Equals(SceneManager.GetActiveScene().name, lobbySceneName, StringComparison.Ordinal);
}
