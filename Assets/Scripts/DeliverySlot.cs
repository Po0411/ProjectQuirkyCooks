using UnityEngine;
using UnityEngine.UI;

public class DeliverySlot : MonoBehaviour
{
    [Tooltip("아이콘을 그릴 Image. 비우면 자식 'Image'/'Icon' 또는 자기 자신의 Image를 자동 사용")]
    public Image targetImage;
    [Tooltip("스프라이트 없으면 감추기")]
    public bool hideWhenEmpty = true;

    // 현재 슬롯에 배정된 아이템 (왼쪽 주문의 실제 데이터)
    public ItemData AssignedItem { get; private set; }

    void Reset() => AutoFindImage();
    void Awake()
    {
        if (targetImage == null) AutoFindImage();
    }

    void AutoFindImage()
    {
        if (targetImage) return;

        var t = transform.Find("Image") ?? transform.Find("Icon");
        if (t && t.TryGetComponent(out Image img))
        {
            targetImage = img;
            return;
        }
        if (!TryGetComponent(out targetImage))
            targetImage = GetComponentInChildren<Image>(true);
    }

    public void SetItem(ItemData item)
    {
        AssignedItem = item;
        SetSprite(item ? item.icon : null);
    }

    public void SetSprite(Sprite s)
    {
        if (!targetImage) return;
        targetImage.sprite = s;
        targetImage.enabled = s != null || !hideWhenEmpty;
    }

    public void Clear() => SetItem(null);
}
