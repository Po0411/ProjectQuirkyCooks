using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

/// <summary>
/// GameMulti 씬에서 로비 생성/참가 UI와 플레이어 준비 상태를 관리합니다.
/// LobbyManager 및 PlayerReady 스크립트와 연동하여 방 입장, 코드 표시, 플레이어 슬롯 활성화/비활성화를 처리합니다.
/// </summary>
public class GameMultiUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject lobbyPanel;    // 방 생성/방 찾기 화면
    public GameObject roomPanel;     // 방 내부 화면

    [Header("Lobby Panel References")]
    public InputField joinCodeInput; // 초대 코드 입력 필드
    public Button createButton;      // 방 생성 버튼
    public Button joinButton;        // 방 참가 버튼

    [Header("Room Panel References")]
    public Text roomCodeText;        // 참여 코드 표시
    public PlayerSlot[] playerSlots; // Player1~Player4 슬롯
    public Button readyButton;       // 준비 버튼
    public Text guideText;           // 안내 문구

    private void Start()
    {
        // 방 생성 버튼: 최대 4인 로비 생성
        if (createButton != null)
        {
            createButton.onClick.RemoveAllListeners();
            createButton.onClick.AddListener(() =>
            {
                LobbyManager.Instance.CreateLobby(4);
            });
        }

        // 방 참가 버튼: 입력한 코드로 로비 참가
        if (joinButton != null && joinCodeInput != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                string code = joinCodeInput.text.Trim();
                if (!string.IsNullOrEmpty(code))
                {
                    LobbyManager.Instance.JoinLobbyByCode(code);
                }
            });
        }

        // 준비 버튼: 자신의 준비 상태 토글
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                // 로컬 플레이어의 PlayerReady 컴포넌트를 찾아 서버 RPC 호출
                var client = NetworkManager.Singleton.LocalClient;
                if (client != null && client.PlayerObject != null)
                {
                    var readyComp = client.PlayerObject.GetComponent<PlayerReady>();
                    if (readyComp != null)
                    {
                        bool newState = !readyComp.isReady.Value;
                        readyComp.SetReadyServerRpc(newState);
                    }
                }
            });
        }

        // 로비 정보 갱신 시 UI 업데이트
        LobbyManager.Instance.OnLobbyUpdated += RefreshRoomUI;
        // 모든 플레이어 준비 완료 시 게임 씬으로 전환
        PlayerReady.OnAllPlayersReady += OnAllPlayersReady;

        // 초기 UI 갱신
        RefreshRoomUI();
    }

    /// <summary>
    /// 로비 정보가 바뀔 때 방 패널과 플레이어 슬롯을 갱신합니다.
    /// </summary>
    private void RefreshRoomUI()
    {
        var lobby = LobbyManager.Instance.joinedLobby;
        // 로비에 속해 있지 않으면 로비 패널을 표시하고 끝냅니다
        if (lobby == null)
        {
            if (lobbyPanel != null) lobbyPanel.SetActive(true);
            if (roomPanel != null) roomPanel.SetActive(false);
            return;
        }

        // 방 내부 패널 표시
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (roomPanel != null) roomPanel.SetActive(true);

        // 참여 코드 표시
        if (roomCodeText != null)
        {
            roomCodeText.text = $"참여 코드: {lobby.LobbyCode}";
        }

        // 안내 문구 업데이트
        if (guideText != null)
        {
            guideText.text = "모든 플레이어가 준비되면 게임이 시작됩니다!";
        }

        // 플레이어 슬롯 갱신
        if (playerSlots != null)
        {
            var players = lobby.Players;
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (i < players.Count)
                {
                    var p = players[i];
                    // 플레이어 표시 이름: Lobby 데이터에 playerName이 있다면 사용
                    string displayName = p.Data != null && p.Data.ContainsKey("playerName") ?
                        p.Data["playerName"].Value : $"Player{i + 1}";
                    // 준비 상태: Lobby 데이터에 isReady가 있다면 사용 ("1" == 준비)
                    bool ready = false;
                    if (p.Data != null && p.Data.ContainsKey("isReady"))
                    {
                        ready = p.Data["isReady"].Value == "1";
                    }
                    // 아바타 스프라이트는 슬롯에 이미 지정된 것을 재사용
                    var slot = playerSlots[i];
                    if (slot != null)
                    {
                        Sprite avatar = null;
                        if (slot.avatarImage != null)
                            avatar = slot.avatarImage.sprite;
                        slot.SetPlayer(displayName, ready, avatar);
                    }
                }
                else
                {
                    // 플레이어가 없으면 슬롯 비활성화
                    if (playerSlots[i] != null)
                    {
                        playerSlots[i].ClearSlot();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 플레이어가 준비된 경우 호출되어 게임 씬을 로드합니다.
    /// </summary>
    private void OnAllPlayersReady()
    {
        // 로비 데이터 업데이트(예: "start" = "1") 등 필요하면 실행하고,
        // 게임 씬으로 전환
        LobbyManager.Instance.UpdateLobbyData("start", "1");
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
