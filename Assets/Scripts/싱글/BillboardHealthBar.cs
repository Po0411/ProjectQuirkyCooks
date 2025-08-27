using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class BillboardHealthBar : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    [Header("UI Components")]
    public Image fillImage;

    [Header("Camera (optional)")]
    [Tooltip("MainCamera 태그를 사용하지 않을 때 수동 지정")]
    public Camera overrideCamera;

    private Transform cam;

    void Start()
    {
        // 1) 수동 지정된 카메라가 있으면 사용, 아니면 Camera.main 사용
        cam = (overrideCamera != null ? overrideCamera.transform : Camera.main?.transform);

        if (cam == null) Debug.LogError("BillboardHealthBar: 사용할 카메라가 없습니다!");
        if (target == null) Debug.LogError("BillboardHealthBar: target 미할당!");
        if (fillImage == null) Debug.LogError("BillboardHealthBar: fillImage 미할당!");
    }

    void LateUpdate()
    {
        if (cam == null || target == null || fillImage == null)
            return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
    }

    public void SetHealthFraction(float fraction)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(fraction);
    }
}