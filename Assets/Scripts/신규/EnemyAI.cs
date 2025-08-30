using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType { Killable, Unkillable }

    [Header("공통")]
    public EnemyType enemyType = EnemyType.Killable;
    public float detectionRange = 12f;
    public float attackRange = 1.6f;
    public float moveSpeed = 1.2f;       // NavMesh 미사용 시 속도
    public float attackCooldown = 1.2f;  // 초
    public int attackSlots = 1;          // 플레이어 HP 슬롯 데미지

    [Header("체력/드랍 (Killable 전용)")]
    public int maxHp = 15;
    public ItemData dropItem;            // 사망 시 드랍(선택)

    private Transform target;
    private float lastAttackTime = -999f;
    private int currentHp;
    private NavMeshAgent agent;

    void Awake()
    {
        currentHp = maxHp;
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.stoppingDistance = attackRange * 0.9f;
    }

    void Update()
    {
        AcquireTarget();
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > attackRange)
        {
            MoveTowards(target.position);
        }
        else
        {
            TryAttack(target);
        }
    }

    void AcquireTarget()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();
        Transform best = null; float bestDist = float.MaxValue;

        foreach (var p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d <= detectionRange && d < bestDist)
            {
                best = p.transform; bestDist = d;
            }
        }
        target = best;
    }

    void MoveTowards(Vector3 dest)
    {
        if (agent != null && agent.enabled)
        {
            agent.speed = moveSpeed;
            agent.SetDestination(dest);
        }
        else
        {
            Vector3 dir = (dest - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    void TryAttack(Transform t)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        var hp = t.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamageSlots(Mathf.Max(1, attackSlots));
            lastAttackTime = Time.time;
        }
    }

    // 원거리 무기 등으로부터의 피해 처리
    public void TakeDamage(int dmg)
    {
        if (enemyType == EnemyType.Unkillable) return;

        currentHp -= Mathf.Max(1, dmg);
        if (currentHp <= 0) Die();
    }

    void Die()
    {
        if (dropItem != null && dropItem.worldPrefab != null)
        {
            Instantiate(dropItem.worldPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
