/*
    Reference:
    1. https://github.com/itisnajim/SocketIOUnity
*/

using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class MatchMakingPayload
{
    public PlayerInfo my_info;
    public PlayerInfo opponent_info;
    public string game_id;
}

public class GomokuClient
{
    private const string uriBase = "http://localhost:3000";
    private SocketIO socket;

    public GomokuClient() { }

    public void Connect(string jwtToken)
    {
        var uri = new Uri(uriBase);

        this.socket = new SocketIO(uri, new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false,
            Query = new Dictionary<string, string>
            {
                { "token", jwtToken }
            }
        });


        socket.OnConnected += (_, _) =>
        {
            Console.WriteLine("[Log] Connected to server!");
        };

        socket.On("match_making_success", res =>
        {
            try
            {
                var json = res.ToString();
                Console.WriteLine($"[DEBUG] Received JSON: {json}");

                var payloads = JsonConvert.DeserializeObject<List<MatchMakingPayload>>(json);

                var payload = payloads[0];

                Console.WriteLine($"[DEBUG] Deserialized payload. Player: {payload.my_info.username}, Opponent: {payload.opponent_info.username}");

                GameManager.instance?.RunOnMainThread(() =>
                {
                    GameManager.instance.SetPlayScene(payload.my_info, payload.opponent_info, payload.game_id);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in match_making_success handler: {ex.Message}");
            }
        });

        socket.On("board_state", res =>
        {
            var map = res.GetValue<Dictionary<string, JsonElement>>();

            string boardStr = map["board_state"].GetString();
            int lastMoveX = map["last_move_x"].GetInt32();
            int lastMoveY = map["last_move_y"].GetInt32();

            // NOTE(jpyo0803): Unity 관련 객체들은 Pure C# 환경에서는 사용 불가능
            GameManager.instance?.RunOnMainThread(() =>
            {
                GameManager.instance.UpdateBoard(boardStr, lastMoveX, lastMoveY);
            });
            Console.WriteLine("[Log] Board state updated.");
        });

        socket.On("place_stone_resp", res =>
        {
            var map = res.GetValue<Dictionary<string, string>>();
            string result = map["result"];
            Console.WriteLine($"[Log] Place stone result: {result}");

            if (result == "win" || result == "lose")
            {
                bool isClientWin = result == "win";
                GameManager.instance?.RunOnMainThread(() =>
                {
                    GameManager.instance?.SetGameResult(isClientWin);
                });
                Console.WriteLine("[Log] Game result set.");
            }
        });

        socket.ConnectAsync(); // 비동기로 연결
    }

    public async Task SendMatchRequest(bool wantAiOpponent)
    {
        Console.WriteLine("[Log] Sending match request...");

        await socket.EmitAsync("match_request", new
        {
            wantAiOpponent = wantAiOpponent
        });
    }

    public async Task SendPlaceStone(int rowIndex, int colIndex)
    {
        if (GameManager.instance.isGameDone)
        {
            Console.WriteLine("[Log] Game is already done.");
            return;
        }

        Console.WriteLine($"[Log] Sending place_stone at ({rowIndex}, {colIndex})");

        await socket.EmitAsync("place_stone", new
        {
            x = rowIndex,
            y = colIndex,
        });
    }

    public async Task Disconnect()
    {
        if (socket == null) return;

        await socket.DisconnectAsync();
        socket.Dispose();
        socket = null;

        Console.WriteLine("[Log] Socket disconnected.");
    }
}
