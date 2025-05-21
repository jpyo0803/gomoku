#include "gomoku_bot_strategy.h"
#include <iostream>
#include <limits>

namespace gomoku {

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  // Find candidates. Candiates are the empty cells +/- 2 from the black or white pieces.
  int board_size = board.size();
  std::vector<std::vector<int>> candidate_map(board_size, std::vector<int>(board_size, 0));

  for (int i = 0; i < board_size; ++i) {
    for (int j = 0; j < board_size; ++j) {
      auto cell = board.GetCell(i, j);
      if (cell == Piece::kEmpty) {
        continue;
      }

      for (int dx = -2; dx <= 2; ++dx) {
        for (int dy = -2; dy <= 2; ++dy) {
          if (dx == 0 && dy == 0) {
            continue;
          }
          int x = i + dx;
          int y = j + dy;

          // skip if out of range
          if (x < 0 || x >= board_size || y < 0 || y >= board_size) {
            continue;
          }
          if (board.GetCell(x, y) == Piece::kEmpty) {
            candidate_map[x][y] = 1;  // mark as candidate
          }
        }
      }
    }
  }

  int alpha = std::numeric_limits<int>::min();
  int beta = std::numeric_limits<int>::max();

  auto result = Minimax(board, max_depth, alpha, beta, true, candidate_map);
  std::cout << "Returned score: " << result.second << std::endl;
  return result.first;
}

std::pair<std::pair<int, int>, int64_t> MinimaxWithAlphaBetaPruning::Minimax(
    Board& board, int depth, int64_t alpha, int64_t beta, bool maximizing_player,
    std::vector<std::vector<int>>& candidate_map) const {
  if (depth == 0) {
    auto score = board.Evaluate(Piece::kBlack) - board.Evaluate(Piece::kWhite);
    return {{-1, -1}, score};
  }

  if (maximizing_player) {
    int64_t max_eval = std::numeric_limits<int64_t>::min();
    std::pair<int, int> best_move = {-1, -1};

    for (int i = 0; i < board.size(); ++i) {
      for (int j = 0; j < board.size(); ++j) {
        if (candidate_map[i][j] == 0) {
          continue;
        }
        int tmp_val = candidate_map[i][j];

        candidate_map[i][j] = 0;  // mark as visited
        board.SetCell(i, j, Piece::kBlack);

        for (int dx = -2; dx <= 2; ++dx) {
          for (int dy = -2; dy <= 2; ++dy) {
            if (dx == 0 && dy == 0) {
              continue;
            }
            int x = i + dx;
            int y = j + dy;

            // skip if out of range
            if (x < 0 || x >= board.size() || y < 0 || y >= board.size()) {
              continue;
            }
            if (board.GetCell(x, y) == Piece::kEmpty) {
              candidate_map[x][y]++;  // mark as candidate
            }
          }
        }

        auto result = Minimax(board, depth - 1, alpha, beta, false, candidate_map);
        auto eval = result.second;
        if (eval > max_eval) {
          max_eval = eval;
          best_move = {i, j};
        }
        alpha = std::max(alpha, eval);
        // Undo the move
        board.SetCell(i, j, Piece::kEmpty);

        candidate_map[i][j] = tmp_val;  // unmark
        for (int dx = -2; dx <= 2; ++dx) {
          for (int dy = -2; dy <= 2; ++dy) {
            if (dx == 0 && dy == 0) {
              continue;
            }
            int x = i + dx;
            int y = j + dy;

            // skip if out of range
            if (x < 0 || x >= board.size() || y < 0 || y >= board.size()) {
              continue;
            }
            if (board.GetCell(x, y) == Piece::kEmpty) {
              candidate_map[x][y]--;  // unmark as candidate
            }
          }
        }

        if (beta <= alpha) {
          break;  // beta cut-off
        }
      }
    }
    return {best_move, max_eval};
  } else {
    int64_t min_eval = std::numeric_limits<int64_t>::max();
    std::pair<int, int> best_move = {-1, -1};

    for (int i = 0; i < board.size(); ++i) {
      for (int j = 0; j < board.size(); ++j) {
        if (candidate_map[i][j] == 0) {
          continue;
        }
        int tmp_val = candidate_map[i][j];

        candidate_map[i][j] = 0;  // mark as visited
        board.SetCell(i, j, Piece::kWhite);

        for (int dx = -2; dx <= 2; ++dx) {
          for (int dy = -2; dy <= 2; ++dy) {
            if (dx == 0 && dy == 0) {
              continue;
            }
            int x = i + dx;
            int y = j + dy;

            // skip if out of range
            if (x < 0 || x >= board.size() || y < 0 || y >= board.size()) {
              continue;
            }
            if (board.GetCell(x, y) == Piece::kEmpty) {
              candidate_map[x][y]++;  // mark as candidate
            }
          }
        }

        auto result = Minimax(board, depth - 1, alpha, beta, true, candidate_map);
        auto eval = result.second;
        if (eval < min_eval) {
          min_eval = eval;
          best_move = {i, j};
        }
        beta = std::min(beta, eval);
        // Undo the move
        board.SetCell(i, j, Piece::kEmpty);

        candidate_map[i][j] = tmp_val;  // unmark
        for (int dx = -2; dx <= 2; ++dx) {
          for (int dy = -2; dy <= 2; ++dy) {
            if (dx == 0 && dy == 0) {
              continue;
            }
            int x = i + dx;
            int y = j + dy;

            // skip if out of range
            if (x < 0 || x >= board.size() || y < 0 || y >= board.size()) {
              continue;
            }
            if (board.GetCell(x, y) == Piece::kEmpty) {
              candidate_map[x][y]--;  // unmark as candidate
            }
          }
        }

        if (beta <= alpha) {
          break;  // alpha cut-off
        }
      }
    }
    return {best_move, min_eval};
  }
}
}  // namespace gomoku