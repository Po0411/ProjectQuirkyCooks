using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Tooltip("스폰 포인트들")]
    public Transform[] spawnPoints;

    [Tooltip("스폰할 프리팹들(몬스터/NPC)")]
    public GameObject[] prefabs;

    public float interval = 180f;
    public int maxAlive = 10;

    private readonly List<GameObject> alive = new List<GameObject>();
    private float timer;

    void Update()
    {
        // null 청소
        for (int i = alive.Count - 1; i >= 0; i--)
            if (alive[i] == null) alive.RemoveAt(i);

        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;

        if (alive.Count >= maxAlive) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (prefabs == null || prefabs.Length == 0) return;

        var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var pf = prefabs[Random.Range(0, prefabs.Length)];

        var go = Instantiate(pf, sp.position, sp.rotation);
        alive.Add(go);
    }
}
