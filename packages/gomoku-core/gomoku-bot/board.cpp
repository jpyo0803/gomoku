#include "board.h"
#include <algorithm>
#include <array>
#include <cassert>
#include <utility>

namespace gomoku {

Board::Board(int size) : size_(size), board_(size, std::vector<Piece>(size, Piece::kEmpty)) {}

void Board::SetCell(int x, int y, Piece piece) {
  assert(x >= 0 && x < size_ && y >= 0 && y < size_);
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
  if (x < 0 || x >= size_ || y < 0 || y >= size_) {
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

      while (nx >= 0 && nx < size_ && ny >= 0 && ny < size_ && board_.at(nx).at(ny) == piece) {
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

int64_t Board::Evaluate(Piece piece) const {
  int64_t score = 0;

  constexpr int64_t kWinScore = 1e9;       // 11111
  constexpr int64_t kOpenFourScore = 1e7;  // 011110, must block hoping for ememy to make a mistake
  constexpr int64_t kBlockedFourScore = 1e6;     // 011112, must block
  constexpr int64_t kOpenThreeScore = 1e5;       // 01110, must block
  constexpr int64_t kBlockedThreeScore = 10000;  // 01112, should block
  constexpr int64_t kOpenTwoScore = 1000;        // 0110, block if no other good move
  constexpr int64_t kBlockedTwoScore = 100;      // 0112, block if no other good move
  constexpr int64_t kOpenOneScore = 10;          // 010, block if no other good move
  constexpr int64_t kBlockedOneScore = 1;        // 012, block if no other good move

  std::vector<std::vector<bool>> visited_horizontal(size_, std::vector<bool>(size_, false));
  std::vector<std::vector<bool>> visited_vertical(size_, std::vector<bool>(size_, false));
  std::vector<std::vector<bool>> visited_diagonal(size_, std::vector<bool>(size_, false));

  for (int x = 0; x < size_; ++x) {
    for (int y = 0; y < size_; ++y) {
      if (board_.at(x).at(y) != piece) continue;

      // check horizontal
      if (!visited_horizontal[x][y]) {
        int count = 1;
        // Only check to the right
        for (int j = y + 1; j < size_ && board_.at(x).at(j) == piece; ++j) {
          count++;
          visited_horizontal[x][j] = true;
        }
        if (count >= 5) {
          score += kWinScore;
        } else if (count == 4) {
          bool left_not_valid = OutOfRange(x, y - 1) || board_.at(x).at(y - 1) != Piece::kEmpty;
          bool right_not_valid =
              OutOfRange(x, y + count) || board_.at(x).at(y + count) != Piece::kEmpty;
          if (left_not_valid || right_not_valid) {
            score += kBlockedFourScore;
          } else {
            score += kOpenFourScore;
          }
        } else if (count == 3) {
          bool left_not_valid = OutOfRange(x, y - 1) || board_.at(x).at(y - 1) != Piece::kEmpty;
          bool right_not_valid =
              OutOfRange(x, y + count) || board_.at(x).at(y + count) != Piece::kEmpty;
          if (left_not_valid || right_not_valid) {
            score += kBlockedThreeScore;
          } else {
            score += kOpenThreeScore;
          }
        } else if (count == 2) {
          bool left_not_valid = OutOfRange(x, y - 1) || board_.at(x).at(y - 1) != Piece::kEmpty;
          bool right_not_valid =
              OutOfRange(x, y + count) || board_.at(x).at(y + count) != Piece::kEmpty;
          if (left_not_valid || right_not_valid) {
            score += kBlockedTwoScore;
          } else {
            score += kOpenTwoScore;
          }
        } else if (count == 1) {
          bool left_not_valid = OutOfRange(x, y - 1) || board_.at(x).at(y - 1) != Piece::kEmpty;
          bool right_not_valid =
              OutOfRange(x, y + count) || board_.at(x).at(y + count) != Piece::kEmpty;
          if (left_not_valid || right_not_valid) {
            score += kBlockedOneScore;
          } else {
            score += kOpenOneScore;
          }
        }
      }

      // check vertical
      if (!visited_vertical[x][y]) {
        int count = 1;
        // Only check down
        for (int j = x + 1; j < size_ && board_.at(j).at(y) == piece; ++j) {
          count++;
          visited_vertical[j][y] = true;
        }
        if (count >= 5) {
          score += kWinScore;
        } else if (count == 4) {
          bool up_not_valid = OutOfRange(x - 1, y) || board_.at(x - 1).at(y) != Piece::kEmpty;
          bool down_not_valid =
              OutOfRange(x + count, y) || board_.at(x + count).at(y) != Piece::kEmpty;
          if (up_not_valid || down_not_valid) {
            score += kBlockedFourScore;
          } else {
            score += kOpenFourScore;
          }
        } else if (count == 3) {
          bool up_not_valid = OutOfRange(x - 1, y) || board_.at(x - 1).at(y) != Piece::kEmpty;
          bool down_not_valid =
              OutOfRange(x + count, y) || board_.at(x + count).at(y) != Piece::kEmpty;
          if (up_not_valid || down_not_valid) {
            score += kBlockedThreeScore;
          } else {
            score += kOpenThreeScore;
          }
        } else if (count == 2) {
          bool up_not_valid = OutOfRange(x - 1, y) || board_.at(x - 1).at(y) != Piece::kEmpty;
          bool down_not_valid =
              OutOfRange(x + count, y) || board_.at(x + count).at(y) != Piece::kEmpty;
          if (up_not_valid || down_not_valid) {
            score += kBlockedTwoScore;
          } else {
            score += kOpenTwoScore;
          }
        } else if (count == 1) {
          bool up_not_valid = OutOfRange(x - 1, y) || board_.at(x - 1).at(y) != Piece::kEmpty;
          bool down_not_valid =
              OutOfRange(x + count, y) || board_.at(x + count).at(y) != Piece::kEmpty;
          if (up_not_valid || down_not_valid) {
            score += kBlockedOneScore;
          } else {
            score += kOpenOneScore;
          }
        }
      }

      // check diagonal
      if (!visited_diagonal[x][y]) {
        int count = 1;
        // Only check down-right
        for (int j = 1;
             j < size_ && x + j < size_ && y + j < size_ && board_.at(x + j).at(y + j) == piece;
             ++j) {
          count++;
          visited_diagonal[x + j][y + j] = true;
        }
        if (count >= 5) {
          score += kWinScore;
        } else if (count == 4) {
          bool up_left_not_valid =
              OutOfRange(x - 1, y - 1) || board_.at(x - 1).at(y - 1) != Piece::kEmpty;
          bool down_right_not_valid = OutOfRange(x + count, y + count) ||
                                      board_.at(x + count).at(y + count) != Piece::kEmpty;
          if (up_left_not_valid || down_right_not_valid) {
            score += kBlockedFourScore;
          } else {
            score += kOpenFourScore;
          }
        } else if (count == 3) {
          bool up_left_not_valid =
              OutOfRange(x - 1, y - 1) || board_.at(x - 1).at(y - 1) != Piece::kEmpty;
          bool down_right_not_valid = OutOfRange(x + count, y + count) ||
                                      board_.at(x + count).at(y + count) != Piece::kEmpty;
          if (up_left_not_valid || down_right_not_valid) {
            score += kBlockedThreeScore;
          } else {
            score += kOpenThreeScore;
          }
        } else if (count == 2) {
          bool up_left_not_valid =
              OutOfRange(x - 1, y - 1) || board_.at(x - 1).at(y - 1) != Piece::kEmpty;
          bool down_right_not_valid = OutOfRange(x + count, y + count) ||
                                      board_.at(x + count).at(y + count) != Piece::kEmpty;
          if (up_left_not_valid || down_right_not_valid) {
            score += kBlockedTwoScore;
          } else {
            score += kOpenTwoScore;
          }
        } else if (count == 1) {
          bool up_left_not_valid =
              OutOfRange(x - 1, y - 1) || board_.at(x - 1).at(y - 1) != Piece::kEmpty;
          bool down_right_not_valid = OutOfRange(x + count, y + count) ||
                                      board_.at(x + count).at(y + count) != Piece::kEmpty;
          if (up_left_not_valid || down_right_not_valid) {
            score += kBlockedOneScore;
          } else {
            score += kOpenOneScore;
          }
        }
      }
    }
  }
  return score;
}

bool Board::OutOfRange(int x, int y) const {
  return x < 0 || x >= size_ || y < 0 || y >= size_;
}

int Board::size() const {
  return size_;
}

}  // namespace gomoku