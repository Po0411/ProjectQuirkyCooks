using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class EnsureMainCamera : MonoBehaviour
{
    void Awake()
    {
        if (Camera.main != null) return;

        var go = new GameObject("MenuCamera");
        var cam = go.AddComponent<Camera>();
        go.tag = "MainCamera";

        cam.orthographic = true;           // 메뉴용이면 직교가 편함
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        if (go.GetComponent<AudioListener>() == null)
            go.AddComponent<AudioListener>();
    }
}
