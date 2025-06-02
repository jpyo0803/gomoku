#include "board.h"
#include <algorithm>
#include <array>
#include <cassert>
#include <utility>

namespace gomoku {

Board::Board() : board_(kBoardSize, std::vector<Piece>(kBoardSize, Piece::kEmpty)),
  row_occupied_{false}, col_occupied_{false} {}

Piece Board::GetCell(int x, int y) const {
  // assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  return board_[x][y];
}

void Board::SetCell(int x, int y, Piece piece) {
  // assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  board_[x][y] = piece;
  if (piece == Piece::kEmpty) {
    row_occupied_[x] = col_occupied_[y] = false;
  } else {
    row_occupied_[x] = col_occupied_[y] = true;
  }
}

bool Board::IsRowEmpty(int row) const {
  return row_occupied_[row] == false;
}

bool Board::IsColEmpty(int col) const {
  return col_occupied_[col] == false;
}

std::vector<int> Board::GetOccupiedRows() const {
  std::vector<int> ret;
  for (int i = 0; i < kBoardSize; ++i) {
    if (row_occupied_[i]) ret.push_back(i);
  }
  return ret;
}

std::vector<int> Board::GetOccupiedCols() const {
  std::vector<int> ret;
  for (int i = 0; i < kBoardSize; ++i) {
    if (col_occupied_[i]) ret.push_back(i);
  }
  return ret;
}

bool Board::IsValidMove(int x, int y) const {
  if (x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize) {
    return false;
  }
  return board_[x][y] == Piece::kEmpty;
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