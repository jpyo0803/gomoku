#include "gomoku_bot_strategy.h"
#include <iostream>
#include <limits>
namespace gomoku {

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  int alpha = std::numeric_limits<int>::min();
  int beta = std::numeric_limits<int>::max();
  return Minimax(board, 0, alpha, beta, true);
}

std::pair<int, int> MinimaxWithAlphaBetaPruning::Minimax(Board& board, int depth, int alpha,
                                                         int beta, bool maximizing_player) const {
  std::cout << "Hello from MinimaxWithAlphaBetaPruning::Minimax" << std::endl;
  return {0, 0};  // Placeholder return value
}

}  // namespace gomoku