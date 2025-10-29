using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    private float mouseSpeed;

    [Tooltip("최소 회전 각도 (아래를 볼 때)")]
    public float minimumX = -90.0f;
    [Tooltip("최대 회전 각도 (위를 볼 때)")]
    public float maximumX = 90.0f;
    
    private float rotationX = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        mouseSpeed = MouseLook.mouseSpeed;
        
        float mouseY = Input.GetAxis("Mouse Y");
        rotationX -= mouseY * mouseSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
}
