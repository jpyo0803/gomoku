#pragma once

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

  std::vector<std::pair<int, int>> black_piece_pos_;
  std::vector<std::pair<int, int>> white_piece_pos_;
};

struct BoardAnalysis {
  int win = 0;

  int open4 = 0;
  int open3 = 0;
  int open2 = 0;
  int open1 = 0;

  int pieced_open4 = 0; // pieced_open_5 이상도 다 4로 침
  int pieced_open3 = 0;
  int pieced_open2 = 0;
  // pieced_open1 은 존재 x

  int half_open4 = 0;
  int half_open3 = 0;
  int half_open2 = 0;
  int half_open1 = 0;

  int pieced_half_open4 = 0;
  int pieced_half_open3 = 0;
  int pieced_half_open2 = 0;
};

BoardAnalysis EvaluateBoardHorizontal(const gomoku::Board& board, gomoku::Piece piece);
BoardAnalysis EvaluateBoardVertical(const gomoku::Board& board, gomoku::Piece piece);
BoardAnalysis EvaluateBoardDiagonal1(const gomoku::Board& board, gomoku::Piece piece);
BoardAnalysis EvaluateBoardDiagonal2(const gomoku::Board& board, gomoku::Piece piece);

}  // namespace gomoku