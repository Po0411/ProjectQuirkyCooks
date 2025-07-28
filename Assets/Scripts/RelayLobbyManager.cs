using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.UI;

public class RelayLobbyManager : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public InputField joinCodeInput;
    public Text joinCodeDisplay;

    private string lobbyId;
    public const int MaxPlayers = 40;

    async void Start()
    {
        // Unity 서비스 초기화
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in with PlayerID: " + AuthenticationService.Instance.PlayerId);
        }

        hostButton.onClick.AddListener(async () => await CreateLobby());
        joinButton.onClick.AddListener(async () => await JoinLobby(joinCodeInput.text));
    }

    // 호스트: 로비 생성 + Relay 할당
    private async Task CreateLobby()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeDisplay.text = "Join Code: " + joinCode;
            Debug.Log("Join Code: " + joinCode);

            // Lobby 생성
            var options = new CreateLobbyOptions
            {
                IsPrivate = false
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync("MyGameLobby", MaxPlayers, options);
            lobbyId = lobby.Id;

            // Relay 연결 설정
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lobby Create Failed: " + e.Message);
        }
    }

    // 클라이언트: Join Code로 접속
    private async Task JoinLobby(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lobby Join Failed: " + e.Message);
        }
    }
}
