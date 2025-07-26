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

namespace jpyo0803
{
    public class MatchMakingPayload
    {
        public PlayerInfo my_info;
        public PlayerInfo opponent_info;
        public string game_id;
    }

    public class WebSocketClient
    {
        private SocketIO socket;

        private readonly ILogger logger;

        public WebSocketClient()
        {
            this.logger = ServiceLocator.Get<ILogger>();
            if (this.logger == null)
            {
                throw new Exception("ILogger service is not registered.");
            }
        }

        public async Task ConnectAsync(string url, string accessToken)
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
                logger.Log("WebSocket connected successfully.");
                // 소켓을 통해 서버에 바인딩 요청
                Bind().Wait();
            };

            socket.On("match_making_success", res =>
            {
                try
                {
                    var json = res.ToString();

                    var payloads = JsonConvert.DeserializeObject<List<MatchMakingPayload>>(json);

                    var payload = payloads[0];

                    logger.Log($"Match making success: Game ID: {payload.game_id}, Opponent: {payload.opponent_info.username}");

                    if (GameManager.instance != null)
                    {
                        GameManager.instance.RunOnMainThread(() =>
                        {
                            GameManager.instance.SetPlayScene(payload.my_info, payload.opponent_info, payload.game_id);
                        });
                    }
                    else
                    {
                        logger.LogError("GameManager instance is null. Cannot set play scene.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception in match_making_success handler: {ex.Message}");
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
                    logger.Log("Board state updated successfully.");
                }
                else
                {
                    logger.LogError("GameManager instance is null. Cannot update board state.");
                }
            });

            socket.On("place_stone_resp", res =>
            {
                var map = res.GetValue<Dictionary<string, string>>();
                string result = map["result"];

                logger.Log($"Place stone response received: {result}");

                if (result == "win" || result == "lose")
                {
                    bool isClientWin = result == "win";

                    if (GameManager.instance != null)
                    {
                        GameManager.instance.RunOnMainThread(() =>
                        {
                            GameManager.instance.SetGameResult(isClientWin);
                        });

                        logger.Log($"Game result set: {isClientWin}");
                    }
                    else
                    {
                        logger.LogError("GameManager instance is null. Cannot set game result.");
                    }
                }
            });

            socket.On("request_result", res =>
            {
                /*
                    NestJS 서버로부터 Request 수행 결과를 수신

                    예로 착수 성공시 아래와 같은 형태로 응답을 받음:
                    {
                        "result": "ok",
                        "message": "Stone placed successfully"
                    }
                */
                var map = res.GetValue<Dictionary<string, string>>();
                string result = map["result"];
                string message = map["message"];
                logger.Log($"Request result received: {result}, Message: {message}");

                if (result == "ok")
                {

                }
                else if (result == "error")
                {
                    if (message == "Token expired")
                    {
                        GameManager.instance.RunOnMainThread(async () =>
                        {
                            await GameManager.instance.RefreshToken();
                        });
                    }
                    else
                    {
                        logger.LogError($"Error processing command: {message}");
                    }
                }
                else
                {
                    logger.LogError($"Command processing failed: {message}");
                }
            });

            await socket.ConnectAsync(); // 비동기로 연결
        }

        // 매치 요청 커맨드를 큐에 추가
        public async Task SendRequest(Command command)
        {
            if (socket == null)
            {
                logger.LogError("WebSocket is not initialized. Cannot send request.");
                return;
            }

            logger.Log($"Sending command: {command.GetEventName()}");
            await socket.EmitAsync(command.GetEventName(), command.GetPayload());
        }

        public async Task Bind()
        {
            if (socket == null)
            {
                logger.LogError("WebSocket is not initialized. Cannot bind.");
                return;
            }

            try
            {
                await socket.EmitAsync("bind", new { });
                logger.Log("WebSocket bound successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error binding WebSocket: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (socket == null) return;

            try
            {
                await socket.DisconnectAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"[Log] Exception during disconnect: {ex.Message}");
            }

            try
            {
                socket.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError($"[Log] Exception during socket disposal: {ex.Message}");
            }

            socket = null;
            logger.Log("WebSocket disconnected and disposed successfully.");
        }
    }
}
