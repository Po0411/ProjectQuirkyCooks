using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Controls")]
    public Button readyButton;

    [Header("Texts")]
    public Text lobbyCodeText;

    [Header("Slots")]
    public Transform playerSlotParent;
    public GameObject playerSlotPrefab; // PlayerSlot 컴포넌트 포함 프리팹

    [Header("Scene")]
    public string gameplaySceneName = "GameScene";

    private RelayLobbyManager relay;
    private readonly List<PlayerSlot> _slots = new List<PlayerSlot>();
    private string _lastRoomCode = "";

    private void Awake()
    {
        Instance = this;
        if (relay == null) relay = FindObjectOfType<RelayLobbyManager>();
    }

    private void OnEnable()
    {
        PlayerReady.OnAllPlayersReady += HandleAllReady;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientChanged;
        }

        if (relay != null)
        {
            relay.OnRoomCreated += OnRoomCreated;   // 호스트: 방 생성 시 코드 표시
            relay.OnJoinSucceeded += OnJoinSucceeded; // 클라: 입장 성공 시 슬롯 갱신
        }
    }

    private void OnDisable()
    {
        PlayerReady.OnAllPlayersReady -= HandleAllReady;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientChanged;
        }

        if (relay != null)
        {
            relay.OnRoomCreated -= OnRoomCreated;
            relay.OnJoinSucceeded -= OnJoinSucceeded;
        }
    }

    private void Start()
    {
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnClickReady);
        }

        if (lobbyCodeText != null)
            lobbyCodeText.text = string.IsNullOrEmpty(_lastRoomCode)
                ? "코드를 입력하거나 방을 생성하세요"
                : $"참여 코드: {_lastRoomCode}";

        RefreshUI();
    }

    private void OnClickReady()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || nm.LocalClient == null || nm.LocalClient.PlayerObject == null)
        {
            Debug.LogWarning("[UIManager] Network not ready.");
            return;
        }

        var pr = nm.LocalClient.PlayerObject.GetComponent<PlayerReady>();
        if (pr != null) pr.ToggleReadyFromUI();
        else Debug.LogWarning("[UIManager] PlayerReady not found on PlayerPrefab.");
    }

    private void HandleAllReady()
    {
        // 호스트에서만 네트워크 씬 전환
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (!string.IsNullOrEmpty(gameplaySceneName))
                NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }

    private void OnRoomCreated(string code)
    {
        _lastRoomCode = code ?? "";
        if (lobbyCodeText != null) lobbyCodeText.text = $"참여 코드: {_lastRoomCode}";
        RefreshUI();
    }

    private void OnJoinSucceeded()
    {
        if (lobbyCodeText != null) lobbyCodeText.text = "방 입장 성공";
        RefreshUI();
    }

    private void OnClientChanged(ulong _) => RefreshUI();

    // 외부에서 호출해도 되도록 유지
    public void UpdateReadyState(PlayerReady _) => RefreshUI();

    private void RefreshUI()
    {
        var nm = NetworkManager.Singleton;
        int count = nm != null ? nm.ConnectedClientsList.Count : 0;
        EnsureSlotPool(count);

        if (nm == null)
        {
            for (int i = 0; i < _slots.Count; i++) _slots[i].ClearSlot();
            return;
        }

        var list = nm.ConnectedClientsList;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < list.Count)
            {
                var po = list[i].PlayerObject;
                var pr = po != null ? po.GetComponent<PlayerReady>() : null;

                string name = $"Player{i + 1}"; // 필요시 닉네임 시스템 연결
                bool ready = pr != null && pr.isReady.Value;

                _slots[i].SetPlayer(name, ready);
            }
            else
            {
                _slots[i].ClearSlot();
            }
        }
    }

    private void EnsureSlotPool(int count)
    {
        if (playerSlotParent == null || playerSlotPrefab == null) return;

        while (_slots.Count < count)
        {
            var go = Instantiate(playerSlotPrefab, playerSlotParent);
            var slot = go.GetComponent<PlayerSlot>();
            if (slot == null) slot = go.AddComponent<PlayerSlot>();
            _slots.Add(slot);
        }
        // 남는 슬롯은 RefreshUI에서 ClearSlot 처리
    }
}
