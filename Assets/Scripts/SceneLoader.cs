using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public string SceneName;

    public void LoadSceneByIndex(int sceneIndex)
    {
        Debug.Log(sceneIndex + "씬으로 이동 하였습니다.");
        SceneManager.LoadScene(sceneIndex);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.CompareTag("Player"))
        {
            Debug.Log(SceneName + "씬으로 이동 하였습니다.");
            SceneManager.LoadScene(SceneName);
        }
    }
}
