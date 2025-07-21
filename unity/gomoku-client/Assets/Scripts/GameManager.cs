using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace jpyo0803
{
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

        private RestApiClient restApiClient;// REST API 클라이언트 인스턴스

        private IHttpProxy _httpProxy;

        public WebSocketClient WebSocketClient { get; private set; } // WebSocket 클라이언트 인스턴스

        private IAuth _authService; // 인증 클라이언트 인스턴스

        public ITokenStorage _tokenStorage = new TokenStorage(); // 토큰 저장소 인터페이스

        public string AuthServerUrl { get; set; } = "http://localhost:3000";

        private string backendServerUrl = "http://localhost:3000";

        public string WebSocketServerUrl { get; } = "http://localhost:3000"; // WebSocket 서버 URL

        public bool isGameDone = false; // 게임 종료 여부

        public PlayerInfo myInfo; // 내 정보
        public PlayerInfo opponentInfo; // 상대 정보

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

        public void Start()
        {
            // ServiceLocator에 Register는 BootSceneManager에서 Awake에서 이루어지므로 Start에서 아래 코드를 호출해야함.
            restApiClient = new RestApiClient(); // REST API 클라이언트 초기화
            WebSocketClient = new WebSocketClient(); // WebSocket 클라이언트 초기화
            _authService = new AuthService(); // 인증 서비스 초기화
            _httpProxy = new HttpProxy(); // HTTP 프록시 초기화
        }


        public void SetPlayScene(PlayerInfo myInfo, PlayerInfo opponentInfo, string gameId)
        {
            Debug.Log($"[Log] Starting game with MyInfo: {myInfo.username}, OpponentInfo: {opponentInfo.username}, GameID: {gameId}");

            this.myInfo = myInfo;
            this.opponentInfo = opponentInfo;

            // PlayScene로 전환
            SceneManager.LoadScene("PlayScene");
        }

        public void UpdateBoard(string boardStr, int newMoveX, int newMoveY)
        {
            // PlaySceneManager 찾기
            try
            {
                PlaySceneManager playSceneManager = FindFirstObjectByType<PlaySceneManager>();
                if (playSceneManager != null)
                {
                    playSceneManager.SetLatestBoardState(boardStr, newMoveX, newMoveY);
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

            // WebSocket 클라이언트의 Command 처리
            if (WebSocketClient != null)
            {
                // 비동기로 백그라운드 처리
                WebSocketClient.ProcessCommands();
            }
        }

        public async Task<int> SignUp(string username, string password)
        {
            if (_authService == null)
            {
                Debug.LogError("[Log Error] AuthService is not initialized properly.");
                return -1;
            }

            // AuthService를 통해 회원가입 요청
            var response = await _authService.SignUp(new SignUpLoginDto
            {
                ServerUrl = AuthServerUrl,
                Username = username,
                Password = password
            });
            return response.Code;
        }

        public async Task<int> Login(string username, string password)
        {
            if (_authService == null)
            {
                Debug.LogError("[Log Error] AuthService is not initialized properly.");
                return -1;
            }
            // AuthService를 통해 로그인 요청
            var response = await _authService.Login(new SignUpLoginDto
            {
                ServerUrl = AuthServerUrl,
                Username = username,
                Password = password
            });

            // 로그인 성공 시 토큰 저장
            if (response.Code == (int)System.Net.HttpStatusCode.OK)
            {
                var t1 = _tokenStorage.UpdateAccessTokenAsync(response.AccessToken);
                var t2 = _tokenStorage.UpdateRefreshTokenAsync(response.RefreshToken);
                await Task.WhenAll(t1, t2);
            }

            return response.Code;
        }

        public async Task<MatchHistory> GetMatchHistoryAsync()
        {
            string url = $"{backendServerUrl}/Sql/my-match-history";
            string accessToken = await _tokenStorage.GetAccessTokenAsync();

            try
            {
                // JWT 토큰을 Authorization 헤더에 추가하여 요청
                HttpResponseMessage response = await _httpProxy.GetAsync(new HttpArgs
                {
                    Url = url,
                    Token = accessToken
                });
                var responseBody = await response.Content.ReadAsStringAsync();

                // 응답을 MatchHistory 객체로 변환
                MatchHistory history = JsonUtility.FromJson<MatchHistory>(responseBody);
                return history;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Log Error] GetMatchHistoryAsync error: {e.Message}");
                return null; // 예외 발생 시 null 반환
            }
        }
    }
}