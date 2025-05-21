#pragma once

#include "igomoku_bot_strategy.h"

namespace gomoku {

class MinimaxWithAlphaBetaPruning : public IGomokuBotStrategy {
 public:
  std::pair<int, int> Solve(Board board, int max_depth) const override;

 private:
  std::pair<std::pair<int, int>, int64_t> Minimax(
      Board& board, int depth, int64_t alpha, int64_t beta, bool maximizing_player,
      std::vector<std::vector<int>>& candidate_map) const;
};

}  // namespace gomoku