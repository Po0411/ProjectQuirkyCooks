using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    private Animator anim;
    private bool isDead;
    private MonsterAI monsterAi;

    [Header("HP Settings")]
    public float maxHp = 5000f;
    private float currentHp;

    [Header("UI")]
    [Tooltip("������ �ｺ�� ��ũ��Ʈ")]
    public BillboardHealthBar healthBar;

    [Header("Death Drop")]
    [Tooltip("������ ������ ���� ������ (Rigidbody �ʿ�)")]
    public GameObject meatPrefab;
    [Tooltip("���Ⱑ Ƣ�� ���� �ʱ� ���� ����")]
    public float spawnForce = 5f;
    [Tooltip("���⸦ ������ ���� ������")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);

    void Start()
    {
        anim = GetComponent<Animator>();
        monsterAi = GetComponent<MonsterAI>();

        currentHp = maxHp;
        if (healthBar != null)
            healthBar.SetHealthFraction(1f);
    }

    /// <summary>
    /// �ܺο��� ���ظ� ���� �� �� �޼��带 ȣ���ϼ���.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0f, currentHp - amount);

        if(GunInOut.gunInOut == true)//현중 If GunInOut 추가함
        {
            // �ｺ�� ������Ʈ
            if (healthBar != null)
                healthBar.SetHealthFraction(currentHp / maxHp);

            // HP�� 0�̸� ��� ó��
            if (currentHp == 0f)
                Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        monsterAi.NotifyDeath();

        SpawnMeat();

        //MonsterDead.isDead = true;

        // (���Ѵٸ� ��� ����Ʈ, ���� �� �߰�)
        Destroy(gameObject, 1.5f);
    }

    void SpawnMeat()
    {
        if (meatPrefab == null)
        {
            Debug.LogWarning("MonsterHealth: meatPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
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