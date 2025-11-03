using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CraftingZone : MonoBehaviour
{
    void Reset()
    {
        // Collider�� Trigger�� �����Ǿ� �ִ��� üũ
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    // �� �信�� ������ ���̵��� Gizmo �׸��� (����)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        var box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}