using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// 버튼 클릭을 통해 LobbyManager에 로비 생성/참가 요청을 전달하고, 상태 텍스트를 업데이트합니다.
/// </summary>
public class LobbyUIButton : MonoBehaviour
{
    /// <summary>
    /// (선택 사항) 레거시 스크립트 호환을 위해 남겨둔 필드입니다. 현재 구현에서는 사용하지 않습니다.
    /// LobbyUIBuilderEditor.cs에서 relay 필드를 할당해도 컴파일 오류가 발생하지 않도록 하기 위한 용도입니다.
    /// 필요에 따라 프로젝트에 존재하는 RelayLobbyManager 등으로 타입을 변경할 수 있습니다.
    /// </summary>
    public MonoBehaviour relay;
    [Header("UI References")]
    public InputField joinCodeInput;
    public Text statusText;

    /// <summary>
    /// 방 참가 버튼 클릭 시 호출됩니다. 입력된 코드를 사용해 로비에 참가합니다.
    /// </summary>
    public void OnClickJoinRoom()
    {
        if (joinCodeInput == null || statusText == null)
            return;
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "초대 코드를 입력하세요";
            return;
        }
        statusText.text = "로비 참가 중...";
        // 비동기 결과는 LobbyManager 이벤트로 처리하며 여기서 await하지 않습니다.
        _ = LobbyManager.Instance.JoinLobbyByCode(code);
    }

    /// <summary>
    /// 방 생성 버튼 클릭 시 호출됩니다. 새 로비를 생성합니다.
    /// </summary>
    public void OnClickCreateRoom()
    {
        if (statusText == null)
            return;
        statusText.text = "로비 생성 중...";
        _ = LobbyManager.Instance.CreateLobby(4);
    }
}
