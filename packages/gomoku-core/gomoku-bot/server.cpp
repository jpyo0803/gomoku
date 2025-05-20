#include <iostream>
#include <utility>
#include "gomoku_bot.h"
#include "gomoku_bot_strategy.h"

int main(int argc, char** argv) {
  // Create a GomokuBot instance
  gomoku::GomokuBot bot;

  // Set the strategy to MinimaxWithAlphaBetaPruning
  bot.set_strategy(std::make_unique<gomoku::MinimaxWithAlphaBetaPruning>());

  // Create a board with size 15
  gomoku::Board board(15);

  // Solve the board with a maximum depth of 3
  auto move = bot.Solve(board, 3);

  // Print the move
  std::cout << "Best move: (" << move.first << ", " << move.second << ")" << std::endl;

  return 0;
}