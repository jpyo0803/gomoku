#include "gomoku_bot_strategy.h"
#include <array>
#include <cassert>
#include <iostream>
#include <limits>
#include "bitboard.h"

#define ALPHA_BETA_PRUNING 1

namespace {
int64_t call_count = 0;
}

namespace gomoku {

constexpr double kEarlyWinScore = 3e15;
constexpr double kWinScore = 1e15;  // open 5 or blocked 5

bool OutOfRange(int x, int y) {
  return x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize;
}

bool IsWin(const gomoku::Board& board, int x, int y, gomoku::Piece piece) {
  constexpr std::array<std::pair<int, int>, 4> kDirections = {
      std::make_pair(1, 0), std::make_pair(0, 1), std::make_pair(1, 1), std::make_pair(1, -1)};

  for (auto [dx, dy] : kDirections) {
    int count = 1;
    for (int dir = -1; dir <= 1; dir += 2) {
      int nx = x + dx * dir;
      int ny = y + dy * dir;

      while (!OutOfRange(nx, ny) && board.GetCell(nx, ny) == piece) {
        count++;
        nx += dx * dir;
        ny += dy * dir;
      }
    }
    if (count >= 5) {
      return true;
    }
  }
  return false;
}

double EvaluateBoard(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;

  auto horizontal_result = EvaluateBoardHorizontal(board, piece);
  auto vertical_result = EvaluateBoardVertical(board, piece);
  auto diagonal1_result = EvaluateBoardDiagonal1(board, piece);
  auto diagonal2_result = EvaluateBoardDiagonal2(board, piece);

  score += horizontal_result;
  score += vertical_result;
  score += diagonal1_result;
  score += diagonal2_result;

  return score;
}

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  // Find candidates. Candiates are the empty cells +/- 2 from the black or white pieces.
  // std::vector<std::vector<bool>> candidate_map(kBoardSize, std::vector<bool>(kBoardSize, false));
  assert(max_depth <= 4);

  // std::vector<std::bitset<kBoardSize>> candidate_map(kBoardSize, std::bitset<kBoardSize>(0));
  BitBoard candidate_map;

  for (int i = 0; i < kBoardSize; ++i) {
    for (int j = 0; j < kBoardSize; ++j) {
      if (board.GetCell(i, j) == Piece::kEmpty) {
        continue;
      }

      candidate_map.SetSquare(i, j, 2);  // mark as candidate for next moves
    }
  }

  double alpha = -std::numeric_limits<double>::infinity();
  double beta = std::numeric_limits<double>::infinity();

  auto result = Minimax(board, max_depth, alpha, beta, true, candidate_map);
  std::cout << "Returned score: " << result.second << std::endl;
  std::cout << "Call count: " << call_count << std::endl;
  return result.first;
}

std::pair<std::pair<int, int>, double> MinimaxWithAlphaBetaPruning::Minimax(
    Board board, int depth, double alpha, double beta, bool maximizing_player,
    BitBoard candidate_map) const {
  call_count++;
  if (depth == 0) {
    auto score = EvaluateBoard(board, Piece::kWhite);
    return {{-1, -1}, score};
  }

  if (maximizing_player) {
    double max_eval = -std::numeric_limits<double>::infinity();
    std::pair<int, int> best_move = {-2, -2};

    for (int i = 0; i < kBoardSize; ++i) {
      if (candidate_map.GetRow(i) == 0) {
        continue;  // skip rows with no candidates
      }
      for (int j = 0; j < kBoardSize; ++j) {
        if (candidate_map.Test(i, j) == false) {
          continue;
        }
        if (board.GetCell(i, j) != Piece::kEmpty) {
          continue;  // skip if not empty
        }

        auto next_candidate_map = candidate_map;
        auto next_board = board;

        next_board.SetCell(i, j, Piece::kWhite);

        // check if the move is winning
        if (IsWin(next_board, i, j, Piece::kWhite)) {
          return {{i, j}, std::numeric_limits<double>::max()};  // early win
        }

        next_candidate_map.SetSquare(i, j, 2);  // mark as candidate for next moves

        auto result = Minimax(next_board, depth - 1, alpha, beta, false, next_candidate_map);
        auto eval = result.second;
        if (eval > max_eval) {
          max_eval = eval;
          best_move = {i, j};
        }

#if ALPHA_BETA_PRUNING == 1
        alpha = std::max(alpha, eval);
        if (beta <= alpha) {
          return {best_move, max_eval};  // beta cut-off
        }
#endif
      }
    }
    return {best_move, max_eval};
  } else {
    double min_eval = std::numeric_limits<double>::infinity();
    std::pair<int, int> best_move = {-3, -3};

    for (int i = 0; i < kBoardSize; ++i) {
      if (candidate_map.GetRow(i) == 0) {
        continue;  // skip rows with no candidates
      }
      for (int j = 0; j < kBoardSize; ++j) {
        if (candidate_map.Test(i, j) == false) {
          continue;
        }
        if (board.GetCell(i, j) != Piece::kEmpty) {
          continue;  // skip if not empty
        }

        auto next_candidate_map = candidate_map;
        auto next_board = board;

        next_board.SetCell(i, j, Piece::kBlack);

        if (IsWin(next_board, i, j, Piece::kBlack)) {
          return {{i, j}, -std::numeric_limits<double>::max()};  // early win
        }

        next_candidate_map.SetSquare(i, j, 2);  // mark as candidate for next moves

        auto result = Minimax(next_board, depth - 1, alpha, beta, true, next_candidate_map);
        auto eval = result.second;
        if (eval < min_eval) {
          min_eval = eval;
          best_move = {i, j};
        }

#if ALPHA_BETA_PRUNING == 1
        beta = std::min(beta, eval);
        if (beta <= alpha) {
          return {best_move, min_eval};  // alpha cut-off
        }
#endif
      }
    }
    return {best_move, min_eval};
  }
}
}  // namespace gomoku