using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("설정")]
    public int maxHpSlots = 5;
    public Image[] hpSegments;
    public Sprite filledSprite;
    public Sprite emptySprite;

    public Camera playerCamera;   // 직접 연결하는 방식으로 변경

    private int currentHpSlots;
    private bool isDead = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        currentHpSlots = maxHpSlots;
        UpdateHpUI();
    }

    public void TakeDamageSlots(int slots)
    {
        if (isDead) return;

        currentHpSlots -= slots;
        currentHpSlots = Mathf.Clamp(currentHpSlots, 0, maxHpSlots);
        UpdateHpUI();

        if (currentHpSlots <= 0)
            Die();
    }

    private void UpdateHpUI()
    {
        for (int i = 0; i < hpSegments.Length; i++)
        {
            if (hpSegments[i] == null) continue;
            if (i < currentHpSlots)
            {
                hpSegments[i].sprite = filledSprite;
                hpSegments[i].enabled = true;
            }
            else
            {
                hpSegments[i].sprite = emptySprite;
                hpSegments[i].enabled = true;
            }
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망");

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        if (playerCamera != null)
        {
            var mouseLook = playerCamera.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestoreFullHp()
    {
        currentHpSlots = maxHpSlots;
        UpdateHpUI();
        isDead = false;

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        if (playerCamera != null)
        {
            var mouseLook = playerCamera.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
