#include "board.h"
#include <algorithm>
#include <array>
#include <cassert>
#include <utility>

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

}  // namespace gomoku