// Assets/Scripts/RelayLobbyManager.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public enum JoinFailReason { None, InvalidCode, LobbyFull, CodeExpired, NetworkError, Unknown }

public class RelayLobbyManager : MonoBehaviour
{
    [Header("설정")]
    public int maxPlayers = 40;

    [Header("씬 설정")]
    [Tooltip("방 생성 후 머무를 로비(준비) 씬 이름")]
    public string lobbySceneName = "ChannelScene";
    [Tooltip("모든 준비 완료 시 이동할 게임 씬 이름")]
    public string gameSceneName = "TextScene"; // 필요 시 인스펙터에서 변경
    [Tooltip("이 필드는 더 이상 사용하지 않음(항상 네트워크로 재로드)")]
    public bool skipIfAlreadyInLobby = true;

    // UI 이벤트
    public event Action<string> OnStatusChanged;
    public event Action OnJoinSucceeded;
    public event Action<JoinFailReason, string> OnJoinFailed;
    public event Action<string> OnRoomCreated; // join code 전달

    private async void Awake()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayLobbyManager] Services init failed: {e.Message}");
            OnStatusChanged?.Invoke("네트워크 초기화 실패");
        }
    }

    /// <summary>
    /// 호스트 시작 + (항상) 로비 씬을 네트워크로 재로드
    /// </summary>
    public async Task<(bool ok, string joinCode, string error)> CreateLobbyAsync()
    {
        try
        {
            if (NetworkManager.Singleton == null)
                return (false, null, "씬에 NetworkManager가 없습니다.");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
                return (false, null, "NetworkManager에 UnityTransport가 없습니다.");

            OnStatusChanged?.Invoke("방 생성 중…");

            // Relay 할당 & 조인코드
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Relay 접속 정보 설정
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 🔴 자동 플레이어 스폰 비활성화 (NGO 1.8+엔 AutoCreatePlayer 토글 없음)
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;

            if (!NetworkManager.Singleton.StartHost())
                return (false, null, "StartHost 실패");

            OnStatusChanged?.Invoke($"방 생성 완료. 초대 코드: {joinCode}");
            OnRoomCreated?.Invoke(joinCode);

            // ✅ 항상 로비 씬을 '네트워크'로 다시 로드하여 씬 오브젝트(NetworkObject)들이 확실히 OnNetworkSpawn 타도록
            if (!string.IsNullOrEmpty(lobbySceneName))
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    lobbySceneName,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }

            return (true, joinCode, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayLobbyManager] CreateLobby failed: {e.Message}");
            OnStatusChanged?.Invoke("방 생성 실패");
            return (false, null, e.Message);
        }
    }

    /// <summary>
    /// 클라이언트로 참가
    /// </summary>
    public async Task<bool> JoinWithCodeAsync(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode) || joinCode.Length != 6)
        {
            OnStatusChanged?.Invoke("올바르지 않은 코드입니다");
            OnJoinFailed?.Invoke(JoinFailReason.InvalidCode, "format");
            return false;
        }

        try
        {
            if (NetworkManager.Singleton == null)
            {
                OnStatusChanged?.Invoke("NetworkManager 없음");
                return false;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                OnStatusChanged?.Invoke("UnityTransport 없음");
                return false;
            }

            OnStatusChanged?.Invoke("참가 시도 중…");

            // Relay 참가
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            transport.SetRelayServerData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.Key,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData
            );

            // 🔴 자동 플레이어 스폰 비활성화
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;

            if (!NetworkManager.Singleton.StartClient())
            {
                OnStatusChanged?.Invoke("클라이언트 시작 실패");
                OnJoinFailed?.Invoke(JoinFailReason.NetworkError, "start client");
                return false;
            }

            OnStatusChanged?.Invoke("방 입장 성공");
            OnJoinSucceeded?.Invoke();
            // 씬 전환은 호스트가 네트워크 씬 로드로 관리하므로 자동 동기화됨
            return true;
        }
        catch (RelayServiceException rse)
        {
            var m = rse.Message.ToLowerInvariant();
            JoinFailReason reason =
                m.Contains("invalid") ? JoinFailReason.InvalidCode :
                m.Contains("full") ? JoinFailReason.LobbyFull :
                m.Contains("expired") ? JoinFailReason.CodeExpired :
                (m.Contains("timeout") || m.Contains("unavailable")) ? JoinFailReason.NetworkError :
                JoinFailReason.Unknown;

            OnStatusChanged?.Invoke(reason switch
            {
                JoinFailReason.InvalidCode => "올바르지 않은 코드입니다",
                JoinFailReason.LobbyFull => "방이 가득 찼습니다",
                JoinFailReason.CodeExpired => "코드가 만료되었습니다",
                JoinFailReason.NetworkError => "네트워크 오류가 발생했습니다",
                _ => "참가 실패"
            });
            OnJoinFailed?.Invoke(reason, rse.Message);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayLobbyManager] Join failed: {e.Message}");
            OnStatusChanged?.Invoke("참가 실패");
            OnJoinFailed?.Invoke(JoinFailReason.Unknown, e.Message);
            return false;
        }
    }
}
