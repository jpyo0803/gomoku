#include "gomoku_bot_strategy.h"
#include <iostream>
#include <limits>
namespace gomoku {

std::pair<int, int> MinimaxWithAlphaBetaPruning::Solve(Board board, int max_depth) const {
  // Find candidates. Candiates are the empty cells +/- 2 from the black or white pieces.
  std::vector<std::vector<bool>> candidates = board.GetCandidates();
  
  int board_size = board.size();
  

  int alpha = std::numeric_limits<int>::min();
  int beta = std::numeric_limits<int>::max();
  return Minimax(board, 0, alpha, beta, true).first;
}

std::pair<std::pair<int, int>, int64_t> MinimaxWithAlphaBetaPruning::Minimax(Board& board, int depth, int alpha,
                                                         int beta, bool maximizing_player) const {
  
  
}

}  // namespace gomoku