using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance = null;

    private const int BOARD_SIZE = 15; // 오목판 크기, 15로 고정

    private Intersection[,] board = new Intersection[BOARD_SIZE, BOARD_SIZE];


    [SerializeField]
    private GameObject blackStonePrefab, blackStoneNewPrefab; // 교차점에 놓을 돌 프리팹
    [SerializeField]
    private GameObject whiteStonePrefab, whiteStoneNewPrefab; // 교차점에 놓을 돌 프리팹

    [SerializeField]
    private Image resultImage;

    [SerializeField]
    private GameObject playAgainButton;

    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;

    // 게임 설정 패널
    public GameObject gameSettingPanel; // 게임 설정 패널 오브젝트
    public Toggle wantAiOpponentToggle; // AI 상대 희망 여부 토글
    public Button okButton; // 게임 설정 패널의 확인 버튼

    private AuthInterface authClient;

    private string username = "test12343"; // 사용자 이름
    private string password = "1q2w3e4r"; // 사용자 비밀번호
    private string jwtToken; // JWT 토큰

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // authClient를 찾아 의존성 주입
        authClient = FindObjectOfType<AuthClient>();
        if (authClient == null)
        {
            Debug.LogError("AuthClient not found in the scene. Please add it to the scene.");
            return;
        }


        // TODO(jpyo0803): Dialog 창을 띄워 사용자 이름과 비밀번호 입력 받기
        // 회원가입
        authClient.SignUp(username, password, (resultCode) =>
        {
            if (resultCode == 201)
            {
                Debug.Log("[Log] SignUp successful.");
            }
            else
            {
                Debug.Log($"[Log] SignUp failed with code: {resultCode}");
            }
            // 로그인
            authClient.Login(username, password, (resultCode, token) =>
            {
                if (resultCode == 200)
                {
                    Debug.Log("[Log] Login successful.");
                    Debug.Log($"[Log] Token: {token}");
                    jwtToken = token; // JWT 토큰 저장

                    InitBoard();
                    // Initialize the result image to be inactive at the start
                    resultImage.gameObject.SetActive(false);
                    playAgainButton.SetActive(false);

                    okButton.onClick.AddListener(OnOkClicked);

                    // Gomoku server와 연결
                    var gomokuClient = FindObjectOfType<GomokuClient>();
                    if (gomokuClient == null)
                    {
                        Debug.LogError("GomokuClient not found in the scene.");
                        return;
                    }

                    gomokuClient.ConnectSocket(jwtToken); // JWT 토큰을 사용하여 서버에 연결
                }
                else
                {
                    Debug.LogError($"[Log] Login failed with code: {resultCode}");
                }
            });
        });
    }

    private void InitBoard()
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
        }
    }

    public void UpdateBoard(string boardStr, int lastMoveX, int lastMoveY)
    {
        Debug.Log("[Log] Updating board state...");
        // 보드 상태 업데이트 

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                char stoneType = boardStr[i * BOARD_SIZE + j];
                bool isLastMove = (i == lastMoveX && j == lastMoveY);

                if (stoneType == 'B') // 검은 돌
                {
                    board[i, j].SetStone(isLastMove ? blackStoneNewPrefab : blackStonePrefab);
                }
                else if (stoneType == 'W') // 흰 돌
                {
                    board[i, j].SetStone(isLastMove ? whiteStoneNewPrefab : whiteStonePrefab);
                }
                else // 빈 칸
                {
                    // 빈 칸일 경우 돌을 놓지 않음
                    // board[i, j].SetStone(null); // 이 줄은 필요 없음, 그냥 두면 빈 칸으로 유지됨
                }
            }
        }
    }

    public void SetGameResult(bool isWin)
    {
        // Display the result image based on the game outcome
        DisplayResultImage(isWin);
    }

    // Update is called once per frame
    private void DisplayResultImage(bool isWin)
    {

        if (isWin)
        {
            resultImage.sprite = winSprite;
        }
        else
        {
            resultImage.sprite = loseSprite;
        }

        resultImage.gameObject.SetActive(true);
        playAgainButton.SetActive(true);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("SampleScene");
    }

    void OnOkClicked()
    {
        bool wantAiOpponent = wantAiOpponentToggle.isOn; // 토글 상태에 따라 AI 상대 희망 여부 설정
        Debug.Log($"[Log] Want AI opponent: {wantAiOpponent}");

        // 매치 요청을 보내는 메소드 호출
        var gomokuClient = FindObjectOfType<GomokuClient>();
        if (gomokuClient == null)
        {
            Debug.LogError("GomokuClient not found in the scene.");
            return;
        }

        gomokuClient.SendMatchRequest(wantAiOpponent);

        // GameSettingPanel을 비활성화
        gameSettingPanel.SetActive(false);
    }

}
