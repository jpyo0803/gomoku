using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance = null;

    private const string serverUrl = "http://localhost:3000";

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

    // 게임 설정 패널

    private string jwtToken; // JWT 토큰

    private GomokuClient gomokuClient; // GomokuClient 인스턴스

    private RestAPIClient restApiClient; // REST API 클라이언트

    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 GameManager 유지
        }
        else
        {
            Destroy(gameObject); // 이미 존재하는 GameManager가 있으면 현재 오브젝트 삭제
        }
    }

    private void Start()
    {
        gomokuClient = new GomokuClient(); // Player1은 임시 플레이어 ID, 실제로는 로그인한 사용자 ID로 설정해야 함
        restApiClient = new RestAPIClient(); // REST API 클라이언트 초기화
    }

    public void SetJwtToken(string token)
    {
        jwtToken = token;
        Debug.Log($"[Log] JWT Token set: {jwtToken}");
    }

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

    public void ConnectGomokuClient()
    {
        if (gomokuClient != null)
        {
            Debug.Log($"[Log] Connecting GomokuClient with JWT Token: {jwtToken}");
            Console.WriteLine($"[Log] Connecting GomokuClient with JWT Tokenssssssss: {jwtToken}");
            gomokuClient.Connect(jwtToken);
        }
        else
        {
            Debug.LogError("GomokuClient is not initialized.");
        }
    }

    public async void SendMatchRequest(bool wantAiOpponent)
    {
        if (gomokuClient != null)
        {
            await gomokuClient.SendMatchRequest(wantAiOpponent);
        }
        else
        {
            Debug.LogError("GomokuClient is not initialized.");
        }
    }

    public async void SendPlaceStone(int row, int col)
    {
        if (gomokuClient != null)
        {
            await gomokuClient.SendPlaceStone(row, col);
        }
        else
        {
            Debug.LogError("GomokuClient is not initialized.");
        }
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

    public async Task<MatchHistory> GetMatchHistory()
    {
        try
        {
            string json = await restApiClient.RequestMatchHistory(serverUrl, jwtToken);

            Debug.Log($"[Log] Match history JSON: {json}");

            MatchHistory history = JsonConvert.DeserializeObject<MatchHistory>(json);
            return history;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"HTTP 오류: {e.Message}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"기타 오류: {e.Message}");
            return null;
        }
    }

    public void DebugPrint(string str)
    {
        Debug.Log($"[Log] PrintString: {str}");
    }
}
