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

  Board(int size, const std::string& board_str);

 private:
  bool IsValidMove(int x, int y) const;

  bool IsWin(int x, int y, Piece piece) const;

 private:
  std::vector<std::vector<Piece>> board_;
  int size_;
};

}  // namespace gomoku