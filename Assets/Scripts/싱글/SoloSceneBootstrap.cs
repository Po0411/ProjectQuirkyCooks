// Assets/Scripts/싱글/SoloSceneBootstrap.cs
using UnityEngine;

public class SoloSceneBootstrap : MonoBehaviour
{
    [Header("Use Scene Player")]
    [Tooltip("씬에 미리 배치된 Player를 그대로 사용합니다.")]
    public bool useScenePlayer = true;

    [Header("Fallback Spawn (옵션)")]
    public bool spawnIfMissing = false;
    public GameObject playerPrefab;     // 필요하면 사용할 프리팹
    public Transform spawnPoint;        // 없으면 (0,0,0)

    [Header("Camera (자동 연결)")]
    public Camera mainCamera;           // 비우면 자동 탐색
    public bool keepCameraPose = true;  // ✅ 카메라 포즈/위치 유지

    void Awake()
    {
        Time.timeScale = 1f;

        if (spawnPoint == null)
        {
            var sp = GameObject.Find("SpawnPoint");
            if (sp) spawnPoint = sp.transform;
        }

        if (playerPrefab == null)
        {
            // 있으면 쓰고, 없어도 상관 없음(씬 플레이어만 쓰는 경우)
            playerPrefab = Resources.Load<GameObject>("Prefabs/Solo/Player");
        }
    }

    void Start()
    {
        // 1) 씬에 있는 플레이어 찾기
        PlayerController player = null;
        if (useScenePlayer)
            player = FindObjectOfType<PlayerController>(true);

        // 2) 없으면 옵션에 따라 스폰
        if (player == null && spawnIfMissing && playerPrefab != null)
        {
            Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
            Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
            player = Instantiate(playerPrefab, pos, rot).GetComponent<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogError("[Solo] PlayerController를 찾을 수 없습니다. (씬에 배치하거나 spawnIfMissing 켜기)");
            return;
        }

        // 3) 멀티 전용 컴포넌트 비활성 + 싱글 필수 스크립트 활성
        PrepareForSolo(player.gameObject);

        // 4) 카메라 설정 – 씬 값/프리팹 값 유지
        SetupCamera(player.transform);

        // 5) 마우스 커서
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- 준비 처리 ----------
    void PrepareForSolo(GameObject player)
    {
        // 네트워크 관련(있을 때만) OFF
        DisableByTypeName(player, "NetworkObject");
        DisableByTypeName(player, "NetworkBehaviour");
        DisableByTypeName(player, "NetworkTransform");
        DisableByTypeName(player, "ClientNetworkTransform");
        DisableByTypeName(player, "OwnerOnlyEnable");
        DisableByTypeName(player, "PlayerReady");

        // 필수 ON (있으면 켜기)
        EnableIfExists<CharacterController>(player, true);
        EnableIfExists<PlayerController>(player, true);
        EnableIfExists<PlayerInteract>(player, true);
        EnableIfExists<InventoryManager>(player, true);
    }

    // ---------- 카메라 ----------
    void SetupCamera(Transform playerRoot)
    {
        // 우선순위: 플레이어 자식 카메라 > 지정/메인 카메라
        Camera pc = playerRoot.GetComponentInChildren<Camera>(true);
        Camera cam = pc != null ? pc : (mainCamera != null ? mainCamera : Camera.main);

        if (cam == null)
        {
            // 씬에 카메라가 정말 없을 때만 새로 생성
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
        }

        // 다른 카메라 비활성
        foreach (var c in FindObjectsOfType<Camera>(true))
            c.gameObject.SetActive(c == cam);

        // SimpleCameraFollow 같은 보조 스크립트는 제거
        var follow = cam.GetComponent("SimpleCameraFollow");
        if (follow != null) Destroy(follow as Component);

        // 마우스 룩 보장 + 타깃 연결
        var ml = cam.GetComponent<MouseLook>();
        if (ml == null) ml = cam.gameObject.AddComponent<MouseLook>();
        ml.playerBody = playerRoot;

        // 포즈 유지가 기본 – 씬에서 배치해둔 카메라/프리팹 자식 카메라의 트랜스폼을 건드리지 않음
        if (!keepCameraPose)
        {
            // 필요 시 강제 오프셋을 적용하는 모드로 바꿀 수 있음 (원하면 여기에 원하는 세팅 추가)
        }

        cam.tag = "MainCamera";
        cam.enabled = true;
        cam.gameObject.SetActive(true);

        mainCamera = cam;
    }

    // ---------- 유틸 ----------
    void DisableByTypeName(GameObject go, string typeName)
    {
        var comp = go.GetComponent(typeName);
        if (comp is Behaviour b) b.enabled = false;
    }

    void EnableIfExists<T>(GameObject go, bool state) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) return;

        if (c is Behaviour b) { b.enabled = state; return; }

        // CharacterController는 Behaviour가 아님
        var cc = c as CharacterController;
        if (cc != null) { cc.enabled = state; return; }
    }
}
