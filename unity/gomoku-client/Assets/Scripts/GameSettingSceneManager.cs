using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace jpyo0803
{
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

        async void Start()
        {
            var t1 = GameManager.instance.ConnectWebSocketAsync();
            // 매치 히스토리 업데이트
            var t2 = DisplayMatchHistoryAsync();

            // 버튼 클릭 이벤트 등록
            okButton.onClick.AddListener(OnOkClicked);

            // 매치 히스토리 표시와 웹소켓 연결을 완료할 때까지 대기
            await Task.WhenAll(t1, t2);
        }

        private void OnOkClicked()
        {
            // AI 상대 희망 여부에 따라 게임 설정을 저장
            bool wantAiOpponent = wantAiOpponentToggle.isOn;

            GameManager.instance.SendMatchRequestAsync(wantAiOpponent);
        }

        private async Task DisplayMatchHistoryAsync()
        {
            var matchHistory = await GameManager.instance.GetMatchHistoryAsync();

            matchHistoryDisplay.text = "[Match History]\n" +
                                        $"Username: {matchHistory.username}\n" +
                                        $"Total Games: {matchHistory.totalGames}\n" +
                                        $"Wins / Losses: {matchHistory.wins} / {matchHistory.losses}\n" +
                                        $"Win Rate: {(float)matchHistory.wins / matchHistory.totalGames * 100:F2}%\n";
        }
    }
}