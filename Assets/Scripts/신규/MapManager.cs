using UnityEngine;

/// <summary>
/// Divides the world into a central safe zone and three coloured regions (pink,
/// blue and green) as described in the design document. Each region is
/// represented by a Collider assigned in the inspector. Other systems can
/// query the MapManager singleton to determine in which region a given
/// position lies. Additional behaviours such as random tree/bush spawning
/// could be added here in the future.
/// </summary>
public class MapManager : MonoBehaviour
{
    public enum Zone
    {
        None,
        Safe,
        Pink,
        Blue,
        Green
    }

    [Header("Zone Colliders")]
    [Tooltip("Collider representing the safe central area where players regenerate health.")]
    public Collider safeZone;
    [Tooltip("Collider for the pink region where potatoes and tomatoes spawn.")]
    public Collider pinkZone;
    [Tooltip("Collider for the blue region where apples and wheat spawn.")]
    public Collider blueZone;
    [Tooltip("Collider for the green region where cabbages and corn spawn.")]
    public Collider greenZone;

    public static MapManager Instance { get; private set; }

    private void Awake()
    {
        // Establish a singleton for easy access. If one already exists, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Returns which zone the provided world position belongs to. Returns None
    /// when the position does not fall into any defined zone. Bounds checks
    /// ensure we only depend on Collider bounds rather than colliding physics.
    /// </summary>
    public Zone GetZoneForPosition(Vector3 position)
    {
        if (safeZone != null && safeZone.bounds.Contains(position)) return Zone.Safe;
        if (pinkZone != null && pinkZone.bounds.Contains(position)) return Zone.Pink;
        if (blueZone != null && blueZone.bounds.Contains(position)) return Zone.Blue;
        if (greenZone != null && greenZone.bounds.Contains(position)) return Zone.Green;
        return Zone.None;
    }
}