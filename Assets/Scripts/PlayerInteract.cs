using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactRange = 5f;
    public LayerMask interactLayer;
    public TextMeshProUGUI interactTextUI;

    private IInteractable currentInteractable;

    void Update()
    {
        // Ray 생성
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();

            if (currentInteractable != null)
            {
                // UI 표시
                interactTextUI.gameObject.SetActive(true);
                interactTextUI.text = currentInteractable.GetInteractText();

                // 좌클릭 시 상호작용
                if (Input.GetMouseButtonDown(0))
                {
                    InventoryManager inv = FindObjectOfType<InventoryManager>();
                    currentInteractable.Interact(inv);
                }
            }
            else
            {
                HideText();
            }
        }
        else
        {
            HideText();
        }
    }

    void HideText()
    {
        if (interactTextUI != null)
            interactTextUI.gameObject.SetActive(false);
        currentInteractable = null;
    }
}
