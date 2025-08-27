// Assets/Scripts/ItemSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [Header("아이템 프리팹들 (비어있으면 스폰 안 함)")]
    public List<GameObject> itemPrefabs = new List<GameObject>();

    [Header("스폰 박스 (로컬좌표 오프셋)")]
    public Vector3 center = Vector3.zero;              // 이 컴포넌트의 Transform 기준 오프셋
    public Vector3 size = new Vector3(10, 1, 10);    // 가로(Y는 두께), 세로(Z)

    [Header("스폰 개수")]
    [Min(0)] public int spawnCount = 10;

    // 중복 스폰 방지
    private bool _spawned = false;

    // --- 헬퍼: NGO 동작 여부
    private static bool NetActive =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // --- 서버/싱글 모두에서 한 번만 스폰
    public override void OnNetworkSpawn()
    {
        // 멀티(=NGO 활성)일 때만 서버가 스폰
        if (IsServer && !_spawned)
        {
            SpawnItems(networked: true);
            _spawned = true;
        }
    }

    private void Start()
    {
        // 싱글(=NGO 비활성)일 땐 Start에서 로컬 스폰
        if (!NetActive && !_spawned)
        {
            SpawnItems(networked: false);
            _spawned = true;
        }
    }

    private void SpawnItems(bool networked)
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0 || spawnCount <= 0)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionInBox();
            Quaternion rot = Quaternion.identity;

            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, spawnPos, rot);

            if (networked)
            {
                // 멀티: NetworkObject가 있으면 네트워크 스폰
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }
                else
                {
                    Debug.LogWarning(
                        $"[ItemSpawner] '{prefab.name}'에 NetworkObject가 없어 " +
                        "클라이언트와 동기화되지 않습니다. (싱글용 프리팹이라면 정상)");
                }
            }
            // 싱글은 Instantiate만 하면 끝
        }
    }

    private Vector3 GetRandomPositionInBox()
    {
        // 박스는 이 컴포넌트의 Transform 기준(로컬 오프셋)
        Vector3 localRandom = new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f),
            Random.Range(-size.z * 0.5f, size.z * 0.5f)
        );
        return transform.TransformPoint(center + localRandom);
    }

    // 에디터에서 범위 보이기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(center, size);
        Gizmos.matrix = prev;
    }
}
