// Assets/Scripts/ItemSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [Header("������ �����յ� (��������� ���� �� ��)")]
    public List<GameObject> itemPrefabs = new List<GameObject>();

    [Header("���� �ڽ� (������ǥ ������)")]
    public Vector3 center = Vector3.zero;              // �� ������Ʈ�� Transform ���� ������
    public Vector3 size = new Vector3(10, 1, 10);    // ����(Y�� �β�), ����(Z)

    [Header("���� ����")]
    [Min(0)] public int spawnCount = 10;

    public float respwn_cool_time;//������ ��Ÿ��
    float cool_time;
    public float dspwn_cool_time;
    float d_cool_time;
    public List<GameObject> dspwn_objs=new List<GameObject>();

    // �ߺ� ���� ����
    private bool _spawned = false;

    // --- ����: NGO ���� ����
    private static bool NetActive =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // --- ����/�̱� ��ο��� �� ���� ����
    public override void OnNetworkSpawn()
    {
        // ��Ƽ(=NGO Ȱ��)�� ���� ������ ����
        if (IsServer && !_spawned)
        {
            SpawnItems(networked: true);
            _spawned = true;
        }
    }

    private void Start()
    {
        // �̱�(=NGO ��Ȱ��)�� �� Start���� ���� ����
        if (!NetActive && !_spawned)
        {
            SpawnItems(networked: false);
            _spawned = true;
        }
        cool_time = respwn_cool_time;
        d_cool_time = dspwn_cool_time;
    }

    private void Update()
    {
        if(respwn_cool_time > 0) cool_time -= Time.deltaTime;
        if(dspwn_cool_time > 0)d_cool_time -= Time.deltaTime;

        if(cool_time <= 0&& respwn_cool_time > 0)
        {
            SpawnItems(networked: false);
            cool_time = respwn_cool_time;
        }
        if(d_cool_time <= 0 && dspwn_cool_time > 0)
        {
            for (int i = 0; i < dspwn_objs.Count; i++)
            {
                Destroy(dspwn_objs[i]);
            }
            dspwn_objs.Clear();
            d_cool_time = dspwn_cool_time;
        }
        
    }

    private void SpawnItems(bool networked)
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0 || spawnCount <= 0)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionInBox();
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, spawnPos, rot);

            if (networked)
            {
                // ��Ƽ: NetworkObject�� ������ ��Ʈ��ũ ����
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }
                else
                {
                    Debug.LogWarning(
                        $"[ItemSpawner] '{prefab.name}'�� NetworkObject�� ���� " +
                        "Ŭ���̾�Ʈ�� ����ȭ���� �ʽ��ϴ�. (�̱ۿ� �������̶�� ����)");
                }
            }
            dspwn_objs.Add(obj);
            // �̱��� Instantiate�� �ϸ� ��
        }
    }

    private Vector3 GetRandomPositionInBox()
    {
        // �ڽ��� �� ������Ʈ�� Transform ����(���� ������)
        Vector3 localRandom = new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f),
            Random.Range(-size.z * 0.5f, size.z * 0.5f)
        );
        return transform.TransformPoint(center + localRandom);
    }

    // �����Ϳ��� ���� ���̱�
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(center, size);
        Gizmos.matrix = prev;
    }
}
