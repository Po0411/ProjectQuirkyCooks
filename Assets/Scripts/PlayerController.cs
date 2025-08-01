using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("이동 설정")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("스태미나")]
    public float stamina = 10f;
    public float maxStamina = 10f;
    public float staminaDrain = 2f;
    public float staminaRegen = 1f;

    [Header("UI Prefabs")]
    public GameObject playerInformationCanvasPrefab;

    private Slider staminaGauge;
    private Image staminaFill;

    [Header("카메라")]
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Color fullColor = new Color(0.2f, 0.7f, 1f); // 파랑
    private Color emptyColor = new Color(1f, 0.2f, 0.2f); // 빨강

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // ✅ PlayerInformationCanvas 생성
        if (playerInformationCanvasPrefab != null)
        {
            GameObject infoCanvas = Instantiate(playerInformationCanvasPrefab);
            infoCanvas.transform.SetParent(GameObject.Find("Canvas").transform, false);

            staminaGauge = infoCanvas.GetComponentInChildren<Slider>(true);
            if (staminaGauge != null)
                staminaFill = staminaGauge.fillRect.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("❌ PlayerInformationCanvas Prefab이 Inspector에 연결되지 않았습니다.");
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // 이동 처리
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && move.magnitude > 0)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
            if (stamina < 0) stamina = 0;
        }
        else
        {
            stamina += staminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ✅ 스태미나 UI 갱신
        if (staminaGauge != null)
        {
            float ratio = stamina / maxStamina;
            staminaGauge.value = ratio;

            if (staminaFill != null)
                staminaFill.color = Color.Lerp(emptyColor, fullColor, ratio);
        }
    }
}
