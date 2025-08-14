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
    public TMP_Text roomCodeText;   // StatusText (TMP)
    public Button readyButton;    // PreparationButton
    public TMP_Text guideText;      // Guide Text (TMP)

    [Header("Player Slots")]
    public PlayerSlot[] playerSlots; // Player1~Player4 (TMP 버전)

    private RelayLobbyManager relay;
    private LobbyReadyManager lobbyReady;

    private void Awake()
    {
        relay = FindObjectOfType<RelayLobbyManager>();
        lobbyReady = FindObjectOfType<LobbyReadyManager>();
    }

    private void OnEnable()
    {
        if (relay != null)
        {
            relay.OnRoomCreated += OnRoomCreated;
            relay.OnJoinSucceeded += OnJoinSucceeded;
        }

        if (lobbyReady != null)
        {
            lobbyReady.OnEntriesChanged += RefreshSlots;
        }
    }

    private void OnDisable()
    {
        if (relay != null)
        {
            relay.OnRoomCreated -= OnRoomCreated;
            relay.OnJoinSucceeded -= OnJoinSucceeded;
        }

        if (lobbyReady != null)
        {
            lobbyReady.OnEntriesChanged -= RefreshSlots;
        }
    }

    private void Start()
    {
        ShowLobby(true);

        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                if (lobbyReady == null)
                {
                    Debug.LogWarning("[UI] LobbyReadyManager 없음");
                    return;
                }
                lobbyReady.ToggleReadyFromUI();
                RefreshSlots();
            });
        }

        RefreshSlots();
    }

    private void OnRoomCreated(string code)
    {
        if (roomCodeText != null) roomCodeText.text = $"참여 코드: {code}";
        ShowLobby(false);
        RefreshSlots();
    }

    private void OnJoinSucceeded()
    {
        if (roomCodeText != null) roomCodeText.text = "방 입장 성공";
        ShowLobby(false);
        RefreshSlots();
    }

    private void ShowLobby(bool showLobby)
    {
        if (lobbyPanel != null) lobbyPanel.SetActive(showLobby);
        if (roomPanel != null) roomPanel.SetActive(!showLobby);

        if (guideText != null)
            guideText.text = "준비 버튼을 눌러주세요.";
    }

    // ===== 슬롯 UI =====
    public void RefreshSlots()
    {
        if (playerSlots == null || playerSlots.Length == 0) return;

        // 로비 준비 목록을 그대로 사용
        if (lobbyReady == null || lobbyReady.Entries == null)
        {
            for (int i = 0; i < playerSlots.Length; i++)
                if (playerSlots[i] != null) playerSlots[i].ClearSlot();
            return;
        }

        // 클라이언트ID 오름차순으로 정렬된 가짜 리스트 만들기
        var list = new System.Collections.Generic.List<LobbyReadyManager.Entry>(lobbyReady.Entries.Count);
        for (int i = 0; i < lobbyReady.Entries.Count; i++)
            list.Add(lobbyReady.Entries[i]);

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

        // 버튼 텍스트/가이드 업데이트(선택)
        if (readyButton != null && lobbyReady.TryGetLocalReady(out bool myReady))
        {
            var txt = readyButton.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = myReady ? "준비 해제" : "준비";
        }

        if (guideText != null)
            guideText.text = lobbyReady.AllReady ? "모든 플레이어 준비 완료!" : "준비 버튼을 눌러주세요.";
    }
}
