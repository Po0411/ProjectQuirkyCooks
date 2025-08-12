#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyUIBuilderEditor
{
    [MenuItem("QuirkyCooks/Build Lobby UI")]
    public static void BuildLobbyUI()
    {
        // Canvas 준비
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject c = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = c.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = c.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
        }

        // EventSystem 준비
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // 루트 패널 생성(배경)
        GameObject bg = new GameObject("LobbyUI_Root", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvas.transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1f, 0.15f); // 연한 배경

        // 왼쪽 패널 (방 리스트)
        GameObject left = new GameObject("Panel_LeftRooms", typeof(RectTransform), typeof(Image));
        left.transform.SetParent(bg.transform, false);
        var lrt = left.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.05f);
        lrt.anchorMax = new Vector2(0.62f, 0.95f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        left.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        // Scroll View 생성
        GameObject scroll = CreateScrollView(left.transform, "RoomScrollView");

        // 오른쪽 패널 (버튼 영역)
        GameObject right = new GameObject("Panel_RightButtons", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        right.transform.SetParent(bg.transform, false);
        var rrt = right.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.68f, 0.2f);
        rrt.anchorMax = new Vector2(0.95f, 0.8f);
        rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
        right.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        var vlg = right.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
        vlg.spacing = 16f; vlg.padding = new RectOffset(24, 24, 24, 24);

        var csf = right.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // 상태 텍스트
        Text status = CreateText(right.transform, "StatusText", "대기 중…", 28, FontStyle.Bold, TextAnchor.MiddleCenter);

        // 코드 입력
        InputField input = CreateInputField(right.transform, "JoinCodeInput", "초대 코드를 입력하세요");

        // 버튼들
        Button btnJoin = CreateButton(right.transform, "BtnJoinRoom", "방 참가", 36);
        Button btnCreate = CreateButton(right.transform, "BtnCreateRoom", "방 생성", 36);

        // 버튼 클릭 시 LobbyManager 호출: 별도의 스크립트에 의존하지 않음
        btnJoin.onClick.RemoveAllListeners();
        btnJoin.onClick.AddListener(() =>
        {
            if (input == null || status == null) return;
            var code = input.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                status.text = "초대 코드를 입력하세요";
                return;
            }
            status.text = "로비 참가 중...";
            LobbyManager.Instance.JoinLobbyByCode(code);
        });
        btnCreate.onClick.RemoveAllListeners();
        btnCreate.onClick.AddListener(() =>
        {
            if (status == null) return;
            status.text = "로비 생성 중...";
            LobbyManager.Instance.CreateLobby(4);
        });

        // 선택 편의
        Selection.activeGameObject = bg;
        Debug.Log("[Lobby UI] 생성 완료.");
    }

    private static GameObject CreateScrollView(Transform parent, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.03f, 0.03f);
        rt.anchorMax = new Vector2(0.97f, 0.97f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);

        // Scroll View 표준 구성
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(root.transform, false);
        var vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
        var vpImg = viewport.GetComponent<Image>(); vpImg.color = new Color(1,1,1,0.0f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var ctRt = content.GetComponent<RectTransform>();
        ctRt.anchorMin = new Vector2(0, 1); ctRt.anchorMax = new Vector2(1, 1);
        ctRt.pivot = new Vector2(0.5f, 1f);
        ctRt.offsetMin = new Vector2(0,0); ctRt.offsetMax = new Vector2(0,0);

        var v = content.GetComponent<VerticalLayoutGroup>();
        v.childControlHeight = true; v.childForceExpandHeight = false;
        v.childControlWidth = true; v.childForceExpandWidth = true;
        v.spacing = 8; v.padding = new RectOffset(12,12,12,12);

        var f = content.GetComponent<ContentSizeFitter>();
        f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        f.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var sv = root.AddComponent<ScrollRect>();
        sv.viewport = viewport.GetComponent<RectTransform>();
        sv.content  = content.GetComponent<RectTransform>();
        sv.horizontal = false;

        // 테스트용 더미 항목 5개
        for (int i = 1; i <= 5; i++)
        {
            var btn = CreateButton(content.transform, $"RoomItem_{i}", $"방 {i}", 28);
            btn.onClick.AddListener(() => Debug.Log($"선택: {btn.name}"));
        }

        return root;
    }

    private static Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle style, TextAnchor align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = text;
        // Unity 2023 이상에서는 Arial.ttf 대신 LegacyRuntime.ttf를 사용해야 합니다
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = align;
        t.color = Color.black;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 60);
        return t;
    }

    private static InputField CreateInputField(Transform parent, string name, string placeholder)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        root.transform.SetParent(parent, false);
        root.GetComponent<Image>().color = Color.white;
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 60);

        // Text
        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(root.transform, false);
        var txt = textGo.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleLeft;
        txt.color = Color.black;
        txt.fontSize = 28;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0,0); trt.anchorMax = new Vector2(1,1);
        trt.offsetMin = new Vector2(16,12); trt.offsetMax = new Vector2(-16,-12);

        // Placeholder
        GameObject phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        phGo.transform.SetParent(root.transform, false);
        var pht = phGo.GetComponent<Text>();
        pht.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pht.text = placeholder;
        pht.fontSize = 24;
        pht.color = new Color(0,0,0,0.4f);
        pht.alignment = TextAnchor.MiddleLeft;
        var prt = phGo.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0,0); prt.anchorMax = new Vector2(1,1);
        prt.offsetMin = new Vector2(16,12); prt.offsetMax = new Vector2(-16,-12);

        var input = root.GetComponent<InputField>();
        input.textComponent = txt;
        input.placeholder = pht;

        return input;
    }

    private static Button CreateButton(Transform parent, string name, string label, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(1,1,1,0.9f);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 80);

        GameObject t = new GameObject("Text", typeof(RectTransform), typeof(Text));
        t.transform.SetParent(go.transform, false);
        var txt = t.GetComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        var trt = t.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.9f, 1f, 1f);
        button.colors = colors;

        return button;
    }
}
#endif
