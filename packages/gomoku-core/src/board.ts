
export function createEmptyBoard(size = 15): Board {
  return new Board(size);
}

export enum Player {
  Empty = 0,
  Black = 1,
  White = 2,
}

export class Board {
  private size: number;
  private board: Player[][];

  constructor(size: number = 15) {
    this.size = size;

    this.board = [];
    for (let i = 0; i < size; i++) {
      this.board[i] = [];
      for (let j = 0; j < size; j++) {
        this.board[i][j] = Player.Empty;
      }
    }
  }

  play(x: number, y: number, player: Player): number {
    if (!this.isValidMove(x, y)) {
      return -1; // Invalid move
    }
    this.board[x][y] = player; // Place the stone
    if (this.isWin(x, y, player)) {
      return 1; // Player wins
    }
    return 0; // Continue the game
  }
  
  private isValidMove(x: number, y: number): boolean {
    return this.board[x]?.[y] === Player.Empty;
  }
  
  private isWin(x: number, y: number, player: Player): boolean {
    const directions = [
      [1, 0], [0, 1], [1, 1], [1, -1],
      /*
      OOO
      OSX
      XXX
      */
    ];
    
    for (const [dx, dy] of directions) {
      let count = 1;
      for (let dir = -1; dir <= 1; dir += 2) {
        let nx = x + dx * dir;
        let ny = y + dy * dir;
        
        while (this.board[nx]?.[ny] === player) { // 만약 해당 좌표가 player와 같지 않거나 범위를 벗어나면 종료
          count++;
          nx += dx * dir;
          ny += dy * dir;
        }
      }
      // 특정 방향으로 연속된 돌의 개수가 5개 이상이면 승리
      if (count >= 5) return true;
    }
    return false;
  }

  getSize(): number {
    return this.size;
  }

  getBoard(): Player[][] {
    return this.board;
  }

  getBoardRaw(): number[][] {
    return this.board.map(row => [...row]);
  }
}