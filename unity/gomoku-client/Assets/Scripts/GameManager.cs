using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
public class PlayerInfo
{
    public string username;
    public bool isAI;
    public string stoneColor;    // "black" or "white"
    public int totalGames;
    public int wins;
    public int draws;
    public int losses;
}

public class MatchHistory
{
    public string username;
    public int totalGames;
    public int wins;
    public int draws;
    public int losses;
}

public class GameManager : MonoBehaviour
{
    // GameManager는 싱글턴 패턴을 통해 전역에서 접근 가능
    public static GameManager instance = null;

    public RestApiClient RestApiClient { get;  private set; } // REST API 클라이언트 인스턴스

    public WebSocketClient WebSocketClient { get; private set; } // WebSocket 클라이언트 인스턴스

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 GameManager 유지

            RestApiClient = new RestApiClient(); // REST API 클라이언트 초기화
            WebSocketClient = new WebSocketClient(); // WebSocket 클라이언트 초기화
        }
        else
        {
            Destroy(gameObject); // 이미 존재하는 GameManager가 있으면 현재 오브젝트 삭제
        }
    }

    public string JwtToken { get; set; } // JWT 토큰

    public string AuthServerUrl { get; } = "http://localhost:3000";

    public string BackendServerUrl { get; } = "http://localhost:3000";

    public string WebSocketServerUrl { get; } = "http://localhost:3000"; // WebSocket 서버 URL

    private const int BOARD_SIZE = 15; // 오목판 크기, 15로 고정

    private Intersection[,] board = new Intersection[BOARD_SIZE, BOARD_SIZE];

    public bool isGameDone = false; // 게임 종료 여부

    public PlayerInfo myInfo; // 내 정보
    public PlayerInfo opponentInfo; // 상대 정보


    [SerializeField]
    private GameObject blackStonePrefab, blackStoneNewPrefab; // 교차점에 놓을 돌 프리팹
    [SerializeField]
    private GameObject whiteStonePrefab, whiteStoneNewPrefab; // 교차점에 놓을 돌 프리팹

    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;

    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    public void SetPlayScene(PlayerInfo myInfo, PlayerInfo opponentInfo, string gameId)
    {
        Debug.Log($"[Log] Starting game with MyInfo: {myInfo.username}, OpponentInfo: {opponentInfo.username}, GameID: {gameId}");

        this.myInfo = myInfo;
        this.opponentInfo = opponentInfo;

        // PlayScene로 전환
        SceneManager.LoadScene("PlayScene");
    }

    public void InitBoard()
    {
        Debug.Log("[Log] Mapping intersections to board...");
        // Intersection 오브젝트를 찾아서 board 배열에 매핑
        var intersectionObjects = FindObjectsOfType<Intersection>();

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
            else
            {
                Debug.Log($"[Log] Intersection at ({row}, {col}) mapped successfully.");
            }
        }
    }

    public void UpdateBoard(string boardStr, int lastMoveX, int lastMoveY)
    {
        Debug.Log("[Log] Updating board state...");
        // 보드 상태 업데이트 
        Debug.Log($"[Log] Board string: {boardStr}, Last move: ({lastMoveX}, {lastMoveY})");

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                char stoneType = boardStr[i * BOARD_SIZE + j];
                bool isLastMove = (i == lastMoveX && j == lastMoveY);

                if (stoneType == 'B') // 검은 돌
                {
                    Debug.Log($"[Log] Placing black stone at ({i}, {j}), Last move: {isLastMove}");
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
                    Debug.Log($"[Log] Placing white stone at ({i}, {j}), Last move: {isLastMove}");
                    try
                    {
                        board[i, j].SetStone(isLastMove ? whiteStoneNewPrefab : whiteStonePrefab);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Log] Error placing white stone at ({i}, {j}): {ex.Message}");
                    }
                }
                else // 빈 칸
                {
                    // 빈 칸일 경우 돌을 놓지 않음
                    // board[i, j].SetStone(null); // 이 줄은 필요 없음, 그냥 두면 빈 칸으로 유지됨
                }
            }
        }
        Debug.Log("[Log] Board state updated successfully.");
    }

    public void SetGameResult(bool isWin)
    {
        // Display the result image based on the game outcome
        isGameDone = true; // 게임 종료 상태 설정
        DisplayGameResult(isWin);
    }

    // Update is called once per frame
    private void DisplayGameResult(bool isWin)
    {
        // PlayManager 찾기 
        try
        {
            PlaySceneManager playSceneManager = FindObjectOfType<PlaySceneManager>();
            if (playSceneManager != null)
            {
                playSceneManager.DisplayGameResult(isWin);
            }
            else
            {
                Debug.LogError("[Log] PlayManager not found in the scene.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Log] Error displaying game result: {ex.Message}");
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameSettingScene");
    }

    public void RunOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    private void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            var action = mainThreadActions.Dequeue();
            action.Invoke();
        }
    }
}
