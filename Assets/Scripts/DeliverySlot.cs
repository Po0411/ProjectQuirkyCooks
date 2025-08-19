using UnityEngine;
using UnityEngine.UI;

public class DeliverySlot : MonoBehaviour
{
    [Tooltip("아이콘을 그릴 Image. 비우면 자식 'Icon' 또는 자기 자신의 Image를 자동 사용")]
    public Image targetImage;
    [Tooltip("스프라이트 없으면 감추기")]
    public bool hideWhenEmpty = true;

    void Reset() => AutoFindImage();
    void Awake() { if (targetImage == null) AutoFindImage(); }

    void AutoFindImage()
    {
        if (transform.Find("Icon") && transform.Find("Icon").TryGetComponent(out Image img)) { targetImage = img; return; }
        if (!TryGetComponent(out targetImage)) targetImage = GetComponentInChildren<Image>(true);
    }

    public void SetSprite(Sprite s)
    {
        if (!targetImage) return;
        targetImage.sprite = s;
        targetImage.enabled = s != null || !hideWhenEmpty;
    }

    public void Clear() => SetSprite(null);
}
