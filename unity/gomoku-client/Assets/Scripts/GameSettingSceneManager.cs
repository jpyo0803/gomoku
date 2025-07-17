using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSettingSceneManager : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField]
    private Toggle wantAiOpponentToggle; // AI 상대 희망 여부 토글

    [Header("Buttons")]
    [SerializeField]
    private Button okButton; // 게임 설정 패널의 확인 버튼

    [Header("Output Displays")]
    [SerializeField]
    private TextMeshProUGUI matchHistoryDisplay; // 매치 히스토리 표시용 텍스트

    void Start()
    {
        // 매치 히스토리 업데이트
        DisplayMatchHistoryAsync();

        var websocketClient = GameManager.instance.WebSocketClient;
        if (websocketClient == null)
        {
            Debug.LogError("[Log Error] WebSocketClient is not initialized properly.");
            return;
        }

        // WebSocket 연결
        // TODO(jpyo0803): WebSocket 연결시 GameManager에서 WebSocketServerUrl과 AccessToken을 가져오는 구조가 좋은지 생각해보기
        websocketClient.Connect(GameManager.instance.WebSocketServerUrl, GameManager.instance.AccessToken);

        // 버튼 클릭 이벤트 등록
        okButton.onClick.AddListener(OnOkClicked);
    }

    private void OnOkClicked()
    {
        // AI 상대 희망 여부에 따라 게임 설정을 저장
        bool wantAiOpponent = wantAiOpponentToggle.isOn;

        var websocketClient = GameManager.instance.WebSocketClient;
        websocketClient.SendMatchRequest(wantAiOpponent);
    }

    private async void DisplayMatchHistoryAsync()
    {
        var matchHistory = await GameManager.instance.GetMatchHistoryAsync();

        matchHistoryDisplay.text = "[Match History]\n" +
                                    $"Username: {matchHistory.username}\n" +
                                    $"Total Games: {matchHistory.totalGames}\n" +
                                    $"Wins / Losses: {matchHistory.wins} / {matchHistory.losses}\n" +
                                    $"Win Rate: {(float)matchHistory.wins / matchHistory.totalGames * 100:F2}%\n";
    }
}
