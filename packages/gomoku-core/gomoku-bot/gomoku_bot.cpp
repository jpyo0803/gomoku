#include "gomoku_bot.h"
#include <cassert>
#include <memory>

namespace gomoku {

std::pair<int, int> GomokuBot::Solve(Board board, int max_depth) const {
  assert(strategy_ != nullptr && "Strategy is not set");
  return strategy_->Solve(board, max_depth);
}

void GomokuBot::set_strategy(std::unique_ptr<IGomokuBotStrategy> strategy) {
  strategy_ = std::move(strategy);
}

}  // namespace gomoku