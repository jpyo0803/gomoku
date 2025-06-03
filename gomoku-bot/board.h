#pragma once

#include <array>
#include <string>
#include <vector>

namespace gomoku {

constexpr int kBoardSize = 15;

enum class Piece {
  kEmpty = 0,
  kBlack,
  kWhite,
};

class Board {
 public:
  Board();

  explicit Board(const std::string& board_str);

  Piece GetCell(int x, int y) const;

  void SetCell(int x, int y, Piece piece);  // no restriction

  int GetSize() const;

  bool IsRowEmpty(int row) const;

  bool IsColEmpty(int col) const;

  std::vector<int> GetOccupiedRows() const;

  std::vector<int> GetOccupiedCols() const;

 private:
  bool IsValidMove(int x, int y) const;

  bool IsWin(int x, int y, Piece piece) const;

  bool OutOfRange(int x, int y) const;

 private:
  std::vector<std::vector<Piece>> board_;

  std::vector<std::pair<int, int>> black_piece_pos_;
  std::vector<std::pair<int, int>> white_piece_pos_;

  std::array<bool, kBoardSize> row_occupied_;
  std::array<bool, kBoardSize> col_occupied_;
};

}  // namespace gomoku