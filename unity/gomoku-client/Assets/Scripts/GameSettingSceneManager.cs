using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
        DisplayMatchHistory();
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

    private async void DisplayMatchHistory()
    {
        var restApiClient = GameManager.instance.RestApiClient;

        string serverUrl = GameManager.instance.BackendServerUrl;
        string jwtToken = GameManager.instance.JwtToken;

        HttpMethod method = HttpMethod.Get;
        string url = $"{serverUrl}/Sql/my-match-history";

        try
        {
            // JWT 토큰을 Authorization 헤더에 추가하여 요청
            string responseBody = await restApiClient.SendRequest(method, url, jwtToken);

            // 응답을 MatchHistory 객체로 변환
            MatchHistory history = JsonUtility.FromJson<MatchHistory>(responseBody);

            // 매치 히스토리 표시
            matchHistoryDisplay.text = "[Match History]\n" +
                                       $"Username: {history.username}\n" +
                                       $"Total Games: {history.totalGames}\n" +
                                       $"Wins / Losses: {history.wins} / {history.losses}\n" +
                                       $"Win Rate: {(float)history.wins / history.totalGames * 100:F2}%\n";
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[Log Error] Failed to load match history: {e.Message}");
            matchHistoryDisplay.text = "Failed to load match history.";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Log Error] An error occurred: {ex.Message}");
            matchHistoryDisplay.text = "An error occurred while loading match history.";
        }
    }
}
