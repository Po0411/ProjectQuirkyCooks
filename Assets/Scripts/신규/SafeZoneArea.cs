using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZoneRegen : MonoBehaviour
{
    [Tooltip("이 간격(초)마다 영역 내 플레이어의 HP 슬롯을 1칸 회복합니다.")]
    public float healIntervalSeconds = 3f;

    // 플레이어별 경과 시간
    private readonly Dictionary<PlayerHealth, float> timers = new Dictionary<PlayerHealth, float>();

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        // 원하면 태그 지정 가능: gameObject.tag = "SafeZone";
    }

    void OnTriggerEnter(Collider other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;
        timers[hp] = 0f;
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
            hp.HealSlots(1);  // 슬롯 1칸 회복
            t -= healIntervalSeconds; // 간격 주기 유지
        }

        timers[hp] = t;
    }

    void OnTriggerExit(Collider other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) timers.Remove(hp);
    }
}
