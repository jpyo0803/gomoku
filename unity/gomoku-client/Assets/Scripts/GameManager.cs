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

    public bool isGameDone = false; // 게임 종료 여부

    public PlayerInfo myInfo; // 내 정보
    public PlayerInfo opponentInfo; // 상대 정보


    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    public void SetPlayScene(PlayerInfo myInfo, PlayerInfo opponentInfo, string gameId)
    {
        Debug.Log($"[Log] Starting game with MyInfo: {myInfo.username}, OpponentInfo: {opponentInfo.username}, GameID: {gameId}");

        this.myInfo = myInfo;
        this.opponentInfo = opponentInfo;

        // PlayScene로 전환
        SceneManager.LoadScene("PlayScene");
    }

    public void UpdateBoard(string boardStr, int lastMoveX, int lastMoveY)
    {
        // PlaySceneManager 찾기
        try
        {
            PlaySceneManager playSceneManager = FindFirstObjectByType<PlaySceneManager>();
            if (playSceneManager != null)
            {
                playSceneManager.UpdateBoard(boardStr, lastMoveX, lastMoveY);
            }
            else
            {
                Debug.LogError("[Log] PlayManager not found in the scene.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Log] Error updating board: {ex.Message}");
        }
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
            PlaySceneManager playSceneManager = FindFirstObjectByType<PlaySceneManager>();
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
