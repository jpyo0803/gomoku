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
      auto cells = body["cells"];  // 2D array of cells

      // Create a Gomoku board
      gomoku::Board board(size);
      for (int i = 0; i < size; ++i) {
        for (int j = 0; j < size; ++j) {
          int cell_value = cells[i][j];  // 0 = empty, 1 = black, 2 = white
          gomoku::Piece piece = static_cast<gomoku::Piece>(cell_value);
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