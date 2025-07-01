using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Threading.Tasks;

public class GameSettingSceneManager : MonoBehaviour
{
    [SerializeField]
    private Toggle wantAiOpponentToggle; // AI 상대 희망 여부 토글
    [SerializeField]
    private Button okButton; // 게임 설정 패널의 확인 버튼
    [SerializeField]
    private TextMeshProUGUI matchHistoryText; // 매치 히스토리 표시용 텍스트

    void Start()
    {
        // 매치 히스토리 업데이트
        UpdateMatchHistoryText();
        // 버튼 클릭 이벤트 등록
        okButton.onClick.AddListener(OnOkClicked);
    }

    void OnOkClicked()
    {
        // AI 상대 희망 여부에 따라 게임 설정을 저장
        bool wantAiOpponent = wantAiOpponentToggle.isOn;

        // GameManager에 설정 전달
        GameManager.instance.SendMatchRequest(wantAiOpponent);
    }

    private async void UpdateMatchHistoryText()
    {
        try
        {
            MatchHistory history = await GameManager.instance.GetMatchHistory();

            matchHistoryText.text = "Match History:\n" +
                                    $"Username: {history.username}\n" +
                                    $"Total Games: {history.totalGames}\n" +
                                    $"Wins: {history.wins}\n" +
                                    $"Draws: {history.draws}\n" +
                                    $"Losses: {history.losses}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"매치 히스토리 로딩 실패: {ex.Message}");
        }
    }
}
