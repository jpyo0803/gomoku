#pragma once

#include <array>
#include <cstdint>

namespace {
constexpr int kBoardSize = 15;
}

namespace gomoku {

class BitBoard {
 public:
  BitBoard() {
    Clear();
  }

  void Clear() {
    for (auto& row : bit_board_) {
      row = 0;
    }
  }

  void Set(int x, int y) {
    bit_board_[x] |= (1 << y);
  }

  void Reset(int x, int y) {
    bit_board_[x] &= ~(1 << y);
  }

  bool Test(int x, int y) const {
    return (bit_board_[x] & (1 << y)) != 0;
  }

  uint16_t GetRow(int x) const {
    return bit_board_[x];
  }

  void SetSquare(int x, int y, int d) {
    // x와 y을 중심으로 2d x 2d 정사각형 영역을 set
    int lower_x = std::max(0, x - d);
    int upper_x = std::min(kBoardSize - 1, x + d);
    int lower_y = std::max(0, y - d);
    int upper_y = std::min(kBoardSize - 1, y + d);

    uint16_t mask = (1 << (upper_y - lower_y + 1)) - 1;  // create a mask for the row
    for (int i = lower_x; i <= upper_x; ++i) {
      // set using bitwise OR
      bit_board_[i] |= (mask << lower_y);
    }
  }

 private:
  std::array<uint16_t, kBoardSize> bit_board_;
};

}  // namespace gomoku