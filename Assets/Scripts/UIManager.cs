// Assets/Scripts/UIManager.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Button readyButton;
    public Text lobbyCodeText;
    public Transform playerSlotParent;
    public GameObject playerSlotPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        readyButton.onClick.AddListener(() =>
        {
            var myPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject?.GetComponent<PlayerReady>();
            if (myPlayer != null)
            {
                bool newState = !myPlayer.isReady.Value;
                myPlayer.SetReadyServerRpc(newState);
            }
        });

        PlayerReady.OnAllPlayersReady += () =>
        {
            // 모든 플레이어가 준비되면 로비 데이터 업데이트 후 게임 씬 로드
            LobbyManager.Instance.UpdateLobbyData("start", "1");
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        };

        LobbyManager.Instance.OnLobbyUpdated += RefreshUI;
    }

    public void UpdateReadyState(PlayerReady playerReady)
    {
        // 각 플레이어 슬롯의 상태 갱신(예: 텍스트 색상 변경)
    }

    private void RefreshUI()
    {
        // 로비 코드 표시
        lobbyCodeText.text = "참여 코드: " + LobbyManager.Instance.joinedLobby.LobbyCode;
        // 플레이어 슬롯 동적 생성 및 상태 갱신
    }
}
