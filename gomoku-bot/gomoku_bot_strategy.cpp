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

constexpr double kWinScore = 1e70;  // open 5 or blocked 5

constexpr std::array<double, 5> kOpenScores = {
    0.0,  // no open
    1e25,  // open 1
    1e35,  // open 2
    1e45,  // open 3
    1e65   // open 4
};

constexpr std::array<double, 5> kHalfOpenScores = {
    0.0,  // no half open
    1e20,  // half open 1
    1e30,  // half open 2
    1e40,  // half open 3
    1e50  // half open 4
};

constexpr double kDoubleOpen3Score = 1e60; // win 2 steps

bool OutOfRange(int x, int y) {
  return y < 0 || y >= kBoardSize || x < 0 || x >= kBoardSize;
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


}  // namespace

double EvaluateBoard(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;

  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  int win = 0;
  std::array<int, 5> open_count_arr = {0, 0, 0, 0, 0};
  std::array<int, 5> half_open_count_arr = {0, 0, 0, 0, 0};

  // Horizontal evaluation
  auto valid_rows = board.GetOccupiedRows();
  for (auto x : valid_rows) {
  // for (int x = 0; x < kBoardSize; ++x) {
    int cnt = 0;
    for (int y = 0; y < kBoardSize; ++y) {
      auto curr_piece = board.GetCell(x, y);
      if (curr_piece == piece) {
        cnt++;
      } else {
        bool is_start_open = !OutOfRange(x, y - cnt - 1) &&
                             board.GetCell(x, y - cnt - 1) == Piece::kEmpty;
        bool is_end_open = !OutOfRange(x, y) && board.GetCell(x, y) == Piece::kEmpty;

        if (cnt >= 5) {
          win++;
          score += kWinScore;
        } else if (cnt > 0) {
          if (is_start_open && is_end_open) {
            open_count_arr[cnt]++;
            score += kOpenScores[cnt];
          } else if (is_start_open || is_end_open) {
            half_open_count_arr[cnt]++;
            score += kHalfOpenScores[cnt];
          }
        }

        // reset count
        cnt = 0;
      }
    }

    if (cnt > 0) {
      bool is_start_open = !OutOfRange(x, kBoardSize - cnt - 1) &&
                           board.GetCell(x, kBoardSize - cnt - 1) == Piece::kEmpty;
      bool is_end_open = false;  // end is always closed in this case
      if (cnt >= 5) {
        win++;
        score += kWinScore;
      } else if (cnt > 0) {
        if (is_start_open && is_end_open) {
          open_count_arr[cnt]++;
          score += kOpenScores[cnt];
        } else if (is_start_open || is_end_open) {
          half_open_count_arr[cnt]++;
          score += kHalfOpenScores[cnt];
        }
      }
    }
  }

  // Vertical evaluation
  auto valid_cols = board.GetOccupiedCols();
  for (auto y : valid_cols) {
    int cnt = 0;
    for (int x = 0; x < kBoardSize; ++x) {
      auto curr_piece = board.GetCell(x, y);
      if (curr_piece == piece) {
        cnt++;
      } else {
        bool is_start_open = !OutOfRange(x - cnt - 1, y) && board.GetCell(x - cnt - 1, y) == Piece::kEmpty;
        bool is_end_open = !OutOfRange(x, y) && board.GetCell(x, y) == Piece::kEmpty;
        if (cnt >= 5) {
          win++;
          score += kWinScore;
        } else if (cnt > 0) {
          if (is_start_open && is_end_open) {
            open_count_arr[cnt]++;
            score += kOpenScores[cnt];
          } else if (is_start_open || is_end_open) {
            half_open_count_arr[cnt]++;
            score += kHalfOpenScores[cnt];
          }
        }

        // reset count
        cnt = 0;
      }
    }

    if (cnt > 0) {
      bool is_start_open = !OutOfRange(kBoardSize - cnt - 1, y) &&
                           board.GetCell(kBoardSize - cnt - 1, y) == Piece::kEmpty;
      bool is_end_open = false;  // end is always closed in this case
      if (cnt >= 5) {
        win++;
        score += kWinScore;
      } else if (cnt > 0) {
        if (is_start_open && is_end_open) {
          open_count_arr[cnt]++;
          score += kOpenScores[cnt];
        } else if (is_start_open || is_end_open) {
          half_open_count_arr[cnt]++;
          score += kHalfOpenScores[cnt];
        }
      }
    }
  }

  // Diagonal evaluation (top-left to bottom-right)
  for (int i = 0; i < kBoardSize; ++i) {
    int cnt = 0;
    int j_len = i == 0 ? kBoardSize : 1;
    for (int j = 0; j < j_len; ++j) {
      int x = i;
      int y = j; 

      if (i >= kBoardSize || j >= kBoardSize) {
        break;  // out of range
      }
      auto curr_piece = board.GetCell(x, y);

      if (curr_piece == piece) {
        cnt++;
      } else {
        bool is_start_open = !OutOfRange(x - cnt - 1, y - cnt - 1) &&
                             board.GetCell(x - cnt - 1, y - cnt - 1) == Piece::kEmpty;
        bool is_end_open = !OutOfRange(x, y) && board.GetCell(x, y) == Piece::kEmpty;
        if (cnt >= 5) {
          win++;
          score += kWinScore;
        } else if (cnt > 0) {
          if (is_start_open && is_end_open) {
            open_count_arr[cnt]++;
            score += kOpenScores[cnt];
          } else if (is_start_open || is_end_open) {
            half_open_count_arr[cnt]++;
            score += kHalfOpenScores[cnt];
          }
        }

        // reset count
        cnt = 0;
      }

      x++;
      y++;
    }

    if (cnt > 0) {
      bool is_start_open = !OutOfRange(kBoardSize - cnt - 1, kBoardSize - cnt - 1) &&
                           board.GetCell(kBoardSize - cnt - 1, kBoardSize - cnt - 1) == Piece::kEmpty;
      bool is_end_open = false;  // end is always closed in this case
      if (cnt >= 5) {
        win++;
      } else if (cnt > 0) {
        if (is_start_open && is_end_open) {
          open_count_arr[cnt]++;
          score += kOpenScores[cnt];
        } else if (is_start_open || is_end_open) {
          half_open_count_arr[cnt]++;
          score += kHalfOpenScores[cnt];
        }
      }
    }
  }

  // Diagonal evaluation (top-right to bottom-left)
  for (int i = 0; i < kBoardSize; ++i) {
    int cnt = 0;
    int j_len = i == 0 ? kBoardSize : 1;
    for (int j = 0; j < j_len; ++j) {
      int x = i;
      int y = kBoardSize - 1 - j;

      if (i >= kBoardSize || y < 0) {
        break;  // out of range
      }
      auto curr_piece = board.GetCell(x, y);

      if (curr_piece == piece) {
        cnt++;
      } else {
        bool is_start_open = !OutOfRange(x - cnt - 1, y + cnt + 1) &&
                             board.GetCell(x - cnt - 1, y + cnt + 1) == Piece::kEmpty;
        bool is_end_open = !OutOfRange(x, y) && board.GetCell(x, y) == Piece::kEmpty;
        if (cnt >= 5) {
          win++;
          score += kWinScore;
        } else if (cnt > 0) {
          if (is_start_open && is_end_open) {
            open_count_arr[cnt]++;
            score += kOpenScores[cnt];
          } else if (is_start_open || is_end_open) {
            half_open_count_arr[cnt]++;
            score += kHalfOpenScores[cnt];
          }
        }

        // reset count
        cnt = 0;
      }

      x++;
      y--;
    }

    if (cnt > 0) {
      bool is_start_open = !OutOfRange(kBoardSize - cnt - 1, cnt + 1) &&
                           board.GetCell(kBoardSize - cnt - 1, cnt + 1) == Piece::kEmpty;
      bool is_end_open = false;  // end is always closed in this case
      if (cnt >= 5) {
        win++;
        score += kWinScore;
      } else if (cnt > 0) {
        if (is_start_open && is_end_open) {
          open_count_arr[cnt]++;
          score += kOpenScores[cnt];
        } else if (is_start_open || is_end_open) {
          half_open_count_arr[cnt]++;
          score += kHalfOpenScores[cnt];
        }
      }
    }
  }
  // Calculate score

  if (open_count_arr[3] >= 2) {
    score += kDoubleOpen3Score;  // double open 3
  }
  if (half_open_count_arr[4] >= 2) {
    score += kOpenScores[4];
  }

  if (half_open_count_arr[4] >= 1 && open_count_arr[3] >= 1) {
    score += kDoubleOpen3Score;
  }

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
          return {{i, j}, kWinScore};
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
          return {{i, j}, -kWinScore};
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