#include "gomoku_bot.h"
#include <cassert>
#include <chrono>
#include <iostream>
#include <memory>

namespace gomoku {

std::pair<int, int> GomokuBot::Solve(Board board, int max_depth) const {
  assert(strategy_ != nullptr && "Strategy is not set");
  // Measure the time taken to solve
  auto start_time = std::chrono::steady_clock::now();
  auto result = strategy_->Solve(board, max_depth);
  auto end_time = std::chrono::steady_clock::now();
  auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
  std::cout << "Time taken to solve: " << duration.count() << " ms" << std::endl;
  return result;
}

void GomokuBot::set_strategy(std::unique_ptr<IGomokuBotStrategy> strategy) {
  strategy_ = std::move(strategy);
}

}  // namespace gomoku