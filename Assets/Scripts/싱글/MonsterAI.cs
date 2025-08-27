using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterType { DaeduMabel, Tomatok, Krang }

[RequireComponent(typeof(CharacterController))]
public class MonsterAI : MonoBehaviour
{
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
    public float moveSpeed = 1.2f;   // DaeduMabel/Tomatok 기본값
    public float krangMoveSpeed = 1.5f;   // Krang 전용
    public float attackInterval = 0.2f;   // DaeduMabel/Tomatok 기본값
    public float krangAttackInterval = 0.3f;   // Krang 전용
    public float stunDuration = 2f;     // Krang 스턴 시간

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

        // ▼ 추가: 바닥 착지
        SnapToGround();

        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void SnapToGround()
    {
        RaycastHit hit;
        // 레이를 몬스터 바로 아래로 쏘고, 레이어 마스크가 필요하다면 추가하세요.
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y;
            transform.position = p;
        }
    }

    void Update()
    {
        if (isStunned) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 추격 실패 조건: 안전 구역 진입 OR 멀어짐(suspiciousRange 초과)
        if (inSafeZone || dist > suspiciousRange)
        {
            state = State.Returning;
            MoveTowards(spawnPos);
            if (Vector3.Distance(transform.position, spawnPos) < 0.1f)
                state = State.Idle;
            return;
        }

        // 상태 전환 (거리 기준)
        if (dist > approachRange) state = State.Suspicious;
        else if (dist > chaseRange) state = State.Approach;
        else if (dist > attackRange) state = State.Chase;
        else state = State.Attack;

        // 행동
        switch (state)
        {
            case State.Suspicious:
                // TODO: 의심 애니메이션
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

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        float speed = (type == MonsterType.Krang) ? krangMoveSpeed : moveSpeed;
        cc.SimpleMove(dir * speed);
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void TryAttack()
    {
        float interval = (type == MonsterType.Krang) ? krangAttackInterval : attackInterval;
        if (Time.time - lastAttackTime < interval) return;
        lastAttackTime = Time.time;

        // 공격 시도 로그
        Debug.Log($"[MonsterAI] TryAttack() 호출. 상태: {state}, 거리: {Vector3.Distance(transform.position, player.position)}");

        // PlayerHealth 컴포넌트 찾아보기
        PlayerHealth ph = player.GetComponent<PlayerHealth>()
                         ?? player.GetComponentInChildren<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError("[MonsterAI] PlayerHealth 컴포넌트를 찾지 못했습니다! player=" + player.name);
            return;
        }

        // 실제 대미지 적용 로그
        Debug.Log("[MonsterAI] 플레이어에게 1칸 대미지 적용!");
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
}
