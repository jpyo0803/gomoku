#include "board.h"
#include <array>
#include <utility>

namespace gomoku {

Board::Board(int size) : size_(size), board_(size, std::vector<Piece>(size, Piece::kEmpty)) {}

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

int Board::size() const {
  return size_;
}

}  // namespace gomoku