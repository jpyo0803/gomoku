#include "gomoku_bot_strategy.h"
#include <cassert>
#include <iostream>
#include <limits>

#define ALPHA_BETA_PRUNING 1

namespace {
int64_t call_count = 0;
}

namespace gomoku {

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  // Find candidates. Candiates are the empty cells +/- 2 from the black or white pieces.
  int board_size = board.size();
  // std::vector<std::vector<bool>> candidate_map(board_size, std::vector<bool>(board_size, false));
  assert(board_size == kBoardSize);
  assert(max_depth <= 4);

  std::vector<std::bitset<kBoardSize>> candidate_map(board_size, std::bitset<kBoardSize>(0));
  for (int i = 0; i < kBoardSize; ++i) {
    for (int j = 0; j < kBoardSize; ++j) {
      if (board.GetCell(i, j) == Piece::kEmpty) {
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
            candidate_map.at(x).set(y);  // mark as candidate
          }
        }
      }
    }
  }

  int alpha = std::numeric_limits<int>::min();
  int beta = std::numeric_limits<int>::max();

  auto result = Minimax(board, max_depth, alpha, beta, true, candidate_map);
  std::cout << "Returned score: " << result.second << std::endl;
  std::cout << "Call count: " << call_count << std::endl;
  return result.first;
}

std::pair<std::pair<int, int>, int64_t> MinimaxWithAlphaBetaPruning::Minimax(
    Board& board, int depth, int64_t alpha, int64_t beta, bool maximizing_player,
    std::vector<std::bitset<kBoardSize>> candidate_map) const {
  call_count++;
  if (depth == 0) {
    auto score = board.Evaluate(Piece::kBlack) - board.Evaluate(Piece::kWhite);
    return {{-1, -1}, score};
  }

  if (maximizing_player) {
    int64_t max_eval = std::numeric_limits<int64_t>::min();
    std::pair<int, int> best_move = {-1, -1};

    for (int i = 0; i < board.size(); ++i) {
      for (int j = 0; j < board.size(); ++j) {
        if (candidate_map.at(i).test(j) == false) {
          continue;
        }
        auto next_candidate_map = candidate_map;
        next_candidate_map.at(i).reset(j);  // mark as not candidate

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
              next_candidate_map.at(x).set(y);  // mark as candidate
            }
          }
        }

        auto result = Minimax(board, depth - 1, alpha, beta, false, next_candidate_map);
        auto eval = result.second;
        if (eval > max_eval) {
          max_eval = eval;
          best_move = {i, j};
        }
        // Undo the move
        board.SetCell(i, j, Piece::kEmpty);

#if ALPHA_BETA_PRUNING == 1
        alpha = std::max(alpha, eval);
        if (beta <= alpha) {
          break;  // beta cut-off
        }
#endif
      }
    }
    return {best_move, max_eval};
  } else {
    int64_t min_eval = std::numeric_limits<int64_t>::max();
    std::pair<int, int> best_move = {-1, -1};

    for (int i = 0; i < board.size(); ++i) {
      for (int j = 0; j < board.size(); ++j) {
        if (candidate_map.at(i).test(j) == false) {
          continue;
        }
        auto next_candidate_map = candidate_map;

        next_candidate_map.at(i).reset(j);  // mark as not candidate

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
              next_candidate_map.at(x).set(y);  // mark as candidate
            }
          }
        }

        auto result = Minimax(board, depth - 1, alpha, beta, true, next_candidate_map);
        auto eval = result.second;
        if (eval < min_eval) {
          min_eval = eval;
          best_move = {i, j};
        }
        // Undo the move
        board.SetCell(i, j, Piece::kEmpty);

#if ALPHA_BETA_PRUNING == 1
        beta = std::min(beta, eval);
        if (beta <= alpha) {
          break;  // alpha cut-off
        }
#endif
      }
    }
    return {best_move, min_eval};
  }
}
}  // namespace gomoku