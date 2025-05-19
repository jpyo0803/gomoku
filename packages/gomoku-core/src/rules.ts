import { Player } from './types';
import type { Board } from './types';

export function isValidMove(board: Board, x: number, y: number): boolean {
  return board[y]?.[x] === Player.Empty;
}

export function checkWin(board: Board, x: number, y: number, player: Player): boolean {
  const directions = [
    [1, 0], [0, 1], [1, 1], [1, -1],
  ];

  for (const [dx, dy] of directions) {
    let count = 1;
    for (let dir = -1; dir <= 1; dir += 2) {
      let nx = x + dx * dir;
      let ny = y + dy * dir;

      while (board[ny]?.[nx] === player) {
        count++;
        nx += dx * dir;
        ny += dy * dir;
      }
    }
    if (count >= 5) return true;
  }
  return false;
}
