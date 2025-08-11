using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    // 이동할 씬 이름
    [SerializeField] private string sceneName;

    /// <summary>
    /// 버튼 클릭 시 호출되는 메서드
    /// </summary>
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
}
