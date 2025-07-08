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

  /*
    착수 시도하는 메서드
    유효하지 않은 착수는 -1을 반착
    유효한 착수는 0을 반환
    착수 후 승리 조건을 만족하면 1을 반환
  */
  play(x: number, y: number, Piece: Piece): number {
    if (!this.isValidMove(x, y)) {
      return -1; // 유효하지 않은 착효
    }
    this.board[x][y] = Piece; // 착수
    if (this.isWin(x, y, Piece)) {
      return 1; // 현재 착수가 승리 조건을 만족
    }
    return 0; // 유효한 착수
  }
  
  // 유효한 착수인지 검사
  private isValidMove(x: number, y: number): boolean {
    return this.board[x][y] === Piece.Empty;
  }
  
  // 최근 착수가 승리 조건을 만족하는지 검사
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
          // 특정 방향으로 연속된 돌의 개수가 5개면 승리
          if (count == 5) return true;
        }
      }
    }
    return false;
  }

  getSize(): number {
    return this.size;
  }

  // 현재 보드 상태를 깊은 복사하여 반환
  getBoard(): Piece[][] {
    return structuredClone(this.board);
  }

  // 오목 보드를 1차원 문자열로 변환 (통신용 객체)
  getBoardString(): string {
    return this.board.flat().map(cell => {
      switch (cell) {
        case Piece.Empty: return '.';
        case Piece.Black: return 'B';
        case Piece.White: return 'W';
        default: return ' ';
      }
    }).join('');
  }

  // JSON 형태로부터 Board 객체를 생성
  static fromJSON(json_data: any): Board {
    const board = new Board(json_data.size);
    board.board = json_data.board.map((row: number[]) => row.map((cell: number) => cell as Piece));
    return board;
  }
}