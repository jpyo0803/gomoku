#pragma once

#include <string>
#include <vector>
#include <iostream>

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

  int GetSize() const;

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

  friend std::ostream& operator<<(std::ostream& os, const BoardAnalysis ba) {
    os << "[Board Analysis]" << std::endl;
    os << "win: " << ba.win << std::endl;
    os << "open4: " << ba.open4 << std::endl;
    os << "open3: " << ba.open3 << std::endl;
    os << "open2: " << ba.open2 << std::endl;
    os << "open1: " << ba.open1 << std::endl;
    os << "pieced_open4: " << ba.pieced_open4 << std::endl;
    os << "pieced_open3: " << ba.pieced_open3 << std::endl;
    os << "pieced_open2: " << ba.pieced_open2 << std::endl;
    os << "half_open4: " << ba.half_open4 << std::endl;
    os << "half_open3: " << ba.half_open3 << std::endl;
    os << "half_open2: " << ba.half_open2 << std::endl;
    os << "half_open1: " << ba.half_open1 << std::endl;
    os << "pieced_half_open4: " << ba.pieced_half_open4 << std::endl;
    os << "pieced_half_open3: " << ba.pieced_half_open3 << std::endl;
    os << "pieced_half_open2: " << ba.pieced_half_open2 << std::endl;
    os << "------------------------" << std::endl;
    return os;
  }
};

double EvaluateBoardHorizontal(const gomoku::Board& board, gomoku::Piece piece);
double EvaluateBoardVertical(const gomoku::Board& board, gomoku::Piece piece);
double EvaluateBoardDiagonal1(const gomoku::Board& board, gomoku::Piece piece);
double EvaluateBoardDiagonal2(const gomoku::Board& board, gomoku::Piece piece);

}  // namespace gomoku