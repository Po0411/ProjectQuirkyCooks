// RoundTimerUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class RoundTimerUI : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] float durationSeconds = 90f;
    [SerializeField] bool autoStart = true;
    [SerializeField] bool loop = false;
    [SerializeField] UnityEvent onTimeUp;

    [Header("UI")]
    [SerializeField] Image fillImage;                 // 빨간 원(타이머 2)
    [SerializeField] TextMeshProUGUI timeText;        // 중앙 시간
    [SerializeField] RectTransform pointerRoot;       // 포인터 회전 기준
    [SerializeField] RectTransform pointerDot;        // 빨간 점
    [SerializeField] float pointerRadius = 52f;       // 점 반지름(픽셀)
    [SerializeField] bool smoothPointer = true;
    [SerializeField] float pointerLerp = 12f;

    float remain;
    bool running;

    void Reset() { ForceFillSettings(); }
    void OnValidate() { ForceFillSettings(); }

    void Awake()
    {
        ForceFillSettings();
        remain = durationSeconds;
        ApplyUI(1f);  // 시작 화면 갱신
    }

    void Start()
    {
        if (autoStart) StartTimer(durationSeconds);
    }

    // === 고정 세팅: Radial360 / Top / Clockwise Off ===
    void ForceFillSettings()
    {
        if (!fillImage) return;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = 2;          // Top
        fillImage.fillClockwise = false;   // CCW가 '채워짐' 방향
        fillImage.fillAmount = 1f;
    }

    public void StartTimer(float seconds)
    {
        durationSeconds = Mathf.Max(0.01f, seconds);
        remain = durationSeconds;
        running = true;
    }

    public void StopTimer(bool showZero = false)
    {
        running = false;
        if (showZero) { remain = 0f; ApplyUI(0f); }
    }

    void Update()
    {
        if (!running) return;

        remain -= Time.deltaTime;
        if (remain <= 0f)
        {
            remain = 0f;
            ApplyUI(0f);
            running = false;
            onTimeUp?.Invoke();
            if (loop) StartTimer(durationSeconds);
            return;
        }

        ApplyUI(remain / durationSeconds);
    }

    void ApplyUI(float ratioRemain)
    {
        // 1) 채우기(시계방향 Off, 원점=Top) → 남은 비율 그대로
        if (fillImage) fillImage.fillAmount = ratioRemain;

        // 2) 포인터(빨간 점) : 경과 비율만큼 '시계방향'으로 회전(=-angle)
        float elapsed = 1f - ratioRemain;
        float z = -elapsed * 360f;                 // 시계방향
        var target = Quaternion.Euler(0, 0, z);
        if (pointerRoot)
            pointerRoot.rotation = smoothPointer
                ? Quaternion.Lerp(pointerRoot.rotation, target, Time.deltaTime * pointerLerp)
                : target;

        if (pointerDot)                             // 점을 반지름 위치로 고정
            pointerDot.anchoredPosition = new Vector2(0f, pointerRadius);

        // 3) 텍스트
        if (timeText)
        {
            int sec = Mathf.CeilToInt(remain);
            int m = sec / 60;
            int s = sec % 60;
            timeText.text = $"{m:00}:{s:00}";
        }
    }
}
