using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayManager : MonoBehaviour
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
