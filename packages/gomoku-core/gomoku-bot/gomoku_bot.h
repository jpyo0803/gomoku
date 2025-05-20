#pragma once

#include <memory>
#include "igomoku_bot_strategy.h"

namespace gomoku {

class GomokuBot {
 public:
  void set_strategy(std::unique_ptr<IGomokuBotStrategy> strategy);

  std::pair<int, int> Solve(Board board, int max_depth) const;

 private:
  std::unique_ptr<IGomokuBotStrategy> strategy_;
};

}  // namespace gomoku