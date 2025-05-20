#pragma once

#include <string>
#include <vector>

namespace gomoku {

enum class Piece {
  kEmpty = 0,
  kBlack,
  kWhite,
};

class Board {
 public:
  Board(int size);

  void SetCell(int x, int y, Piece piece) {
    board_.at(x).at(y) = piece;
  }

 private:
  bool IsValidMove(int x, int y) const;

  bool IsWin(int x, int y, Piece piece) const;

  int size() const;

 private:
  std::vector<std::vector<Piece>> board_;
  int size_;
};

}  // namespace gomoku