using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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

    [Header("카메라")]
    public Camera playerCamera; // 프리팹의 자식 카메라 연결

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 내 캐릭터가 아니라면 조작 불가 + 카메라 꺼버림
        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            enabled = false;
            return;
        }

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!IsOwner) return; // 내 캐릭터만 조작

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            stamina += staminaRegen * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}