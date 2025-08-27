using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettlementPanelUI : MonoBehaviour
{
    [Header("표시용")]
    public CanvasGroup panel;     // 없으면 SetActive로 대체
    public TMP_Text targetText;   // 목표 금액(=부채)
    public TMP_Text revenueText;  // 총수익(+)
    public TMP_Text cancelText;   // 주문취소(-) : 지금은 0
    public TMP_Text totalText;    // 합계
    public TMP_Text verdictText;  // 상환 성공/실패
    public Color successColor = new Color32(38, 180, 70, 255);
    public Color failColor = new Color32(236, 64, 64, 255);

    [Header("옵션")]
    public KeyCode closeKey = KeyCode.Return;
    public GameObject successFx;  // 선택
    public GameObject failFx;     // 선택

    long cancelPenalty = 0; // 훅만 남겨둠(지금은 0)

    void Awake()
    {
        Show(false);
    }

    void OnEnable()
    {
        var timer = FindObjectOfType<TimerUIController>(true);
        if (timer) timer.OnTimerFinished += Open;
        var gsm = GameStatsManager.Instance;
        if (gsm) gsm.OnRoundEnded += Open; // 안전망
    }

    void OnDisable()
    {
        var timer = FindObjectOfType<TimerUIController>(true);
        if (timer) timer.OnTimerFinished -= Open;
        var gsm = GameStatsManager.Instance;
        if (gsm) gsm.OnRoundEnded -= Open;
    }

    void Update()
    {
        if (panel && panel.alpha > 0.5f && Input.GetKeyDown(closeKey))
            Show(false);
    }

    public void Open()
    {
        var gsm = GameStatsManager.Instance;
        if (gsm == null) return;

        long target = gsm.Debt;
        long revenue = gsm.RoundRevenue;
        long total = revenue - cancelPenalty;
        bool success = total >= target;

        if (targetText) targetText.text = target.ToString("N0");
        if (revenueText) revenueText.text = $"+{revenue.ToString("N0")}";
        if (cancelText) cancelText.text = $"-{cancelPenalty.ToString("N0")}";
        if (totalText) totalText.text = total.ToString("N0");

        if (verdictText)
        {
            verdictText.text = success ? "상환 성공!" : "상환 실패!";
            verdictText.color = success ? successColor : failColor;
        }

        if (successFx) successFx.SetActive(success);
        if (failFx) failFx.SetActive(!success);

        Show(true);
    }

    void Show(bool on)
    {
        if (panel)
        {
            panel.alpha = on ? 1f : 0f;
            panel.blocksRaycasts = on;
            panel.interactable = on;
        }
        else
        {
            gameObject.SetActive(on);
        }
    }
}
