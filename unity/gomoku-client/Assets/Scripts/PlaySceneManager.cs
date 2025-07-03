using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlaySceneManager : MonoBehaviour
{
    private const int BOARD_SIZE = 15; // 오목판 크기, 15로 고정
    private Intersection[,] board = new Intersection[BOARD_SIZE, BOARD_SIZE];

    private string latestBoardState;
    private int lastMoveX = -1; // 마지막 이동 X 좌표
    private int lastMoveY = -1; // 마지막 이동 Y 좌표

    [Header("Buttons")]
    [SerializeField]
    private GameObject restartButton;

    [Header("UI Elements")]
    [SerializeField]
    private Image resultImage;

    // GameOverPanel 인스턴스
    [SerializeField]
    private GameObject gameOverPanel;


    [Header("Output Displays")]
    [SerializeField]
    private TextMeshProUGUI myMatchHistoryDisplay; // 매치 히스토리 표시용 텍스트
    [SerializeField]
    private TextMeshProUGUI opponentMatchHistoryDisplay; // 상대 매치 히스토리 표시용 텍스트]

    [Header("Resources")]
    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;
 
    [SerializeField]
    private GameObject blackStonePrefab, blackStoneNewPrefab; // 교차점에 놓을 돌 프리팹
  
    [SerializeField]
    private GameObject whiteStonePrefab, whiteStoneNewPrefab; // 교차점에 놓을 돌 프리팹



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 내 MatchHistory 텍스트 설정
        myMatchHistoryDisplay.text = $"Username: {GameManager.instance.myInfo.username}\n" +
                                  $"Total Games: {GameManager.instance.myInfo.totalGames}\n" +
                                  $"Wins: {GameManager.instance.myInfo.wins}\n" +
                                  $"Draws: {GameManager.instance.myInfo.draws}\n" +
                                  $"Losses: {GameManager.instance.myInfo.losses}";

        // 상대 MatchHistory 텍스트 설정 (만약 AI라면 상대 매치 히스토리는 비워두고 AI 상대라고 표시)
        if (GameManager.instance.opponentInfo.isAI)
        {
            opponentMatchHistoryDisplay.text = "Opponent: AI\n";
        }
        else
        {
            opponentMatchHistoryDisplay.text = $"Username: {GameManager.instance.opponentInfo.username}\n" +
                                            $"Total Games: {GameManager.instance.opponentInfo.totalGames}\n" +
                                            $"Wins: {GameManager.instance.opponentInfo.wins}\n" +
                                            $"Draws: {GameManager.instance.opponentInfo.draws}\n" +
                                            $"Losses: {GameManager.instance.opponentInfo.losses}";
        }

        InitBoard(); // 게임 보드 초기화

        GameManager.instance.isGameDone = false; // 게임 상태 초기화

        gameOverPanel.SetActive(false); // 게임 오버 패널 비활성화
        restartButton.SetActive(false); // Restart 버튼 비활성화
        resultImage.gameObject.SetActive(false); // 결과 이미지 비활성화
    }

    private void InitBoard()
    {
        Debug.Log("[Log] Mapping intersections to board...");
        // Intersection 오브젝트를 찾아서 board 배열에 매핑
        var intersectionObjects = FindObjectsByType<Intersection>(FindObjectsSortMode.None);

        foreach (var intersection in intersectionObjects)
        {
            // 오목판상에서 교차점의 행과 열 인덱스를 가져와서 board 배열에 매핑
            int row = intersection.GetRowIndex();
            int col = intersection.GetColIndex();

            board[row, col] = intersection;
            if (board[row, col] == null)
            {
                Debug.LogError($"[Log] Intersection at ({row}, {col}) is null!");
            }
        }
    }
 
    public void SetLatestBoardState(string boardStr, int lastMoveX, int lastMoveY)
    {
        Debug.Log("[Log] Setting latest board state with last move...");
        // 최신 보드 상태와 마지막 이동 좌표를 저장
        latestBoardState = boardStr;
        this.lastMoveX = lastMoveX;
        this.lastMoveY = lastMoveY;

        Debug.Log($"[Log] Latest board state set: {latestBoardState}, Last move: ({lastMoveX}, {lastMoveY})");
    }

    private void Update()
    {
        if (GameManager.instance.isGameDone == false && !string.IsNullOrEmpty(latestBoardState))
        {
            Debug.Log("[Log] Updating board with latest state...");
            // 최신 보드 상태로 업데이트
            UpdateBoard(latestBoardState, lastMoveX, lastMoveY); // 마지막 이동 좌표는 -1, -1로 설정 (아직 알 수 없음)
            latestBoardState = null; // 업데이트 후 최신 상태 초기화
        }
    }

    private void UpdateBoard(string boardStr, int lastMoveX, int lastMoveY)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                char stoneType = boardStr[i * BOARD_SIZE + j];
                bool isLastMove = (i == lastMoveX && j == lastMoveY);

                if (stoneType == 'B') // 검은 돌
                {
                    try
                    {
                        board[i, j].SetStone(isLastMove ? blackStoneNewPrefab : blackStonePrefab);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Log] Error placing black stone at ({i}, {j}): {ex.Message}");
                    }
                }
                else if (stoneType == 'W') // 흰 돌
                {
                    try
                    {
                        board[i, j].SetStone(isLastMove ? whiteStoneNewPrefab : whiteStonePrefab);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Log] Error placing white stone at ({i}, {j}): {ex.Message}");
                    }
                }
            }
        }
        Debug.Log("[Log] Board state updated successfully.");
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
