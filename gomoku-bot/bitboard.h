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
  private:
    std::array<uint16_t, kBoardSize> bit_board_;
};

}