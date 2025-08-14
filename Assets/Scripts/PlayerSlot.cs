using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSlot : MonoBehaviour
{
    public Image avatarImage;   // (선택) 아이콘
    public TMP_Text nameText;     // Player1 등 (TMP)
    public TMP_Text readyText;    // 준비/미준비 (TMP)

    public void ClearSlot()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayer(string displayName, bool isReady, Sprite avatarSprite = null)
    {
        gameObject.SetActive(true);

        if (nameText != null)
            nameText.text = displayName;

        if (readyText != null)
        {
            readyText.text = isReady ? "준비" : "미준비";
            readyText.color = isReady ? Color.green : Color.red;
        }

        if (avatarImage != null && avatarSprite != null)
            avatarImage.sprite = avatarSprite;
    }
}
