using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("����")]
    public int maxHpSlots = 5;
    public Image[] hpSegments;
    public Sprite filledSprite;
    public Sprite emptySprite;

    public Vector3 respown_pos;
    public GameObject respwn_text;
    public Animator pade_anim;
    public CameraRotation rotation;

    private Animator anim;//현중

    public Camera playerCamera;   // ���� �����ϴ� ������� ����

    private int currentHpSlots;
    private bool isDead = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        anim = GetComponent<Animator>();//현중

        currentHpSlots = maxHpSlots;
        UpdateHpUI();
    }

    private void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                pade_anim.gameObject.SetActive(true);
                Invoke("RestoreFullHp", 1f);//인보크로 변경
            }
        }
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

    /// <summary>
    /// ������ �ڿ� ȸ��/���� API. slots ��ŭ HP ������ ȸ���մϴ�(�ּ� 1).
    /// </summary>
    public void HealSlots(int slots)
    {
        if (isDead) return;
        currentHpSlots = Mathf.Clamp(currentHpSlots + Mathf.Max(1, slots), 0, maxHpSlots);
        UpdateHpUI();
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
        Debug.Log("사망");
        anim.SetTrigger("isDead");

        respwn_text.gameObject.SetActive(true);//리스폰 안내 텍스트 활어화

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        if (playerCamera != null)
        {
            var mouseLook = playerCamera.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;
            rotation.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestoreFullHp()
    {
        gameObject.transform.position = respown_pos;
        anim.SetTrigger("Respwn");
        pade_anim.SetTrigger("pade_in");//연출 추가
        currentHpSlots = maxHpSlots;
        UpdateHpUI();
        isDead = false;
        respwn_text.gameObject.SetActive(false);    

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        if (playerCamera != null)
        {
            var mouseLook = playerCamera.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;
            rotation.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dead_Line"))
            Die();
    }
}
