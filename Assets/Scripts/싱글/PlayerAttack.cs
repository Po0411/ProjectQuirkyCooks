using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("1ȸ Ŭ���� ���� ���ط�")]
    public float damage = 500f;
    [Tooltip("�ִ� ��Ÿ�")]
    public float range = 10f;
    [Tooltip("���� ���̾ üũ")]
    public LayerMask monsterLayer;

    [Header("References")]
    [Tooltip("�÷��̾� ���� ī�޶�")]
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryHit();
    }

    void TryHit()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (GunInOut.gunInOut == true) //<-이거 추가함
        {
            if (Physics.Raycast(ray, out RaycastHit hit, range, monsterLayer))
            {
                // ���� �ｺ ������Ʈ Ž��
                MonsterHealth mh = hit.collider.GetComponent<MonsterHealth>()
                 ?? hit.collider.GetComponentInParent<MonsterHealth>();
                if (mh != null)
                {
                    mh.TakeDamage(damage);
                }
            }
        }
    }
}
