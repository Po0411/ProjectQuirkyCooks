using UnityEngine;

public class AirGun : MonoBehaviour
{
    public int damage = 3;
    public float range = 5f;
    public float cooldown = 1f;

    public Camera fireCamera; // 쏘는 기준 카메라
    private float lastShot;

    void Awake()
    {
        if (fireCamera == null) fireCamera = GetComponentInParent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastShot >= cooldown)
        {
            lastShot = Time.time;

            var cam = fireCamera != null ? fireCamera : Camera.main;
            if (cam == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                var enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null) enemy.TakeDamage(Mathf.Max(1, damage));
            }
        }
    }
}
