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

  Piece GetCell(int x, int y) const;

  void SetCell(int x, int y, Piece piece);  // no restriction

  int Play(int x, int y, Piece piece);

  int size() const;

 private:
  bool IsValidMove(int x, int y) const;

  bool IsWin(int x, int y, Piece piece) const;

  bool OutOfRange(int x, int y) const;

 private:
  std::vector<std::vector<Piece>> board_;
  int size_;

  std::vector<std::pair<int, int>> black_piece_pos_;
  std::vector<std::pair<int, int>> white_piece_pos_;
};

}  // namespace gomoku