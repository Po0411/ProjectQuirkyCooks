using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

public class ModeSelectUI : MonoBehaviour
{
    [SerializeField] string soloSceneName = "SoloMapScene";

    // 싱글 카드/버튼 OnClick 에 연결
    public void OnClickSolo()
    {
        GameModeState.Current = GameMode.Solo;

        // 혹시 에디터에서 호스트/클라가 켜진 상태면 정리
#if UNITY_NETCODE_GAMEOBJECTS
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
#endif
        SceneManager.LoadScene(soloSceneName, LoadSceneMode.Single);
    }

    // (선택) 멀티 버튼용
    public void OnClickMulti(string lobbySceneName = "ChannelScene")
    {
        GameModeState.Current = GameMode.Multi;
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }
}
