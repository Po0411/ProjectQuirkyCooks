using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles gathering of special resources from specific spawn points.
/// Spawns resource prefabs in each zone based on the map configuration.
/// Includes a cooldown so that resources cannot be harvested too quickly.
/// </summary>
public class GatherSystem : NetworkBehaviour
{
    [Header("Gather Settings")]
    public float gatherCooldown = 1f; // 1 second cooldown between gathers
    public float spawnInterval = 180f; // resources respawn every 3 minutes
    public Transform[] pinkZoneSpawns;
    public Transform[] blueZoneSpawns;
    public Transform[] greenZoneSpawns;
    public GameObject potatoPrefab;
    public GameObject tomatoPrefab;
    public GameObject applePrefab;
    public GameObject wheatPrefab;
    public GameObject cabbagePrefab;
    public GameObject cornPrefab;

    private float lastGatherTime;

    // Spawns resources in each zone based on the plan
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        InvokeRepeating(nameof(RespawnResources), 0f, spawnInterval);
    }

    private void RespawnResources()
    {
        foreach (var sp in pinkZoneSpawns)
        {
            SpawnResource(potatoPrefab, sp.position);
            SpawnResource(tomatoPrefab, sp.position);
        }
        foreach (var sp in blueZoneSpawns)
        {
            SpawnResource(applePrefab, sp.position);
            SpawnResource(wheatPrefab, sp.position);
        }
        foreach (var sp in greenZoneSpawns)
        {
            SpawnResource(cabbagePrefab, sp.position);
            SpawnResource(cornPrefab, sp.position);
        }
    }

    private void SpawnResource(GameObject prefab, Vector3 position)
    {
        if (!IsServer || prefab == null) return;
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn(true);
    }

    public void GatherResource(Transform player, ItemData itemData)
    {
        if (Time.time - lastGatherTime < gatherCooldown) return;
        lastGatherTime = Time.time;
        // Add to player's inventory
        InventoryManager inv = player.GetComponent<InventoryManager>();
        if (inv != null)
        {
            inv.AddItemLocal(itemData);
        }
    }
}