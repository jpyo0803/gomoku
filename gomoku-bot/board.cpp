#include "board.h"
#include <algorithm>
#include <array>
#include <cassert>
#include <utility>
#include <cmath>

namespace gomoku {

Board::Board() : board_(kBoardSize, std::vector<Piece>(kBoardSize, Piece::kEmpty)) {}

Piece Board::GetCell(int x, int y) const {
  assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  return board_.at(x).at(y);
}

void Board::SetCell(int x, int y, Piece piece) {
  assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  board_.at(x).at(y) = piece;
}

int Board::Play(int x, int y, Piece piece) {
  if (!IsValidMove(x, y)) {
    return -1;  // Invalid move
  }

  board_.at(x).at(y) = piece;
  if (piece == Piece::kBlack) {
    black_piece_pos_.emplace_back(x, y);
  } else {
    white_piece_pos_.emplace_back(x, y);
  }

  if (IsWin(x, y, piece)) {
    return 1;  // Win
  }

  return 0;  // Continue playing
}

bool Board::IsValidMove(int x, int y) const {
  if (x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize) {
    return false;
  }
  return board_.at(x).at(y) == Piece::kEmpty;
}

bool Board::IsWin(int x, int y, Piece piece) const {
  constexpr std::array<std::pair<int, int>, 4> kDirections = {
      std::make_pair(1, 0), std::make_pair(0, 1), std::make_pair(1, 1), std::make_pair(1, -1)};

  for (auto [dx, dy] : kDirections) {
    int count = 1;
    for (int dir = -1; dir <= 1; dir += 2) {
      int nx = x + dx * dir;
      int ny = y + dy * dir;

      while (nx >= 0 && nx < kBoardSize && ny >= 0 && ny < kBoardSize &&
             board_.at(nx).at(ny) == piece) {
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

bool Board::OutOfRange(int x, int y) const {
  return x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize;
}

int Board::GetSize() const {
  return kBoardSize;
}

class SlidingWindow5 {
  constexpr static int kWidth = 5;
 public:
  SlidingWindow5(Piece our_piece) :
      our_piece_(our_piece), our_state_(0), enemy_state_(0), fill_state_(0) {
    if (is_score_table_initialized == false) {
      for (int i = 0; i < (1 << kWidth); ++i) {
        double score = 0.0;
        double popcount = static_cast<double>(__builtin_popcount(i));
        double base = std::pow(1e3, popcount); // 아군이 많이 놓여있을 수록 극적으로 높은 점수 
        for (int j = 0; j < kWidth; ++j) {
          if ((i >> j) & 0b1) {
            int mid = kWidth / 2;
            int factor = mid + 1 - abs(mid - j);
            score += base * factor; // 중앙에 몰려있을 수록 높은 점수
          }
        }
        score_table[i] = score;
      }
      is_score_table_initialized = true;
    }

    mask_ = (1 << kWidth) - 1;
  }

  void Update(Piece curr_piece) {
    if (curr_piece == Piece::kEmpty) {
      our_state_ <<= 1;
      enemy_state_ <<= 1;
      fill_state_ <<= 1;
    } else if (curr_piece == our_piece_) {
      our_state_ = (our_state_ << 1) | 0b1;
      enemy_state_ <<= 1;
      fill_state_ = (fill_state_ << 1) | 0b1;
    } else {
      // enemy
      our_state_ <<= 1;
      enemy_state_ = (enemy_state_ << 1) | 0b1;
      fill_state_ = (fill_state_ << 1) | 0b1;
    }
  }

  double GetScore() const {
    if (is_score_table_initialized == false) {
      throw std::runtime_error("Score table is not initialized.");
    }
    double our_score = score_table[our_state()];
    double enemy_score = score_table[enemy_state()];
    return our_score - enemy_score;
  }

  int our_state() const {
    return our_state_ & mask_;
  }

  int GetOurCount() const {
    return __builtin_popcount(our_state());
  }

  int enemy_state() const {
    return enemy_state_ & mask_;
  }

  int GetEnemyCount() const {
    return __builtin_popcount(enemy_state());
  }

  int fill_state() const {
    return fill_state_ & mask_;
  }

  int GetFillCount() const {
    return __builtin_popcount(fill_state());
  }

  std::string GetOurStateBitString() const {
    return GetStateBitString(our_state_);
  }

  std::string GetEnemyStateBitString() const {
    return GetStateBitString(enemy_state_);
  }

  std::string GetFillStateBitString() const {
    return GetStateBitString(fill_state_);
  }

  std::string GetStateBitString(int state) const {
    std::string ret;
    for (int i = kWidth - 1; i >= 0; --i) {
      bool bit = (state >> i) & 0b1;
      if (bit) ret.push_back('1');
      else ret.push_back('0'); 
    }
    return ret;
  }
  
 private:
  int our_state_, enemy_state_, fill_state_;
  int mask_;
  
  Piece our_piece_;
 
  // Static member
 private:
  static bool is_score_table_initialized;
  static std::array<double, (1 << kWidth)> score_table;
};

bool SlidingWindow5::is_score_table_initialized = false;
std::array<double, (1 << SlidingWindow5::kWidth)> SlidingWindow5::score_table = {0.0};

double EvaluateBoardHorizontal(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;
  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  for (int i = 0; i < kBoardSize; ++i) {
    SlidingWindow5 sw(piece);
    for (int j = 0; j < kBoardSize; ++j) {
      auto curr_piece = board.GetCell(i, j);
      sw.Update(curr_piece);
      score += sw.GetScore();
    }
  }

  return score;
}

double EvaluateBoardVertical(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;
  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  for (int j = 0; j < kBoardSize; ++j) {
    SlidingWindow5 sw(piece);
    for (int i = 0; i < kBoardSize; ++i) {
      auto curr_piece = board.GetCell(i, j);
      sw.Update(curr_piece);
      score += sw.GetScore();
    }
  }

  return score;
}

double EvaluateBoardDiagonal1(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;
  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  // Diagonal from top-left to bottom-right
  for (int i = 0; i < kBoardSize; ++i) { // 0 ~ 28
    SlidingWindow5 sw(piece);
    int j_len = (i == 0) ? kBoardSize : 1;
    for (int j = 0; j < j_len; ++j) {
      int x = i;
      int y = j;

      sw.Update(board.GetCell(x, y));
      score += sw.GetScore();
      x++;
      y++;
      if (x >= kBoardSize || y >= kBoardSize) {
        break; // Out of bounds
      }
    }
  }

  return score;
}

double EvaluateBoardDiagonal2(const gomoku::Board& board, gomoku::Piece piece) {
  double score = 0.0;
  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  // Diagonal from top-right to bottom-left
  for (int i = 0; i < kBoardSize; ++i) { // 0 ~ 28
    SlidingWindow5 sw(piece);
    int j_len = (i == 0) ? kBoardSize : 1;
    for (int j = 0; j < j_len; ++j) {
      int x = i;
      int y = kBoardSize - 1 - j;

      sw.Update(board.GetCell(x, y));
      score += sw.GetScore();
      x++;
      y--;
      if (x >= kBoardSize || y < 0) {
        break; // Out of bounds
      }
    }
  }

  return score;
}

}  // namespace gomoku