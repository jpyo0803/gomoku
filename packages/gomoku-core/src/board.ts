import { Player } from './types';

export function createEmptyBoard(size = 15): Player[][] {
  const board: Player[][] = [];
  for (let i = 0; i < size; i++) {
    board[i] = [];
    for (let j = 0; j < size; j++) {
      board[i][j] = Player.Empty;
    }
  }
  return board;
}

// 테스트용 실행
if (require.main === module) {
  const board = createEmptyBoard(15);
  console.log(board);
}
