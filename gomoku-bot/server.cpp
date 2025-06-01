#include <iostream>
#include <websocketpp/config/asio_no_tls.hpp>
#include <websocketpp/server.hpp>
#include "gomoku_bot.h"
#include "gomoku_bot_strategy.h"
#include "json.hpp"

using json = nlohmann::json;
typedef websocketpp::server<websocketpp::config::asio> server;

class GomokuServer {
 public:
  GomokuServer() {
    ws_server.init_asio();
    ws_server.set_message_handler([this](websocketpp::connection_hdl hdl, server::message_ptr msg) {
      handle_message(hdl, msg);
    });
  }

  void run(uint16_t port) {
    ws_server.listen(port);
    ws_server.start_accept();
    std::cout << "WebSocket server started at ws://localhost:" << port << std::endl;
    ws_server.run();
  }

 private:
  server ws_server;

  void handle_message(websocketpp::connection_hdl hdl, server::message_ptr msg) {
    try {
      json req = json::parse(msg->get_payload());
      std::string type = req["type"];

      if (type == "your_turn_ai") {
        std::string flat = req["board"];
        std::string playerId = req["playerId"];

        const int size = kBoardSize;
        if (flat.size() != size * size) {
          throw std::runtime_error("Invalid board string length");
        }

        gomoku::Board board;
        for (int i = 0; i < size; ++i) {
          for (int j = 0; j < size; ++j) {
            char c = flat[i * size + j];
            gomoku::Piece piece;
            switch (c) {
              case 'B':
                piece = gomoku::Piece::kBlack;
                break;
              case 'W':
                piece = gomoku::Piece::kWhite;
                break;
              case '.':
                piece = gomoku::Piece::kEmpty;
                break;
              default:
                throw std::runtime_error(std::string("Invalid cell character: ") + c);
            }
            board.SetCell(i, j, piece);
          }
        }

        gomoku::GomokuBot bot;
        bot.set_strategy(std::make_unique<gomoku::MinimaxWithAlphaBetaPruning>());
        auto [x, y] = bot.Solve(board, /*max_depth=*/4);

        json resp = {{"type", "place_stone_ai"}, {"x", x}, {"y", y}, {"playerId", playerId}};
        ws_server.send(hdl, resp.dump(), msg->get_opcode());

      } else {
        std::cerr << "⚠️ Unknown message type: " << type << std::endl;
      }

    } catch (const std::exception& e) {
      json err = {{"type", "error"}, {"message", e.what()}};
      ws_server.send(hdl, err.dump(), msg->get_opcode());
    }
  }
};

int main() {
  GomokuServer server;
  server.run(8080);
  return 0;
}