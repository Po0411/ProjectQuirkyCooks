using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameplaySceneName = "GameMulti";

    [Header("Player Prefab (수동 스폰용)")]
    [SerializeField] private GameObject playerPrefab; // 인스펙터에서 Player 프리팹 연결

    private bool _hookedSceneManager;

    private NetworkManager NM => NetworkManager.Singleton;

    private void Awake()
    {
        // 씬 넘어도 유지되어야 스폰 담당을 계속할 수 있음
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        var nm = NM;
        if (nm != null)
        {
            nm.OnClientConnectedCallback += OnClientConnected;
            nm.OnClientDisconnectCallback += OnClientDisconnected;
            TryHookSceneManager();
        }
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        // 초기화 순서 대비해서 몇 프레임 지연 훅
        StartCoroutine(WaitAndHookSceneManager());
    }

    private System.Collections.IEnumerator WaitAndHookSceneManager()
    {
        for (int i = 0; i < 5 && !_hookedSceneManager; i++)
        {
            TryHookSceneManager();
            yield return null;
        }
    }

    private void OnDisable()
    {
        var nm = NM;
        if (nm != null)
        {
            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;

            if (_hookedSceneManager && nm.SceneManager != null)
                nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        _hookedSceneManager = false;
    }

    private void TryHookSceneManager()
    {
        var nm = NM;
        if (nm != null && nm.SceneManager != null && !_hookedSceneManager)
        {
            // NGO 1.8.1 시그니처
            nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            _hookedSceneManager = true;
        }
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        // 호스트만 스폰 수행
        if (!IsServer()) return;

        if (to.name == gameplaySceneName)
        {
            // 호스트가 자기 씬 바꾸는 즉시 1차 시도(느린 클라는 아래 콜백에서 보강)
            SpawnAllMissingPlayers();
        }
    }

    // 네트워크 씬 로드가 각 클라이언트에서 "완료"되었을 때 호출됨
    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
                                      List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer()) return;
        if (sceneName != gameplaySceneName) return;

        // 완료한 클라이언트만 안전하게 개별 스폰
        foreach (var clientId in clientsCompleted)
            SpawnIfMissing(clientId);

        // 혹시 누락 대비(네트워크 상태 등)로 한 번 더 전체 스윕
        SpawnAllMissingPlayers();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer()) return;
        // 게임 씬에 이미 있는 상태에서 뒤늦게 붙는 클라(리존/재접속 등) 대응
        if (IsInGameplay())
            SpawnIfMissing(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // 필요 시 정리 로직 추가 가능
    }

    private void SpawnAllMissingPlayers()
    {
        if (!IsServer()) return;

        foreach (var id in NM.ConnectedClientsIds)
            SpawnIfMissing(id);
    }

    private void SpawnIfMissing(ulong clientId)
    {
        if (!IsServer()) return;

        if (NM.ConnectedClients.TryGetValue(clientId, out var cc))
        {
            if (cc.PlayerObject != null) return; // 이미 스폰된 경우

            var prefab = playerPrefab != null ? playerPrefab : NM.NetworkConfig.PlayerPrefab;
            if (prefab == null)
            {
                Debug.LogError("[PlayerSpawner] Player prefab reference missing (inspector or NetworkConfig)");
                return;
            }

            var obj = Instantiate(prefab);
            var no = obj.GetComponent<NetworkObject>();
            if (no == null)
            {
                Debug.LogError("[PlayerSpawner] PlayerPrefab에 NetworkObject 필요");
                Destroy(obj);
                return;
            }

            // 게임 씬에서만 플레이어를 소환한다
            no.SpawnAsPlayerObject(clientId, destroyWithScene: true);
        }
    }

    private bool IsServer() => NM != null && NM.IsServer;
    private bool IsInGameplay() => SceneManager.GetActiveScene().name == gameplaySceneName;
}
