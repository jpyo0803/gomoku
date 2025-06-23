using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettingManager : MonoBehaviour
{
    [SerializeField]
    private Toggle wantAiOpponentToggle; // AI 상대 희망 여부 토글
    [SerializeField]
    private Button okButton; // 게임 설정 패널의 확인 버튼

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        okButton.onClick.AddListener(OnOkClicked);
    }

    void OnOkClicked()
    {
        // AI 상대 희망 여부에 따라 게임 설정을 저장
        bool wantAiOpponent = wantAiOpponentToggle.isOn;

        // GameManager에 설정 전달
        GameManager.instance.SendMatchRequest(wantAiOpponent);

        // 게임 Scene로 전환
        SceneManager.LoadScene("PlayScene");
    }
}
