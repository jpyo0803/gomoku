export enum Piece {
  Empty = 0,
  Black = 1,
  White = 2,
}

export class Board {
  private size: number;
  private board: Piece[][];

  constructor(size: number = 15) {
    this.size = size;

    this.board = [];
    for (let i = 0; i < size; i++) {
      this.board[i] = [];
      for (let j = 0; j < size; j++) {
        this.board[i][j] = Piece.Empty;
      }
    }
  }

  play(x: number, y: number, Piece: Piece): number {
    if (!this.isValidMove(x, y)) {
      return -1; // Invalid move
    }
    this.board[x][y] = Piece; // Place the stone
    if (this.isWin(x, y, Piece)) {
      return 1; // Piece wins
    }
    return 0; // Continue the game
  }
  
  private isValidMove(x: number, y: number): boolean {
    return this.board[x]?.[y] === Piece.Empty;
  }
  
  private isWin(x: number, y: number, Piece: Piece): boolean {
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
        
        while (this.board[nx]?.[ny] === Piece) { // 만약 해당 좌표가 Piece와 같지 않거나 범위를 벗어나면 종료
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

  getBoard(): Piece[][] {
    return this.board;
  }

  getBoardRaw(): number[][] {
    return this.board.map(row => [...row]);
  }

  getBoardString(): string {
    // Convert the board to a single string representation
    return this.board.flat().map(cell => {
      switch (cell) {
        case Piece.Empty: return '.';
        case Piece.Black: return 'B';
        case Piece.White: return 'W';
        default: return ' ';
      }
    }).join('');
  }
}