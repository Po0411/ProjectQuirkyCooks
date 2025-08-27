using UnityEngine;

public class SoloRoundDirector : MonoBehaviour
{
    public int initialCash = 0;
    public Vector2Int debtRange = new Vector2Int(50000, 80000);
    public float roundSeconds = 90f;

    void Start()
    {
        GameStatsManager.Instance?.NewRoundInit(initialCash,
            Random.Range(debtRange.x, debtRange.y + 1));

        var timer = FindObjectOfType<TimerUIController>(true);
        if (timer) timer.Begin(roundSeconds);
    }
}
