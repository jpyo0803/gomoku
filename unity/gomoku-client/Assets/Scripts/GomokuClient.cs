/*
    Reference:
    1. https://github.com/itisnajim/SocketIOUnity
*/

using UnityEngine;
using SocketIOClient;
using Newtonsoft.Json.Linq;
// using SocketIOClient.Newtonsoft.Json;
using System.Collections.Generic;

public class GomokuClient : MonoBehaviour
{
    const int BOARD_SIZE = 15; // Gomoku board size
    Intersection[,] board = new Intersection[BOARD_SIZE, BOARD_SIZE];
    private SocketIOUnity socket;

    void Start()
    {
        Debug.Log("Starting Gomoku Client...");

        ConnectSocket();

        var intersectionObjects = FindObjectsOfType<Intersection>();
        foreach (var intersection in intersectionObjects)
        {
            int row = intersection.GetRowIndex();
            int col = intersection.GetColIndex();
            board[row, col] = intersection;
        }
    }

    private void ConnectSocket()
    {
        // 연결 대상 주소
        var uri = new System.Uri("http://localhost:3000");

        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = EngineIO.V4, // 엔진IO 버전
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        // 연결 이벤트 핸들러
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to server!");
            SendMatchRequest();
        };

        // match_success 응답 수신
        socket.On("match_success", response =>
        {
            Debug.Log("Match success received: " + response.GetValue());
        });

        socket.OnUnityThread("board_state", res =>
        {
            var map = res.GetValue<Dictionary<string, string>>();
            string boardStr = map["board_state"];
            Debug.Log(boardStr);
            UpdateBoard(boardStr);
        });
        // 연결
        socket.Connect();
    }

    void SendMatchRequest()
    {
        Debug.Log("Sending match request...");
        socket.Emit("match_request", new
        {
            playerId = "player123",
            wantAiOpponent = true // AI opponent 여부
        });
    }

    public void SendPlaceStone(int row_index, int col_index)
    {
        // 예시: 클릭 시 match_request 이벤트 전송
        Debug.Log("Mouse clicked, sending desired stone location...");
        socket.Emit("place_stone", new
        {
            x = row_index, // 예시 좌표
            y = col_index,  // 예시 좌표
            playerId = "player123" // 플레이어 ID
        });
    }

    private void UpdateBoard(string boardStr)
    {
        // 보드 상태를 업데이트하는 로직
        // 예시: 각 칸에 대해 클릭 이벤트를 설정
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                char stone = boardStr[i * BOARD_SIZE + j];
                // 여기서 stone에 따라 UI를 업데이트하거나 게임 상태를 변경할 수 있음
                if (stone == 'B')
                {
                    // 흑돌을 놓는 로직
                    board[i, j].SetStone(true);
                }
                else if (stone == 'W')
                {
                    // 백돌을 놓는 로직
                    board[i, j].SetStone(false);
                }
            }
        }
    }
}
