using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyReadyManager : NetworkBehaviour
{
    [Header("Scene Names")]
    public string lobbySceneName = "ChannelScene";
    public string gameplaySceneName = "GameMulti";

    // 준비 상태 항목
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

    public NetworkList<Entry> Entries => _entries;

    private void Awake()
    {
        _entries = new NetworkList<Entry>();
    }

    private void OnEnable()
    {
        // 네트워크 매니저 콜백을 미리 구독(호스트 초기 타이밍 커버)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _entries.OnListChanged += _ => OnEntriesChanged?.Invoke();

        // 어떤 순서여도 서버가 현재 접속자들을 채우도록 보장
        if (IsServer)
        {
            SeedExistingClients();
        }

        // 초기 UI 타이밍 보정: 클라에서도 한 번 갱신 이벤트 쏘아주기
        OnEntriesChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _entries.OnListChanged -= _ => OnEntriesChanged?.Invoke();
    }

    // ---- 서버 전용 헬퍼 ----
    private void SeedExistingClients()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            AddOrUpdate(id, false);
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
        // 필요하면 게임 씬 진입 시 초기화
        // if (to.name == gameplaySceneName) _entries.Clear();
    }

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

    // ---- 퍼블릭 API ----
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
}
