#pragma once

#include <bitset>
#include "igomoku_bot_strategy.h"

namespace gomoku {

class MinimaxWithAlphaBetaPruning : public IGomokuBotStrategy {
 public:
  std::pair<int, int> Solve(Board board, int max_depth) const override;

 private:
  std::pair<std::pair<int, int>, double> Minimax(
      Board board, int depth, double alpha, double beta, bool maximizing_player,
      std::vector<std::bitset<kBoardSize>> candidate_map) const;
};

}  // namespace gomoku