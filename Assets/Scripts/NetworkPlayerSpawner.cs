using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Settings")]
    public string gameplaySceneName = "TextScene";
    public GameObject playerPrefab;

    private readonly HashSet<ulong> _spawned = new HashSet<ulong>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnServerStarted() => TrySpawnAllIfInGameplay();
    private void OnClientConnected(ulong clientId) => TrySpawnIfInGameplay(clientId);
    private void OnClientDisconnected(ulong clientId) => _spawned.Remove(clientId);

    private void OnSceneChanged(Scene from, Scene to)
    {
        if (IsGameplay(to.name)) TrySpawnAllIfInGameplay();
        else _spawned.Clear();
    }

    private bool IsGameplay(string name) =>
        string.Equals(name, gameplaySceneName, System.StringComparison.Ordinal);

    private bool IsServer =>
        NetworkManager.Singleton && NetworkManager.Singleton.IsServer;

    private void TrySpawnAllIfInGameplay()
    {
        if (!IsServer || !IsGameplay(SceneManager.GetActiveScene().name)) return;
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            TrySpawn(id);
    }

    private void TrySpawnIfInGameplay(ulong clientId)
    {
        if (!IsServer || !IsGameplay(SceneManager.GetActiveScene().name)) return;
        TrySpawn(clientId);
    }

    private void TrySpawn(ulong clientId)
    {
        if (_spawned.Contains(clientId)) return;
        if (!playerPrefab) { Debug.LogError("[Spawner] Player Prefab is null"); return; }

        var obj = Instantiate(playerPrefab);
        var no = obj.GetComponent<NetworkObject>();
        if (!no) { Debug.LogError("[Spawner] Player Prefab needs NetworkObject"); Destroy(obj); return; }

        no.SpawnAsPlayerObject(clientId);
        _spawned.Add(clientId);
    }
}
