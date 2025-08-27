// Assets/Scripts/Boot/SoloUnpauseAndClean.cs
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-9999)]
public class SoloUnpauseAndClean : MonoBehaviour
{
    [SerializeField] bool destroyExtraCameras = true;

    void Awake()
    {
        // 1) 시간/물리/오디오 모두 정상값으로 복구
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Physics.autoSimulation = true;
#if UNITY_2023_1_OR_NEWER
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
#endif
        AudioListener.pause = false;

        // 2) (선택) 남아있는 메뉴 카메라 정리
        if (destroyExtraCameras)
        {
            var cams = FindObjectsOfType<Camera>(true);
            // 메인 하나만 남기고 나머지는 끄거나 삭제
            var main = Camera.main != null ? Camera.main : cams.FirstOrDefault();
            foreach (var c in cams)
            {
                if (main != null && c == main) continue;
                // 이름이 MenuCamera 같은 건 과감히 제거
                if (c.name.Contains("MenuCamera")) Destroy(c.gameObject);
                else c.enabled = false; // 안전하게 비활성
            }
        }

        Debug.Log($"[SoloFix] timescale={Time.timeScale}, autosim={Physics.autoSimulation}, cams={FindObjectsOfType<Camera>(true).Length}");
    }
}
