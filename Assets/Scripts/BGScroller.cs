using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIBGScroller : MonoBehaviour
{
    [Tooltip("스크롤 속도 (UV 단위/초)")]
    public float speed = 0.5f;

    private float offset = 0f;
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        // UV 초기값 가져오기
        offset = rawImage.uvRect.x;
    }

    void Update()
    {
        // 오프셋 누적
        offset += Time.deltaTime * speed;
        // 0~1 범위로 래핑(optional)
        offset %= 1f;

        // 기존 UV 크기랑 Y 위치는 유지
        Rect uv = rawImage.uvRect;
        uv.x = offset;
        rawImage.uvRect = uv;
    }
}
