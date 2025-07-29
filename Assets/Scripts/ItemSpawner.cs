using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [Header("아이템 프리팹들")]
    public List<GameObject> itemPrefabs;

    [Header("스폰 박스 범위 설정")]
    public Vector3 center = Vector3.zero;       // 중앙 좌표
    public Vector3 size = new Vector3(10, 1, 10); // 박스 크기 (가로, 세로, 깊이)

    [Header("스폰 개수")]
    public int spawnCount = 10;

    public override void OnNetworkSpawn()
    {
        if (IsServer)  // Host 또는 Dedicated Server만 실행
        {
            SpawnItems();
        }
    }

    private void SpawnItems()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionInBox();

            // 랜덤 프리팹 선택
            GameObject randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];

            // 서버에서 네트워크 오브젝트 생성
            GameObject itemInstance = Instantiate(randomPrefab, spawnPos, Quaternion.identity);
            var netObj = itemInstance.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn(); // NGO 관리 시작
            }
            else
            {
                Debug.LogWarning($"{randomPrefab.name} 에 NetworkObject 컴포넌트가 없습니다!");
            }
        }
    }

    private Vector3 GetRandomPositionInBox()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-size.x / 2, size.x / 2),
            Random.Range(-size.y / 2, size.y / 2),
            Random.Range(-size.z / 2, size.z / 2)
        );

        return center + randomPos;
    }

    // 박스 영역 씬에서 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}
