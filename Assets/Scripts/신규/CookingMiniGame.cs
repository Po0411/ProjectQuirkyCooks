using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Enumerates the different cooking mini‑games described in the design document. Each
/// value corresponds to a unique interaction (slicing, grilling, grinding, boiling,
/// frying, plating) that the player can perform when preparing dishes. These mini‑
/// games are deliberately abstracted here; the actual interactive logic (mouse
/// swipes, timing bars, etc.) should be implemented within the RunMiniGame coroutine
/// as appropriate for your project.
/// </summary>
public enum CookingMiniGameType
{
    Slice,   // 썰기
    Grill,   // 굽기
    Grind,   // 갈기
    Boil,    // 끓이기
    Fry,     // 튀기기
    Plate    // 플레이팅
}

/// <summary>
/// Represents a single cooking mini‑game session. When started, it runs for a
/// specified duration and then reports a success or failure outcome via the
/// OnCompleted event. The successThreshold controls the probability of success in
/// this simplified implementation. Developers should replace the contents of
/// RunMiniGame with actual mini‑game mechanics and determine success based on
/// player input rather than random chance.
/// </summary>
public class CookingMiniGame : MonoBehaviour
{
    [Header("Mini‑game Settings")]
    public CookingMiniGameType miniGameType = CookingMiniGameType.Slice;
    [Tooltip("How long the mini‑game lasts in seconds before determining a result.")]
    public float duration = 5f;
    [Tooltip("Probability between 0 and 1 that the mini‑game will succeed. Replace this with actual skill check logic.")]
    [Range(0f, 1f)]
    public float successThreshold = 0.8f;
    [Tooltip("Event invoked when the mini‑game finishes. The boolean indicates success (true) or failure (false).")]
    public UnityEvent<bool> OnCompleted;

    private bool running = false;

    /// <summary>
    /// Begins the mini‑game if it is not already in progress. This method can be
    /// called by an external script (e.g. a cooking station) when the player
    /// interacts with the station. Internally it starts a coroutine that waits
    /// for the specified duration and then determines the outcome.
    /// </summary>
    public void StartMiniGame()
    {
        if (running) return;
        running = true;
        StartCoroutine(RunMiniGame());
    }

    /// <summary>
    /// Coroutine that simulates the mini‑game. Replace the loop body with
    /// interactive mechanics (UI bars, button presses, etc.) to create a real
    /// gameplay experience. When the timer elapses, a success or failure is
    /// calculated using the successThreshold and the result is emitted via
    /// OnCompleted.
    /// </summary>
    private IEnumerator RunMiniGame()
    {
        float elapsed = 0f;
        // You can display UI or update progress here while the mini‑game runs.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Simple random success check. In a full implementation this would be
        // determined by player skill (e.g. accuracy of slicing, correct timing, etc.).
        bool success = Random.value <= successThreshold;
        OnCompleted?.Invoke(success);
        running = false;
    }
}