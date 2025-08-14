using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameplaySceneName = "GameMulti";

    [Header("Player Prefab (수동 스폰용)")]
    [SerializeField] private GameObject playerPrefab; // ← 인스펙터에 Player 프리팹 할당

    private NetworkManager NM => NetworkManager.Singleton;

    private void OnEnable()
    {
        if (NM != null)
        {
            NM.OnClientConnectedCallback += OnClientConnected;
            NM.OnClientDisconnectCallback += OnClientDisconnected;
            // NGO 1.8.1 시그니처
            NM.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        if (NM != null)
        {
            NM.OnClientConnectedCallback -= OnClientConnected;
            NM.OnClientDisconnectCallback -= OnClientDisconnected;
            NM.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        if (!IsServer()) return;
        if (to.name == gameplaySceneName)
            SpawnAllMissingPlayers();
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
                                      List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer()) return;
        if (sceneName == gameplaySceneName)
            SpawnAllMissingPlayers();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer()) return;
        if (IsInGameplay())
            SpawnIfMissing(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // 필요시 정리 로직 추가
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
            if (cc.PlayerObject != null) return; // 이미 스폰됨

            // 우선순위: 인스펙터 참조 → NetworkConfig.PlayerPrefab
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

            no.SpawnAsPlayerObject(clientId, destroyWithScene: true);
        }
    }

    private bool IsServer() => NM != null && NM.IsServer;
    private bool IsInGameplay() => SceneManager.GetActiveScene().name == gameplaySceneName;
}
