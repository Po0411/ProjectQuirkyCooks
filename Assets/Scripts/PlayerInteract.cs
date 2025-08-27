using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Camera (비워두면 자동 탐색)")]
    [SerializeField] private Camera cam;

    [Header("상호작용 설정")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactLayer = ~0;

    [Header("UI (선택)")]
    [SerializeField] private TextMeshProUGUI interactTextUI;

    private IInteractable current;
    private InventoryManager inv; // 내 플레이어의 인벤 캐시

    void Awake()
    {
        // 내 인벤토리 캐시(멀티 대비: 내 오브젝트에 붙은 것 우선)
        inv = GetComponent<InventoryManager>();
        if (inv == null) inv = FindObjectOfType<InventoryManager>(true);

        // 카메라 자동 할당
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            var found = FindObjectOfType<Camera>(true);
            if (found != null)
            {
                cam = found;
                if (!found.CompareTag("MainCamera")) found.tag = "MainCamera";
            }
        }

        if (interactTextUI) interactTextUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!cam) { cam = Camera.main; if (!cam) return; }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // 트리거도 맞도록 Collide 지정
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer, QueryTriggerInteraction.Collide))
        {
            // ⬅️ 자식 콜라이더여도 상위에서 IInteractable 검색
            var interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                current = interactable;

                if (interactTextUI)
                {
                    interactTextUI.gameObject.SetActive(true);
                    interactTextUI.text = current.GetInteractText();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    current.Interact(inv); // 인벤 null이어도 상대 측에서 방어하도록 구현됨
                }
                return;
            }
        }

        HideText();
    }

    private void HideText()
    {
        if (interactTextUI) interactTextUI.gameObject.SetActive(false);
        current = null;
    }
}
