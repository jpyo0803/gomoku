/*
    Reference:
    1. https://github.com/itisnajim/SocketIOUnity
*/

using UnityEngine;
using SocketIOClient;
// using SocketIOClient.Newtonsoft.Json;
// using Newtonsoft.Json.Linq;

public class GomokuClient : MonoBehaviour
{
    const int BOARD_SIZE = 15; // Gomoku board size
    private SocketIOUnity socket;

    void Start()
    {
        Debug.Log("Starting Gomoku Client...");

        ConnectSocket();
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

        // 연결
        socket.Connect();
    }

    void SendMatchRequest()
    {
        Debug.Log("Sending match request...");
        socket.Emit("match_request", new
        {
            playerId = "player123"
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
}
