// Assets/Scripts/PlayerController.cs
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("이동 설정")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("스태미나 설정")]
    public float stamina = 10f;
    public float maxStamina = 10f;
    public float staminaDrain = 2f;   // 달릴 때 초당 감소
    public float staminaRegen = 1f;   // 걷거나 멈출 때 초당 회복

    [Header("UI (씬에 있는 Image를 직접 연결)")]
    public Image staminaGauge;            // ← Canvas/PlayerPanel/.../StaminaGauge 의 Image
    public bool autoFindGaugeByName = true; // 비워뒀으면 이름으로 찾아봄("StaminaGauge")

    [Header("카메라")]
    public Camera playerCamera;

    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;

    bool NetActive =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // 네트워크가 꺼져 있으면(싱글) 항상 조작 허용, 켜져있으면 오너만
    bool HasLocalAuthority => !NetActive || IsOwner;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        // 카메라 토글
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(HasLocalAuthority);

        // 스태미나 게이지 자동 검색(옵션)
        if (staminaGauge == null && autoFindGaugeByName)
        {
            var go = GameObject.Find("StaminaGauge");
            if (go != null) staminaGauge = go.GetComponent<Image>();
        }

        // 게이지 이미지 타입 강제(안전)
        if (staminaGauge != null)
        {
            staminaGauge.type = Image.Type.Filled;
            staminaGauge.fillMethod = Image.FillMethod.Horizontal;
            staminaGauge.fillOrigin = (int)Image.OriginHorizontal.Left;
            UpdateStaminaUI();
        }
    }

    void Update()
    {
        if (!HasLocalAuthority) return;

        // 지상 체크
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // 입력
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        // 달리기/스태미나
        float currentSpeed = walkSpeed;
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && move.sqrMagnitude > 0.001f;
        bool hasStamina = stamina > 0.1f;

        if (wantsToRun && hasStamina)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
            if (stamina < 0f) stamina = 0f;
        }
        else
        {
            currentSpeed = walkSpeed;
            stamina += staminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 중력
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (staminaGauge == null) return;
        staminaGauge.fillAmount = (maxStamina > 0f) ? Mathf.Clamp01(stamina / maxStamina) : 0f;
    }
}
