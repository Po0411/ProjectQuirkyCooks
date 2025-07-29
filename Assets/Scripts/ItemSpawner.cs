using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹들")]
    public List<GameObject> itemPrefabs;

    [Header("스폰 박스 범위 설정")]
    public Vector3 center = Vector3.zero;       // 중앙 좌표
    public Vector3 size = new Vector3(10, 1, 10); // 박스 크기 (가로, 세로, 깊이)

    [Header("스폰 개수")]
    public int spawnCount = 10;

    void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionInBox();

            // 랜덤 프리팹 선택
            GameObject randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];

            // 소환
            Instantiate(randomPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPositionInBox()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-size.x / 2, size.x / 2),
            Random.Range(-size.y / 2, size.y / 2),
            Random.Range(-size.z / 2, size.z / 2)
        );

        return center + randomPos;
    }

    // 박스 영역 씬에서 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}
