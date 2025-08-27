// Assets/Scripts/MouseLook.cs
using UnityEngine;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    [Header("마우스 감도")]
    public float mouseSensitivity = 100f;

    [Header("회전 대상(플레이어 루트)")]
    public Transform playerBody;

    [Header("상/하 한계 (도)")]
    public float mouseY_U = 90f;   // 위로 볼 수 있는 각도
    public float mouseY_D = 90f;   // 아래로 볼 수 있는 각도

    [Header("커서 잠금")]
    public bool lockCursorOnStart = true;

    private float xRotation = 0f;

    // ───────────────────────── helpers (싱글/멀티 공통 권한 판단)
    bool NetActive =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // 네트워크 OFF(=싱글)면 무조건 조작 허용, 네트워크 ON이면 IsOwner만 허용
    bool HasLocalAuthority => !NetActive || IsOwner;

    void Start()
    {
        if (!HasLocalAuthority)
        {
            // 멀티에서 내 소유가 아니면 동작 비활성
            enabled = false;
            return;
        }

        if (lockCursorOnStart) ApplyCursorLock(true);

        // 카메라 피치 각도 초기화(씬에서 회전된 상태면 보정)
        Vector3 e = transform.localEulerAngles;
        // Unity는 0~360 도수, -180~180 범위로 변환
        xRotation = e.x > 180f ? e.x - 360f : e.x;
    }

    void OnDisable()
    {
        // 컴포넌트 꺼질 때 커서 잠금 풀기(원하면 주석)
        ApplyCursorLock(false);
    }

    void Update()
    {
        if (!HasLocalAuthority) return;

        // ESC로 잠금 토글, 마우스 좌클릭으로 재잠금 (원하면 삭제)
        if (Input.GetKeyDown(KeyCode.Escape)) ApplyCursorLock(false);
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            ApplyCursorLock(true);

        // UI 열려 잠금 풀린 상태면 입력 무시
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라(피치)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -mouseY_U, mouseY_D);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 플레이어 바디(요)
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }

    void ApplyCursorLock(bool on)
    {
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }
}
