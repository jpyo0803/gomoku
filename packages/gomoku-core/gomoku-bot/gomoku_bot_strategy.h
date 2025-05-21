#pragma once

#include "igomoku_bot_strategy.h"

namespace gomoku {

class MinimaxWithAlphaBetaPruning : public IGomokuBotStrategy {
 public:
  std::pair<int, int> Solve(Board board, int max_depth) const override;

 private:
  std::pair<std::pair<int, int>, int64_t> Minimax(Board& board, int depth, int alpha, int beta,
                              bool maximizing_player) const;
};

}  // namespace gomoku