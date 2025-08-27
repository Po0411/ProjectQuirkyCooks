using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("1회 클릭당 입힐 피해량")]
    public float damage = 500f;
    [Tooltip("최대 사거리")]
    public float range = 10f;
    [Tooltip("몬스터 레이어만 체크")]
    public LayerMask monsterLayer;

    [Header("References")]
    [Tooltip("플레이어 시점 카메라")]
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryHit();
    }

    void TryHit()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range, monsterLayer))
        {
            // 몬스터 헬스 컴포넌트 탐색
            MonsterHealth mh = hit.collider.GetComponent<MonsterHealth>()
                             ?? hit.collider.GetComponentInParent<MonsterHealth>();
            if (mh != null)
            {
                mh.TakeDamage(damage);
            }
        }
    }
}
