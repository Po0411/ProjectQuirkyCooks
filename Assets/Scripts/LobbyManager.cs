// Assets/Scripts/LobbyManager.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    public Lobby joinedLobby;
    public string playerName;
    public event Action OnLobbyUpdated;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        playerName = $"Player{Random.Range(1000, 9999)}";
    }

    public async Task CreateLobby(int maxPlayers)
    {
        try
        {
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false, // 코드로만 입장하려면 true 설정 후 코드 공유
                Data = new Dictionary<string, DataObject> {
                    { "start", new DataObject(DataObject.VisibilityOptions.Member, "0") } // 게임 시작 여부 플래그
                }
            };
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(playerName + "Lobby", maxPlayers, lobbyOptions);
            Debug.Log("로비 생성: " + joinedLobby.LobbyCode);
            OnLobbyUpdated?.Invoke();
            await RelayHost(); // Relay 서버 호스팅
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    public async Task JoinLobbyByCode(string code)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log("로비 참가 완료");
            OnLobbyUpdated?.Invoke();
            await RelayJoin(); // Relay 서버 접속
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    public async Task UpdateLobbyData(string key, string value)
    {
        try
        {
            var data = new Dictionary<string, DataObject> {
                { key, new DataObject(DataObject.VisibilityOptions.Member, value) }
            };
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions { Data = data });
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    public async Task RelayHost()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(joinedLobby.MaxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        // 로비 데이터에 Relay 코드 저장
        await UpdateLobbyData("relaycode", joinCode);
        // Netcode의 Transport 설정
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        NetworkManager.Singleton.StartHost();
    }

    public async Task RelayJoin()
    {
        string relayCode = joinedLobby.Data["relaycode"].Value;
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
        NetworkManager.Singleton.StartClient();
    }

    private void Update()  // 로비 상태 갱신 폴링
    {
        if (joinedLobby != null)
        {
            LobbyService.Instance.GetLobbyAsync(joinedLobby.Id).ContinueWith(task =>
            {
                if (task.Result != null) { joinedLobby = task.Result; OnLobbyUpdated?.Invoke(); }
            });
        }
    }
}
