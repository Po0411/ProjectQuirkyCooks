using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 UI에서 각 플레이어 슬롯의 정보를 표시하고 활성화/비활성화하는 스크립트입니다.
/// 아바타 이미지, 플레이어 이름, 준비 상태 텍스트를 관리합니다.
/// </summary>
public class PlayerSlot : MonoBehaviour
{
    public Image avatarImage;    // 플레이어 아이콘
    public Text nameText;        // 플레이어 이름 라벨
    public Text readyText;       // 준비 상태 라벨

    /// <summary>
    /// 슬롯을 비활성화합니다. 플레이어가 나갔을 때 사용합니다.
    /// </summary>
    public void ClearSlot()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 슬롯에 플레이어 정보를 설정하고 활성화합니다.
    /// </summary>
    /// <param name="displayName">표시할 플레이어 이름</param>
    /// <param name="isReady">준비 상태</param>
    /// <param name="avatar">아바타 스프라이트</param>
    public void SetPlayer(string displayName, bool isReady, Sprite avatar)
    {
        gameObject.SetActive(true);
        if (nameText != null) nameText.text = displayName;
        if (avatarImage != null) avatarImage.sprite = avatar;
        if (readyText != null)
        {
            readyText.text = isReady ? "준비" : "미준비";
            readyText.color = isReady ? Color.green : Color.red;
        }
    }
}
