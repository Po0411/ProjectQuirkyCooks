using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    // 이동할 씬 이름
    [SerializeField] private string sceneName;

    public GameObject startButton;
    public GameObject quitButton;

    private AudioSource audioSource;

    public AudioClip hovering;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ 이동할 씬 이름이 설정되지 않았습니다.");
            return;
        }
        
        // 씬이 빌드 세팅에 포함되어 있어야 함
        SceneManager.LoadScene(sceneName);
    }

    public void ShowStartButton()
    {
        startButton.SetActive(true);
        audioSource.PlayOneShot(hovering);
    }

    public void HideStartButton()
    {
        startButton.SetActive(false);
    }
    
    public void ShowQuitButton()
    {
        quitButton.SetActive(true);
        audioSource.PlayOneShot(hovering);
    }

    public void HideQuitButton()
    {
        quitButton.SetActive(false);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}

