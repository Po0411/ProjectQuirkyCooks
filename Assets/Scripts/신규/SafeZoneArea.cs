using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZoneRegen : MonoBehaviour
{
    [Tooltip("�� ����(��)���� ���� �� �÷��̾��� HP ������ 1ĭ ȸ���մϴ�.")]
    public float healIntervalSeconds = 3f;

    public GameObject recipe;

    // �÷��̾ ��� �ð�
    private readonly Dictionary<PlayerHealth, float> timers = new Dictionary<PlayerHealth, float>();

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        // ���ϸ� �±� ���� ����: gameObject.tag = "SafeZone";
    }

    void OnTriggerEnter(Collider other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;
        timers[hp] = 0f;

        recipe.SetActive(true);
    }

    void OnTriggerStay(Collider other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;

        float t = 0f;
        timers.TryGetValue(hp, out t);
        t += Time.deltaTime;

        if (t >= healIntervalSeconds)
        {
            hp.HealSlots(1);  // ���� 1ĭ ȸ��
            t -= healIntervalSeconds; // ���� �ֱ� ����
        }

        timers[hp] = t;
    }

    void OnTriggerExit(Collider other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) timers.Remove(hp);

        recipe.SetActive(false);
    }
}
