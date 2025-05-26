#include <iostream>
#include <utility>
#include "gomoku_bot.h"
#include "gomoku_bot_strategy.h"

// For handling HTTP requests
#include "httplib.h"
#include "json.hpp"

using json = nlohmann::json;
using namespace httplib;
using namespace std;

int main(int argc, char** argv) {
  // Create an HTTP server
  Server svr;

  svr.Post("/solve", [&](const Request& req, Response& res) {
    try {
      // Parse the JSON request body
      auto body = json::parse(req.body);
      int size = body["size"];
      int max_depth = body["max_depth"];
      cout << endl << "[Request]" << endl;
      cout << "Board size: " << size << endl;
      cout << "Max depth: " << max_depth << endl;

      std::string flat = body["cells"];  // 1D 문자열 받기
      if (flat.size() != size * size) {
        throw std::runtime_error("Invalid board string length");
      }

      // Create a Gomoku board
      gomoku::Board board(size);

      for (int i = 0; i < size; ++i) {
        for (int j = 0; j < size; ++j) {
          char c = flat[i * size + j];
          gomoku::Piece piece;
          switch (c) {
            case '.':
              piece = gomoku::Piece::kEmpty;
              break;
            case 'B':
              piece = gomoku::Piece::kBlack;
              break;
            case 'W':
              piece = gomoku::Piece::kWhite;
              break;
            default:
              throw std::runtime_error(std::string("Invalid cell character: ") + c);
          }
          board.SetCell(i, j, piece);
        }
      }

      // Create a GomokuBot instance
      gomoku::GomokuBot bot;
      bot.set_strategy(std::make_unique<gomoku::MinimaxWithAlphaBetaPruning>());

      auto [x, y] = bot.Solve(board, max_depth);

      cout << "Best move: (" << x << ", " << y << ")" << endl;

      json resp = {
          {"x", x},
          {"y", y},
      };
      res.set_content(resp.dump(), "application/json");
    } catch (const std::exception& e) {
      res.status = 400;
      res.set_content(std::string("Error: ") + e.what(), "text/plain");
    }
  });

  std::cout << "Server started at http://localhost:8080" << std::endl;
  svr.listen("0.0.0.0", 8080);

  return 0;
}