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

  void SetCell(int x, int y, Piece piece);  // no restriction

  int Play(int x, int y, Piece piece);

 private:
  bool IsValidMove(int x, int y) const;

  bool IsWin(int x, int y, Piece piece) const;

  int64_t Evaluate(Piece piece) const;

  bool OutOfRange(int x, int y) const;

  int size() const;

 private:
  std::vector<std::vector<Piece>> board_;
  int size_;

  std::vector<std::pair<int, int>> black_piece_pos_;
  std::vector<std::pair<int, int>> white_piece_pos_;
};

}  // namespace gomoku