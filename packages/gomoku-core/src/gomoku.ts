import {Board, Piece} from './board';

const GOMOKU_BOARD_SIZE = 15 as const;

export function createGomokuGame(): Gomoku {
    return new Gomoku();
}

export class Gomoku {
    private board: Board;
    private currentPiece: Piece;

    constructor() {
        this.board = new Board(GOMOKU_BOARD_SIZE); // 15x15 강제
        this.currentPiece = Piece.Black; // 시작 플레이어는 Black
    }

    play(x: number, y: number): 'ok' | 'invalid' | 'win' {
        const result = this.board.play(x, y, this.currentPiece);
        if (result === -1) {
            return 'invalid'; // Invalid move
        } else if (result === 1) {
            return 'win'; // Player wins
        }
        this.currentPiece = this.currentPiece === Piece.Black ? Piece.White : Piece.Black; // Switch player
        return 'ok'; // Move accepted
    }

    getCurrentPlayer(): string {
        return this.currentPiece === Piece.Black ? 'B' : 'W'; // Return 1 for Black, 2 for White
    }

    getBoardString(): string {
        return this.board.getBoardString();
    }

    static fromJSON(json_data: any): Gomoku {
        const gomoku = new Gomoku();
        gomoku.board = Board.fromJSON(json_data.board); // Assuming Board has a fromJSON method
        gomoku.currentPiece = json_data.currentPiece === 1 ? Piece.Black : Piece.White; // Determine current player based on ID
        return gomoku;
    }
}