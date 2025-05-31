#pragma once

#include <utility>
#include "board.h"

namespace gomoku {
class IGomokuBotStrategy {
 public:
  virtual std::pair<int, int> Solve(Board board, int max_depth) const = 0;

  virtual ~IGomokuBotStrategy() = default;
};

}  // namespace gomoku