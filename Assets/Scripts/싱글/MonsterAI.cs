using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MonsterType { DaeduMabel, Tomatok, Krang }

[RequireComponent(typeof(CharacterController))]
public class MonsterAI : MonoBehaviour
{
    private bool isDead = false;

    [Header("Type & References")]
    public MonsterType type;
    public Transform player;
    public string safeZoneTag = "SafeZone";

    [Header("Detection Ranges (m)")]
    public float suspiciousRange = 10f;
    public float approachRange = 8f;
    public float chaseRange = 5f;
    public float attackRange = 2f;
    public float closeAttackRange = 1f;

    [Header("Movement & Attack")]
    public float moveSpeed = 1.2f;   // DaeduMabel/Tomatok �⺻��
    public float krangMoveSpeed = 1.5f;   // Krang ����
    public float attackInterval = 0.2f;   // DaeduMabel/Tomatok �⺻��
    public float krangAttackInterval = 0.3f;   // Krang ����
    public float stunDuration = 2f;     // Krang ���� �ð�

    private CharacterController cc;
    private Vector3 spawnPos;
    private float lastAttackTime;
    private bool inSafeZone;
    private bool isStunned;

    enum State { Idle, Suspicious, Approach, Chase, Attack, Stunned, Returning }
    private State state = State.Idle;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        spawnPos = transform.position;

        // �� �߰�: �ٴ� ����
        SnapToGround();

        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void SnapToGround()
    {
        RaycastHit hit;
        // ���̸� ���� �ٷ� �Ʒ��� ���, ���̾� ����ũ�� �ʿ��ϴٸ� �߰��ϼ���.
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y;
            transform.position = p;
        }
    }

    void Update()
    {
        if (isDead) return;
        float dist = Vector3.Distance(transform.position, player.position);

        // �߰� ���� ����: ���� ���� ���� OR �־���(suspiciousRange �ʰ�)
        if (inSafeZone || dist > suspiciousRange)
        {
            state = State.Returning;
            MoveTowards(spawnPos);
            if (Vector3.Distance(transform.position, spawnPos) < 0.1f)
                state = State.Idle;
            return;
        }

        // ���� ��ȯ (�Ÿ� ����)
        if (dist > approachRange) state = State.Suspicious;
        else if (dist > chaseRange) state = State.Approach;
        else if (dist > attackRange) state = State.Chase;
        else state = State.Attack;

        // �ൿ
        switch (state)
        {
            case State.Suspicious:
                // TODO: �ǽ� �ִϸ��̼�
                break;
            case State.Approach:
            case State.Chase:
                MoveTowards(player.position);
                break;
            case State.Attack:
                TryAttack();
                break;
        }
    }

    // void MoveTowards(Vector3 targetPos)
    // {
    //     Vector3 dir = (targetPos - transform.position).normalized;
    //     float speed = (type == MonsterType.Krang) ? krangMoveSpeed : moveSpeed;
    //     cc.SimpleMove(dir * speed);
    //     if (dir != Vector3.zero)
    //         transform.rotation = Quaternion.LookRotation(dir);
    // }

    void MoveTowards(Vector3 targetPos)
{
    // 1. 방향 벡터 계산 (Y값 포함)
    Vector3 dir = (targetPos - transform.position).normalized;
    float speed = (type == MonsterType.Krang) ? krangMoveSpeed : moveSpeed;

    // 2. 이동: SimpleMove는 어차피 dir의 Y값을 무시하고 자체 중력을 적용합니다.
    cc.SimpleMove(dir * speed);

    // 3. 회전용 벡터 생성 (Y값을 0으로 만듦)
    Vector3 lookDir = new Vector3(dir.x, 0, dir.z);

    // 4. 수평 방향이 있을 때만 회전 (Y축으로 정확히 겹치지 않을 때)
    if (lookDir != Vector3.zero)
    {
        // Y값이 0인 lookDir를 바라보게 하여 수평 회전만 하도록 함
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}

    void TryAttack()
    {
        float interval = (type == MonsterType.Krang) ? krangAttackInterval : attackInterval;
        if (Time.time - lastAttackTime < interval) return;
        lastAttackTime = Time.time;

        // ���� �õ� �α�
        Debug.Log($"[MonsterAI] TryAttack() ȣ��. ����: {state}, �Ÿ�: {Vector3.Distance(transform.position, player.position)}");

        // PlayerHealth ������Ʈ ã�ƺ���
        PlayerHealth ph = player.GetComponent<PlayerHealth>()
                         ?? player.GetComponentInChildren<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError("[MonsterAI] PlayerHealth ������Ʈ�� ã�� ���߽��ϴ�! player=" + player.name);
            return;
        }

        // ���� ����� ���� �α�
        Debug.Log("[MonsterAI] �÷��̾�� 1ĭ ����� ����!");
        if (type == MonsterType.Krang)
        {
            StartCoroutine(StunPlayer());
        }
        else
        {
            ph.TakeDamageSlots(1);
        }
    }

    IEnumerator StunPlayer()
    {
        isStunned = true;
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        yield return new WaitForSeconds(stunDuration);

        if (pc != null) pc.enabled = true;
        isStunned = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(safeZoneTag) && other.gameObject.tag == safeZoneTag)
            inSafeZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(safeZoneTag) && other.gameObject.tag == safeZoneTag)
            inSafeZone = false;
    }

    public void NotifyDeath()
    {
        isDead = true;
        if (cc != null)
        {
            cc.enabled = false;
        }
    }
}
