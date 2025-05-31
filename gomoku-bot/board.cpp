#include "board.h"
#include <algorithm>
#include <array>
#include <cassert>
#include <utility>

namespace gomoku {

Board::Board() : board_(kBoardSize, std::vector<Piece>(kBoardSize, Piece::kEmpty)) {}

Piece Board::GetCell(int x, int y) const {
  assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  return board_.at(x).at(y);
}

void Board::SetCell(int x, int y, Piece piece) {
  assert(x >= 0 && x < kBoardSize && y >= 0 && y < kBoardSize);
  board_.at(x).at(y) = piece;
}

int Board::Play(int x, int y, Piece piece) {
  if (!IsValidMove(x, y)) {
    return -1;  // Invalid move
  }

  board_.at(x).at(y) = piece;
  if (piece == Piece::kBlack) {
    black_piece_pos_.emplace_back(x, y);
  } else {
    white_piece_pos_.emplace_back(x, y);
  }

  if (IsWin(x, y, piece)) {
    return 1;  // Win
  }

  return 0;  // Continue playing
}

bool Board::IsValidMove(int x, int y) const {
  if (x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize) {
    return false;
  }
  return board_.at(x).at(y) == Piece::kEmpty;
}

bool Board::IsWin(int x, int y, Piece piece) const {
  constexpr std::array<std::pair<int, int>, 4> kDirections = {
      std::make_pair(1, 0), std::make_pair(0, 1), std::make_pair(1, 1), std::make_pair(1, -1)};

  for (auto [dx, dy] : kDirections) {
    int count = 1;
    for (int dir = -1; dir <= 1; dir += 2) {
      int nx = x + dx * dir;
      int ny = y + dy * dir;

      while (nx >= 0 && nx < kBoardSize && ny >= 0 && ny < kBoardSize &&
             board_.at(nx).at(ny) == piece) {
        count++;
        nx += dx * dir;
        ny += dy * dir;
      }
    }
    if (count >= 5) {
      return true;
    }
  }
  return false;
}

bool Board::OutOfRange(int x, int y) const {
  return x < 0 || x >= kBoardSize || y < 0 || y >= kBoardSize;
}

BoardAnalysis EvaluateBoardHorizontal(const gomoku::Board& board, gomoku::Piece piece) {
  BoardAnalysis result;
  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  for (int i = 0; i < kBoardSize; ++i) {
    bool is_started = false;
    int cnt[2]{};
    int cnt_idx = 0;
    int empty_cnt = 0;

    bool is_start_open = false;

    
    for (int j = 0; j < kBoardSize; ++j) {
      auto curr_piece = board.GetCell(i, j);
      if (curr_piece == Piece::kEmpty) {
        // 빈칸이 두개 연속으로 나오면 새롭게 시작
        if (is_started) {

        } else {
          // 아직 시작하지 않았는데 현재 cell이 비어있다면 start를 open 처리
          is_start_open = true;
        }
      } else if (curr_piece == piece) {
        if (is_started) {

        } else {
          is_started = true;
        }
      } else {
        // opponent

        if (empty_cnt == 0) {
          // 아군칸 카운팅 후 빈칸이 존재했다면 빈칸 처리함
          // 즉, 현재가 적군칸이고 빈칸이 이전에 없었던 상황을 여기서 처리
          if (is_started) { 
            int curr_cnt = cnt[cnt_idx];
            assert(curr_cnt > 0); // 이미 시작했다면 현재 카운트는 1이상

            if (curr_cnt >= 5) {
              // 5이상이면 승리
              result.win++;
            } else {
              if (is_start_open) {
                // 끝이 닫혀있기에 4이하에서는 시작이 열려있지 않으면 점수에 기여하지 않기에
                // 시작이 열려있을때만 처리
                if (curr_cnt == 4) {
                  result.half_open4++;
                } else if (curr_cnt == 3) {
                  result.half_open3++;
                } else if (curr_cnt == 2) {
                  result.half_open2++;
                } else if (curr_cnt == 1) {
                  result.half_open1++;
                }
              } 
            }
          }
        }

      }
    }
  }

  return result;
}

BoardAnalysis EvaluateBoardVertical(const gomoku::Board& board, gomoku::Piece piece) {
  BoardAnalysis result;

  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  return result;
}

BoardAnalysis EvaluateBoardDiagonal1(const gomoku::Board& board, gomoku::Piece piece) {
  BoardAnalysis result;

  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  return result;
}

BoardAnalysis EvaluateBoardDiagonal2(const gomoku::Board& board, gomoku::Piece piece) {
  BoardAnalysis result;

  const Piece kOpponent = (piece == Piece::kBlack) ? Piece::kWhite : Piece::kBlack;

  return result;
}

}  // namespace gomoku