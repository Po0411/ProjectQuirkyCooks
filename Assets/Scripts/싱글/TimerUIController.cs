using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUIController : MonoBehaviour
{
    [Header("UI")]
    public Image fill;            // Radial360·상단·시계방향 꺼짐(=반시계) 고정
    public Image redDot;          // 빨간 점
    public TMP_Text timeText;

    [Header("세팅")]
    public float duration = 90f;  // 라운드 길이(초)

    float remain;
    bool running;

    public event Action OnTimerFinished;

    void OnEnable()
    {
        Begin(duration);
    }

    public void Begin(float sec)
    {
        remain = Mathf.Max(0, sec);
        running = true;
        UpdateVisual(force: true);
    }

    void Update()
    {
        if (!running) return;

        remain -= Time.deltaTime;
        if (remain <= 0f)
        {
            remain = 0f;
            running = false;
            UpdateVisual();
            OnTimerFinished?.Invoke();            // ✅ 라운드 끝 알림
            GameStatsManager.Instance?.EndRound(); // 브로드캐스트(정산창 등)
            return;
        }
        UpdateVisual();
    }

    void UpdateVisual(bool force = false)
    {
        if (fill)
        {
            float t = (duration <= 0 ? 0 : remain / duration); // 1→0
            fill.fillAmount = t;

            // 빨간 점 회전(상단에서 반시계로 이동, 이미지 세팅에 맞춤: Radial360/상단/시계방향 OFF)
            if (redDot)
            {
                float angle = Mathf.Lerp(360f, 0f, t); // 위에서 반시계로
                redDot.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        if (timeText)
        {
            int r = Mathf.CeilToInt(remain);
            int mm = Mathf.Clamp(r / 60, 0, 99);
            int ss = Mathf.Clamp(r % 60, 0, 59);
            timeText.text = $"{mm:00}:{ss:00}";
        }
    }
}
