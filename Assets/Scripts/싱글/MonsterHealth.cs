using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public float maxHp = 5000f;
    private float currentHp;

    [Header("UI")]
    [Tooltip("빌보드 헬스바 스크립트")]
    public BillboardHealthBar healthBar;

    [Header("Death Drop")]
    [Tooltip("죽으면 생성할 고기 프리팹 (Rigidbody 필요)")]
    public GameObject meatPrefab;
    [Tooltip("고기가 튀어 오를 초기 힘의 세기")]
    public float spawnForce = 5f;
    [Tooltip("고기를 생성할 높이 오프셋")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);

    void Start()
    {
        currentHp = maxHp;
        if (healthBar != null)
            healthBar.SetHealthFraction(1f);
    }

    /// <summary>
    /// 외부에서 피해를 입힐 때 이 메서드를 호출하세요.
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentHp = Mathf.Max(0f, currentHp - amount);

        // 헬스바 업데이트
        if (healthBar != null)
            healthBar.SetHealthFraction(currentHp / maxHp);

        // HP가 0이면 사망 처리
        if (currentHp == 0f)
            Die();
    }

    void Die()
    {
        SpawnMeat();
        // (원한다면 사망 이펙트, 사운드 등 추가)
        Destroy(gameObject);
    }

    void SpawnMeat()
    {
        if (meatPrefab == null)
        {
            Debug.LogWarning("MonsterHealth: meatPrefab이 할당되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject meat = Instantiate(meatPrefab, spawnPos, Random.rotation);

        Rigidbody rb = meat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force = Vector3.up * spawnForce +
                            new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * spawnForce * 0.5f;
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}