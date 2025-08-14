using UnityEngine;
using UnityEngine.UI;

// 씬에 배치해서 UI 레퍼런스를 RelayLobbyManager와 묶어주는 런타임 훅
public class RelayLobbyUIHook : MonoBehaviour
{
    [Header("Refs")]
    public RelayLobbyManager relay;   // 씬에 존재하는 RelayLobbyManager
    public InputField joinCodeInput;  // JoinCodeInput
    public Button btnCreateRoom;      // BtnCreateRoom
    public Button btnJoinRoom;        // BtnJoinRoom
    public Text statusText;           // StatusText (상태/코드 표기)

    private void Awake()
    {
        // 자동 탐색(비었을 경우)
        if (relay == null) relay = FindObjectOfType<RelayLobbyManager>();
    }

    private void OnEnable()
    {
        if (relay != null)
        {
            relay.OnStatusChanged += HandleStatus;
            relay.OnRoomCreated += HandleRoomCreated;
            relay.OnJoinSucceeded += HandleJoinSucceeded;
            relay.OnJoinFailed += HandleJoinFailed;
        }
    }

    private void OnDisable()
    {
        if (relay != null)
        {
            relay.OnStatusChanged -= HandleStatus;
            relay.OnRoomCreated -= HandleRoomCreated;
            relay.OnJoinSucceeded -= HandleJoinSucceeded;
            relay.OnJoinFailed -= HandleJoinFailed;
        }
    }

    private void Start()
    {
        if (btnCreateRoom != null)
        {
            btnCreateRoom.onClick.RemoveAllListeners();
            btnCreateRoom.onClick.AddListener(async () =>
            {
                if (relay == null) { HandleStatus("RelayLobbyManager 없음"); return; }
                var res = await relay.CreateLobbyAsync();
                if (!res.ok) HandleStatus("방 생성 실패: " + res.error);
                // 성공 시 HandleRoomCreated 통해 코드 표기됨
            });
        }

        if (btnJoinRoom != null)
        {
            btnJoinRoom.onClick.RemoveAllListeners();
            btnJoinRoom.onClick.AddListener(async () =>
            {
                if (relay == null) { HandleStatus("RelayLobbyManager 없음"); return; }
                var code = joinCodeInput != null ? joinCodeInput.text.Trim() : "";
                if (string.IsNullOrEmpty(code)) { HandleStatus("코드를 입력하세요"); return; }
                await relay.JoinWithCodeAsync(code);
            });
        }
    }

    private void HandleStatus(string msg)
    {
        if (statusText != null) statusText.text = msg ?? "";
    }

    private void HandleRoomCreated(string code)
    {
        if (statusText != null) statusText.text = $"참여 코드: {code}";
    }

    private void HandleJoinSucceeded()
    {
        HandleStatus("입장 성공");
    }

    private void HandleJoinFailed(JoinFailReason reason, string details)
    {
        HandleStatus($"입장 실패: {reason}");
    }
}
