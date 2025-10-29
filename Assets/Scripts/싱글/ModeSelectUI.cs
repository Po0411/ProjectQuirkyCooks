using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

public class ModeSelectUI : MonoBehaviour
{
    [SerializeField] string soloSceneName = "MainLevel";

    // �̱� ī��/��ư OnClick �� ����
    public void OnClickSolo()
    {
        GameModeState.Current = GameMode.Solo;

        // Ȥ�� �����Ϳ��� ȣ��Ʈ/Ŭ�� ���� ���¸� ����
#if UNITY_NETCODE_GAMEOBJECTS
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
#endif
        SceneManager.LoadScene(soloSceneName, LoadSceneMode.Single);
    }

    // (����) ��Ƽ ��ư��
    public void OnClickMulti(string lobbySceneName = "ChannelScene")
    {
        GameModeState.Current = GameMode.Multi;
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }
}
