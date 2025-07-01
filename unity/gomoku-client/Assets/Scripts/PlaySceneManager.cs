using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlaySceneManager : MonoBehaviour
{
    // Restart 버튼 
    [SerializeField]
    private GameObject restartButton;

    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;

    [SerializeField]
    private Image resultImage;

    // GameOverPanel 인스턴스
    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private GameObject leftPlayInfoPanel; // 플레이 정보 패널
    private GameObject rightPlayInfoPanel; // 플레이 정보 패널


    [SerializeField]
    private TextMeshProUGUI myMatchHistoryText; // 매치 히스토리 표시용 텍스트
    [SerializeField]
    private TextMeshProUGUI opponentMatchHistoryText; // 상대 매치 히스토리 표시용 텍스트]


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 내 MatchHistory 텍스트 설정
        myMatchHistoryText.text = $"Username: {GameManager.instance.myInfo.username}\n" +
                                  $"Total Games: {GameManager.instance.myInfo.totalGames}\n" +
                                  $"Wins: {GameManager.instance.myInfo.wins}\n" +
                                  $"Draws: {GameManager.instance.myInfo.draws}\n" +
                                  $"Losses: {GameManager.instance.myInfo.losses}";

        // 상대 MatchHistory 텍스트 설정 (만약 AI라면 상대 매치 히스토리는 비워두고 AI 상대라고 표시)
        if (GameManager.instance.opponentInfo.isAI)
        {
            opponentMatchHistoryText.text = "Opponent: AI\n";
        }
        else
        {
            opponentMatchHistoryText.text = $"Username: {GameManager.instance.opponentInfo.username}\n" +
                                            $"Total Games: {GameManager.instance.opponentInfo.totalGames}\n" +
                                            $"Wins: {GameManager.instance.opponentInfo.wins}\n" +
                                            $"Draws: {GameManager.instance.opponentInfo.draws}\n" +
                                            $"Losses: {GameManager.instance.opponentInfo.losses}";
        }

        GameManager.instance.InitBoard(); // 게임 보드 초기화

        GameManager.instance.isGameDone = false; // 게임 상태 초기화

        gameOverPanel.SetActive(false); // 게임 오버 패널 비활성화
        restartButton.SetActive(false); // Restart 버튼 비활성화
        resultImage.gameObject.SetActive(false); // 결과 이미지 비활성화
    }

    public void DisplayGameResult(bool isWin)
    {
        Debug.Log($"[Log] Game result: {(isWin ? "Win" : "Lose")}");
        resultImage.sprite = isWin ? winSprite : loseSprite; // 승리 또는 패배 이미지 설정
        gameOverPanel.SetActive(true); // 게임 오버 패널 활성화
        resultImage.gameObject.SetActive(true); // 결과 이미지 활성화
        restartButton.SetActive(true); // Restart 버튼 활성화
    }

    public void OnClickRestartButton()
    {
        // GameSeetingScene로 이동
        Debug.Log("[Log] Restart button clicked. Loading GameSettingScene...");
        SceneManager.LoadScene("GameSettingScene");
    }
}
