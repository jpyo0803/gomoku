/*
    Reference:
    1. https://github.com/itisnajim/SocketIOUnity
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;           
using Newtonsoft.Json;            

public class MatchMakingPayload
{
    public PlayerInfo my_info;
    public PlayerInfo opponent_info;
    public string game_id;
}

public abstract class Command
{
    protected string EventName;
    public string GetEventName()
    {
        return EventName;
    }

    public abstract object[] GetPayload();
}

public class MatchRequestCommand : Command
{
    private bool wantAiOpponent;

    public MatchRequestCommand(bool wantAiOpponent)
    {
        this.EventName = "match_request";
        this.wantAiOpponent = wantAiOpponent;
    }

    public override object[] GetPayload()
    {
        return new object[] { new { wantAiOpponent = this.wantAiOpponent } };
    }
}

public class PlaceStoneCommand : Command
{
    private int x;
    private int y;

    public PlaceStoneCommand(int x, int y)
    {
        this.EventName = "place_stone";
        this.x = x;
        this.y = y;
    }

    public override object[] GetPayload()
    {
        return new object[] { new { x = this.x, y = this.y } };
    }
}

public class WebSocketClient
{
    private SocketIO socket;

    // Command queue
    private Queue<Command> commandQueue = new Queue<Command>();

    public void Connect(string url, string accessToken)
    {
        var uri = new Uri(url);

        this.socket = new SocketIO(uri, new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false,
            Query = new Dictionary<string, string>
        {
            { "token", accessToken }
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
                Console.WriteLine($"[Log] Received JSON: {json}");

                var payloads = JsonConvert.DeserializeObject<List<MatchMakingPayload>>(json);

                var payload = payloads[0];

                Console.WriteLine($"[Log] Deserialized payload. Player: {payload.my_info.username}, Opponent: {payload.opponent_info.username}");

                if (GameManager.instance != null)
                {
                    GameManager.instance.RunOnMainThread(() =>
                    {
                        GameManager.instance.SetPlayScene(payload.my_info, payload.opponent_info, payload.game_id);
                        Console.WriteLine("[Log] Play scene set successfully.");
                    });
                }
                else
                {
                    Console.WriteLine("[Log Error] GameManager instance is null. Cannot set play scene.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] Exception in match_making_success handler: {ex.Message}");
            }
        });

        socket.On("board_state", res =>
        {
            var map = res.GetValue<Dictionary<string, JsonElement>>();

            string boardStr = map["board_state"].GetString();
            int newMoveX = map["last_move_x"].GetInt32();
            int newMoveY = map["last_move_y"].GetInt32();

            // NOTE(jpyo0803): Unity 관련 객체들은 Pure C# 환경에서는 사용 불가능

            if (GameManager.instance != null)
            {
                GameManager.instance.RunOnMainThread(() =>
                {
                    GameManager.instance.UpdateBoard(boardStr, newMoveX, newMoveY);
                });
                Console.WriteLine("[Log] Board state updated.");
            }
            else
            {
                Console.WriteLine("[Log Error] GameManager instance is null. Cannot update board state.");
            }
        });

        socket.On("place_stone_resp", res =>
        {
            var map = res.GetValue<Dictionary<string, string>>();
            string result = map["result"];
            Console.WriteLine($"[Log] Place stone result: {result}");

            if (result == "win" || result == "lose")
            {
                bool isClientWin = result == "win";

                if (GameManager.instance != null)
                {
                    GameManager.instance.RunOnMainThread(() =>
                    {
                        GameManager.instance.SetGameResult(isClientWin);
                    });
                    Console.WriteLine("[Log] Game result set.");
                }
                else
                {
                    Console.WriteLine("[Log Error] GameManager instance is null. Cannot set game result.");
                }
            }
        });

        socket.ConnectAsync(); // 비동기로 연결
    }

    // 매치 요청 커맨드를 큐에 추가
    public void SendMatchRequest(bool wantAiOpponent)
    {
        Console.WriteLine("[Log] Enqueue MatchRequest command");
        commandQueue.Enqueue(new MatchRequestCommand(wantAiOpponent));
    }

    // 착수 커맨드를 큐에 추가
    public void SendPlaceStone(int rowIndex, int colIndex)
    {
        if (GameManager.instance.isGameDone)
        {
            Console.WriteLine("[Log] Game is already done.");
            return;
        }

        Console.WriteLine($"[Log] Enqueue PlaceStone command, placement: ({rowIndex}, {colIndex})");
        commandQueue.Enqueue(new PlaceStoneCommand(rowIndex, colIndex));
    }

    /*
        커맨드 큐에서 커맨드를 처리.

        GameManager의 Update 메서드에서 주기적으로 비동기 호출됨.
    */
    public async Task ProcessCommands()
    {
        while (commandQueue.Count > 0)
        {
            var command = commandQueue.Dequeue();
            Console.WriteLine($"[Log] Processing command: {command.GetEventName()}");

            try
            {
                await socket.EmitAsync(command.GetEventName(), command.GetPayload());
                Console.WriteLine($"[Log] Command {command.GetEventName()} sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] Exception while sending command {command.GetEventName()}: {ex.Message}");
            }
        }
    }

    public async Task Disconnect()
    {
        if (socket == null) return;

        try
        {
            await socket.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Log] Exception during DisconnectAsync: {ex.Message}");
        }

        try
        {
            socket.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Log] Exception during Dispose: {ex.Message}");
        }

        socket = null;
        Console.WriteLine("[Log] Socket disconnected.");
    }
}
