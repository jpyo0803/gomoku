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

namespace {
  // anonymous namespace for EvaluateBoard
  BitBoard visited_horizontal;
  BitBoard visited_vertical;
  BitBoard visited_diagonal_down_right;
  BitBoard visited_diagonal_down_left;

  constexpr std::array<double, 5> kOpenScores = {
      0.0,  // [0] unused
      1e3,  // [1] open 1
      1e6,  // [2] open 2
      1e9,  // [3] open 3
      1e12  // [4] open 4
  };
  
  constexpr std::array<double, 5> kBlockedScores = {
      0.0,  // [0] unused
      1e1,  // [1] blocked 1
      1e4,  // [2] blocked 2
      1e7,  // [3] blocked 3
      1e10  // [4] blocked 4
  };
}

double EvaluateBoard(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;

  // Reset visited arrays
  visited_horizontal.Clear();
  visited_vertical.Clear();
  visited_diagonal_down_right.Clear();
  visited_diagonal_down_left.Clear();

  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  for (int x = 0; x < kBoardSize; ++x) {
    for (int y = 0; y < kBoardSize; ++y) {
      if (board.GetCell(x, y) != piece) continue;

      // check horizontal
      if (!visited_horizontal.Test(x, y)) {
        int count = 1;
        // Only check to the right
        for (int j = y + 1; j < kBoardSize && board.GetCell(x, j) == piece; ++j) {
          count++;
          visited_horizontal.Set(x, j);
        }
        if (count >= 5) {
          score += kWinScore;
        } else {
          bool left_not_valid = OutOfRange(x, y - 1) || board.GetCell(x, y - 1) == kOpponent;
          bool right_not_valid =
              OutOfRange(x, y + count) || board.GetCell(x, y + count) == kOpponent;
          if (left_not_valid || right_not_valid) {
            score += kBlockedScores[count];
          } else {
            score += kOpenScores[count];
          }
        }
      }

      // check vertical
      if (!visited_vertical.Test(x, y)) {
        int count = 1;
        // Only check down
        for (int j = x + 1; j < kBoardSize && board.GetCell(j, y) == piece; ++j) {
          count++;
          visited_vertical.Set(j, y);
        }
        if (count >= 5) {
          score += kWinScore;
        } else {
          bool up_not_valid = OutOfRange(x - 1, y) || board.GetCell(x - 1, y) == kOpponent;
          bool down_not_valid =
              OutOfRange(x + count, y) || board.GetCell(x + count, y) == kOpponent;
          if (up_not_valid || down_not_valid) {
            score += kBlockedScores[count];
          } else {
            score += kOpenScores[count];
          }
        }
      }

      // check diagonal down-right
      if (!visited_diagonal_down_right.Test(x, y)) {
        int count = 1;
        // Only check down-right
        for (int j = 1; j < kBoardSize && x + j < kBoardSize && y + j < kBoardSize &&
                        board.GetCell(x + j, y + j) == piece;
             ++j) {
          count++;
          visited_diagonal_down_right.Set(x + j, y + j);
        }
        if (count >= 5) {
          score += kWinScore;
        } else {
          bool up_left_not_valid =
              OutOfRange(x - 1, y - 1) || board.GetCell(x - 1, y - 1) == kOpponent;
          bool down_right_not_valid =
              OutOfRange(x + count, y + count) || board.GetCell(x + count, y + count) == kOpponent;
          if (up_left_not_valid || down_right_not_valid) {
            score += kBlockedScores[count];
          } else {
            score += kOpenScores[count];
          }
        }
      }

      // check diagonal down-left
      if (!visited_diagonal_down_left.Test(x, y)) {
        int count = 1;
        // Only check down-left
        for (int j = 1; j < kBoardSize && x + j < kBoardSize && y - j >= 0 &&
                        board.GetCell(x + j, y - j) == piece;
             ++j) {
          count++;
          visited_diagonal_down_left.Set(x + j, y - j);
        }
        if (count >= 5) {
          score += kWinScore;
        } else {
          bool up_right_not_valid =
              OutOfRange(x - 1, y + 1) || board.GetCell(x - 1, y + 1) == kOpponent;
          bool down_left_not_valid =
              OutOfRange(x + count, y - count) || board.GetCell(x + count, y - count) == kOpponent;
          if (up_right_not_valid || down_left_not_valid) {
            score += kBlockedScores[count];
          } else {
            score += kOpenScores[count];
          }
        }
      }
    }
  }
  return score;
}

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  // Find candidates. Candiates are the empty cells +/- 2 from the black or white pieces.
  // std::vector<std::vector<bool>> candidate_map(kBoardSize, std::vector<bool>(kBoardSize, false));
  assert(board.size() == kBoardSize);
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
    auto score = EvaluateBoard(board, Piece::kWhite) - EvaluateBoard(board, Piece::kBlack);
    return {{-1, -1}, score};
  }

  if (maximizing_player) {
    double max_eval = -std::numeric_limits<double>::infinity();
    std::pair<int, int> best_move = {-1, -1};

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
          return {{i, j}, kEarlyWinScore};
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
    std::pair<int, int> best_move = {-1, -1};

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
          return {{i, j}, -kEarlyWinScore};
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