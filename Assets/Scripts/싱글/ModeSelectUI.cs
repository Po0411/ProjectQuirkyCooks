using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

public class ModeSelectUI : MonoBehaviour
{
    [SerializeField] string soloSceneName = "MainLevel";

    public GameObject soloButton;

    private AudioSource audioSource;

    public AudioClip hovering;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }



    public void OnClickSolo()
    {
        GameModeState.Current = GameMode.Solo;

        #if UNITY_NETCODE_GAMEOBJECTS
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        #endif
        SceneManager.LoadScene(soloSceneName, LoadSceneMode.Single);
    }

        // // (����) ��Ƽ ��ư��
        // public void OnClickMulti(string lobbySceneName = "ChannelScene")
        // {
        //     GameModeState.Current = GameMode.Multi;
        //     SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        // }
    
    
    public void Hovering()
    {
        soloButton.SetActive(true);
        audioSource.PlayOneShot(hovering);
    }

    public void Hide()
    {
        soloButton.SetActive(false);
    }
}
