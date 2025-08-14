using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameMultiUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject lobbyPanel;   // LobbyUI_Root
    public GameObject roomPanel;    // PlayerPanel

    [Header("Room UI")]
    public TMP_Text roomCodeText;   // StatusText
    public Button readyButton;    // PreparationButton
    public TMP_Text guideText;      // Guide Text

    [Header("Player Slots")]
    public PlayerSlot[] playerSlots; // Player1~Player4 (각각 PlayerSlot(TMP))

    private RelayLobbyManager relay;
    private LobbyReadyManager lobbyReady;

    // 비활성 객체도 포함해서 탐색
    private T FindAny<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
        return FindObjectOfType<T>(true);
#endif
    }

    private void Awake()
    {
        relay = FindAny<RelayLobbyManager>();
        lobbyReady = FindAny<LobbyReadyManager>();
    }

    private void OnEnable()
    {
        WireEvents(true);
    }

    private void OnDisable()
    {
        WireEvents(false);
    }

    private void WireEvents(bool on)
    {
        // Relay
        if (relay == null) relay = FindAny<RelayLobbyManager>();
        if (relay != null)
        {
            if (on)
            {
                relay.OnRoomCreated += OnRoomCreated;
                relay.OnJoinSucceeded += OnJoinSucceeded;
            }
            else
            {
                relay.OnRoomCreated -= OnRoomCreated;
                relay.OnJoinSucceeded -= OnJoinSucceeded;
            }
        }

        // LobbyReadyManager
        if (lobbyReady == null) lobbyReady = FindAny<LobbyReadyManager>();
        if (lobbyReady != null)
        {
            if (on) lobbyReady.OnEntriesChanged += RefreshSlots;
            else lobbyReady.OnEntriesChanged -= RefreshSlots;
        }
    }

    private void Start()
    {
        // 기본은 로비 화면
        ShowLobby(true);

        // 준비 버튼
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                // 매니저가 없으면 다시 탐색
                if (lobbyReady == null) lobbyReady = FindAny<LobbyReadyManager>();
                if (lobbyReady == null)
                {
                    Debug.LogWarning("[UI] LobbyReadyManager 없음");
                    return;
                }
                lobbyReady.ToggleReadyFromUI();
                RefreshSlots();
            });
        }

        // 네트워크 싱크 타이밍 보정: 1프레임 뒤 한번 더
        StartCoroutine(CoRefreshSoon());
    }

    private IEnumerator CoRefreshSoon()
    {
        yield return null; // 1 frame
        // 혹시 아직 못 찾았으면 재탐색
        if (lobbyReady == null) lobbyReady = FindAny<LobbyReadyManager>();
        RefreshSlots();
    }

    private void OnRoomCreated(string code)
    {
        if (roomCodeText) roomCodeText.text = $"참여 코드: {code}";
        ShowLobby(false);
        StartCoroutine(CoRefreshSoon());
    }

    private void OnJoinSucceeded()
    {
        if (roomCodeText) roomCodeText.text = "방 입장 성공";
        ShowLobby(false);
        StartCoroutine(CoRefreshSoon());
    }

    private void ShowLobby(bool showLobby)
    {
        if (lobbyPanel) lobbyPanel.SetActive(showLobby);
        if (roomPanel) roomPanel.SetActive(!showLobby);

        if (guideText)
            guideText.text = showLobby ? "" : "준비 버튼을 눌러주세요.";
    }

    // ==== 슬롯 갱신 ====
    public void RefreshSlots()
    {
        // 매니저 재확인
        if (lobbyReady == null) lobbyReady = FindAny<LobbyReadyManager>();

        if (playerSlots == null || playerSlots.Length == 0)
            return;

        if (lobbyReady == null || lobbyReady.Entries == null)
        {
            Debug.Log("[UI] entries null");
            for (int i = 0; i < playerSlots.Length; i++)
                if (playerSlots[i] != null) playerSlots[i].ClearSlot();
            return;
        }

        // NetworkList는 IReadOnlyList가 아니므로 수동 복사
        var list = new List<LobbyReadyManager.Entry>(lobbyReady.Entries.Count);
        for (int i = 0; i < lobbyReady.Entries.Count; i++)
            list.Add(lobbyReady.Entries[i]);

        // 디버그: 현재 인원 확인
        Debug.Log($"[UI] entries count = {list.Count}");

        // 오름차순 정렬(안전)
        list.Sort((a, b) => a.ClientId.CompareTo(b.ClientId));

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < list.Count)
            {
                var e = list[i];
                var slot = playerSlots[i];
                if (slot == null) continue;

                string display = $"Player{i + 1}";
                slot.SetPlayer(display, e.Ready, slot.avatarImage ? slot.avatarImage.sprite : null);
            }
            else
            {
                if (playerSlots[i] != null) playerSlots[i].ClearSlot();
            }
        }

        // 버튼 텍스트/가이드
        if (readyButton != null && lobbyReady.TryGetLocalReady(out bool myReady))
        {
            var txt = readyButton.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) txt.text = myReady ? "준비 해제" : "준비";
        }

        if (guideText != null)
            guideText.text = lobbyReady.AllReady ? "모든 플레이어 준비 완료!" : "준비 버튼을 눌러주세요.";
    }
}
