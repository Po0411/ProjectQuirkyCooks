using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;               // 상호작용 거리
    public LayerMask interactLayer;                // 상호작용 가능한 오브젝트 레이어
    public TextMeshProUGUI interactText;           // 화면 중앙 텍스트 (예: "● 썰기")

    void Update()
    {
        if (interactText != null)
            interactText.text = ""; // 기본적으로 숨김

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // 인터랙트 텍스트 표시
                if (interactText != null)
                    interactText.text = "● " + interactable.GetInteractText();

                // 클릭 시 상호작용
                if (Input.GetMouseButtonDown(0))
                {
                    InventoryManager inv = FindObjectOfType<InventoryManager>();
                    if (inv != null)
                    {
                        // InventoryManager 있는 버전 사용
                        interactable.Interact(inv);
                    }
                    else
                    {
                        // 없으면 파라미터 없는 버전 호출
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
