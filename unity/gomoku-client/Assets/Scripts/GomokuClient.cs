/*
    Reference:
    1. https://github.com/itisnajim/SocketIOUnity
*/

using UnityEngine;
using SocketIOClient;
using System.Collections.Generic;

public class GomokuClient : MonoBehaviour
{
    private const string uri_base = "http://localhost:3000";
    private const int BOARD_SIZE = 15; // 오목판 크기, 15로 고정
    private Intersection[,] board = new Intersection[BOARD_SIZE, BOARD_SIZE];
    private SocketIOUnity socket;
    private bool matchRequested = false; // 매치 요청 여부
    private string playerId; // 플레이
    private string opponentId;
    private bool isBlackStone;
    private string gameId;

    private bool isGameDone = false; // 게임 종료 여부
    private void Start()
    {
        Debug.Log("[Log] GomokuClient Start() called");
        ConnectSocket();
        MapIntersectionsToBoard();

        // 랜덤 스트링을 생성하여 playerId로 사용
        playerId = System.Guid.NewGuid().ToString(); // 예시로 GUID 사용
    }

    private void ConnectSocket()
    {
        // 연결 대상 주소
        var uri = new System.Uri(uri_base);

        this.socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = EngineIO.V4, // 엔진IO 버전
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false, // 소켓 라이브러리가 자의로 재연결하지 않도록 설정
        });

        // 연결 이벤트 핸들러
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("[Log] Connected to server!");
            SendMatchRequest();
        };

        // match_making_success 응답 수신
        socket.On("match_making_success", res =>
        {
            // gameId(string)와 stone color(string)를 받아옴
            var data = res.GetValue<Dictionary<string, string>>();
            // this.playerId = data["player_id"]; // 플레이어 ID
            this.opponentId = data["opponent_id"]; // 상대 플레이어 ID
            this.gameId = data["game_id"];
            this.isBlackStone = data["stone_color"] == "black"; // 흑돌 여부 확인

            Debug.Log($"[Log] Match making success! Opponent Id: {opponentId} Game ID: {gameId}, Stone Color: {(isBlackStone ? "Black" : "White")}");
        });

        // OnUnityThread를 사용해야 Unity API 안전하게 호출 가능
        socket.OnUnityThread("board_state", res =>
        {
            var map = res.GetValue<Dictionary<string, string>>();
            string boardStr = map["board_state"];
            Debug.Log(boardStr);
            UpdateBoard(boardStr);
            
            Debug.Log("[Log] Board state updated successfully.");
        });

        socket.OnUnityThread("place_stone_resp", res =>
        {
            var map = res.GetValue<Dictionary<string, string>>();
            string result = map["result"];
            Debug.Log($"[Log] Place stone response: {result}");
            if (result == "win" || result == "lose")
            {
                this.isGameDone = true; // 게임 종료 상태로 설정

                // ResultUI 컴포넌트에 게임 결과 통보

                try
                {
                    // GameManager 찾기
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        bool isClientWin = result == "win";
                        gameManager.SetGameResult(isClientWin); // 게임 결과 이미지 표시
                        Debug.Log("[Log] Game result displayed successfully.");
                    }
                    else
                    {
                        Debug.LogWarning("[Log] GameManager component not found in the scene.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Exception occurred: " + ex.Message);
                }
                // if (resultUI != null)
                // {
                //     bool isClientWin = result == "win";
                //     resultUI.NotifyGameResult(isClientWin);
                // }
                // else
                // {
                //     Debug.LogWarning("[Log] ResultUI component not found in the scene.");
                // }

            }
        });

        socket.Connect(); // 소켓 서버에 연결
    }

    private void MapIntersectionsToBoard()
    {
        Debug.Log("[Log] Mapping intersections to board...");
        // Intersection 오브젝트를 찾아서 board 배열에 매핑
        var intersectionObjects = FindObjectsOfType<Intersection>();
        foreach (var intersection in intersectionObjects)
        {
            int row = intersection.GetRowIndex();
            int col = intersection.GetColIndex();
            board[row, col] = intersection;
        }
    }

    private void SendMatchRequest()
    {
        Debug.Log("[Log] Sending match request...");
        if (matchRequested)
        {
            return;
        }
        matchRequested = true;

        socket.Emit("match_request", new
        {
            playerId = this.playerId, // 요청 플레이어 ID (유일함 보장할 것)
            wantAiOpponent = true // AI 상대 희망 여부
        });
    }

    private void UpdateBoard(string boardStr)
    {
        Debug.Log("[Log] Updating board state...");
        // 보드 상태 업데이트 
        // TODO(jpyo0803): 변화가 있는 부분만 업데이트하도록 최적화 필요
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                char stone = boardStr[i * BOARD_SIZE + j];
                if (stone == 'B')
                {
                    board[i, j].SetStone(true); // 흑돌 착수
                }
                else if (stone == 'W')
                {
                    board[i, j].SetStone(false); // 백돌 착수
                }
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[Log] GomokuClient OnDestroy() called");

        if (socket == null) return;
        socket.Disconnect(); // 소켓 연결 해제
        socket.Dispose(); // 소켓 리소스 해제
        socket = null; // 소켓 객체 초기화
    }

    public void SendPlaceStone(int row_index, int col_index)
    {
        if (isGameDone)
        {
            Debug.LogWarning("[Log] Game is already done. Cannot place stone.");
            return;
        }
        Debug.Log($"[Log] Sending place_stone event at ({row_index}, {col_index})");

        socket.Emit("place_stone", new
        {
            x = row_index, // x 좌표 (행 인덱스)
            y = col_index,  // y 좌표 (열 인덱스)
            playerId = this.playerId // 플레이어 ID (유일함 보장할 것)
        });
    }
}
