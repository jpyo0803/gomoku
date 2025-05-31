#include <gtest/gtest.h>
#include "board.h"
#include <iostream>

using namespace std;
using namespace gomoku;

TEST(EvaluateBoardHorizontal, Test1) {
  Board board;

  board.SetCell(0, 3, Piece::kBlack);
  board.SetCell(0, 4, Piece::kBlack);
  board.SetCell(0, 5, Piece::kWhite);
  board.SetCell(0, 6, Piece::kBlack);
  board.SetCell(0, 7, Piece::kWhite);

  auto result = EvaluateBoardHorizontal(board, Piece::kBlack);
  // EXPECT_EQ(result.open3, 1);
  // EXPECT_EQ(result.open2, 1);

  // cout << result;
}
