#include <gtest/gtest.h>
#include "board.h"

TEST(EvaluateBoardHorizontal, Test1) {
  gomoku::Board board;

  auto result = gomoku::EvaluateBoardHorizontal(board, gomoku::Piece::kBlack);
}
