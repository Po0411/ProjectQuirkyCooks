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

    [Header("스태미나 설정")]
    public float stamina = 10f;
    public float maxStamina = 10f;
    public float staminaDrain = 2f;
    public float staminaRegen = 1f;

    [Header("UI Prefabs")]
    public GameObject playerInformationCanvasPrefab;

    private Image staminaBar; // Slider 대신 Image

    [Header("카메라")]
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

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

        // PlayerInformationCanvas 생성
        if (playerInformationCanvasPrefab != null)
        {
            GameObject infoCanvas = Instantiate(playerInformationCanvasPrefab);
            infoCanvas.transform.SetParent(GameObject.Find("Canvas").transform, false);

            var staminaObj = infoCanvas.transform.Find("PlayerInformationPanel/PlayerHp-Stamina/StaminaGauge");
            if (staminaObj != null)
            {
                staminaBar = staminaObj.GetComponent<Image>();
            }
            else
            {
                Debug.LogError("❌ StaminaGauge에서 Image를 찾을 수 없음");
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = walkSpeed;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0;
        bool hasStamina = stamina > 0.1f;

        if (wantsToRun && hasStamina)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
            if (stamina < 0) stamina = 0;
        }
        else
        {
            currentSpeed = walkSpeed;
            stamina += staminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ✅ Image.fillAmount로 UI 갱신
        if (staminaBar != null)
        {
            staminaBar.fillAmount = stamina / maxStamina;
        }
    }
}
