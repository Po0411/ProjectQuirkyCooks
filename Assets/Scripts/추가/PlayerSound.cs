using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("사운드 클립")]
    public AudioClip attackSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        HandleInputAndPlaySound();
    }

    void HandleInputAndPlaySound()
    {
        if (Input.GetMouseButtonDown(0) && GunInOut.gunInOut == true)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
