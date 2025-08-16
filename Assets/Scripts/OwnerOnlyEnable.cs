using Unity.Netcode;
using UnityEngine;

public class OwnerOnlyEnable : NetworkBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] MonoBehaviour[] controls;
    void Awake() { if (!cam) cam = GetComponentInChildren<Camera>(true); }
    public override void OnNetworkSpawn()
    {
        bool on = IsOwner;
        if (cam) cam.gameObject.SetActive(on);
        if (controls != null) foreach (var c in controls) if (c) c.enabled = on;
    }
}
