import {Board, Piece} from './board';

const GOMOKU_BOARD_SIZE = 15 as const;

export class Gomoku {
    private board: Board;
    private currentPiece: Piece;
    private lastMove: { x: number; y: number } = { x: -1, y: -1 }; // 초기값 설정

    constructor() {
        this.board = new Board(GOMOKU_BOARD_SIZE); // 15x15 강제
        this.currentPiece = Piece.Black; // 시작 플레이어는 Black
    }

    /*
        플레이어가 착수 시도하는 메서드
        유효하지 않은 착수는 'invalid'를 반환
        유효한 착수는 'ok'를 반환
        착수 후 승리 조건을 만족하면 'win'을 반환
    */
    play(x: number, y: number): 'ok' | 'invalid' | 'win' {
        const result = this.board.play(x, y, this.currentPiece);
        if (result === -1) {
            return 'invalid'; // 유효하지 않은 착수
        }
        this.lastMove = { x, y }; // 가장 최근 착수 저최

        if (result === 1) {
            return 'win'; // 방금 착수한 플레이어 승리
        }
        this.currentPiece = this.currentPiece === Piece.Black ? Piece.White : Piece.Black; // 다음 플레이어로 전환
        return 'ok'; // 유효한 착수
    }

    // 가장 최근 착수 좌표를 반환
    getLastMove(): { x: number; y: number } {
        return this.lastMove;
    }

    // 현재 플레이어의 색을 반환
    // 'B'는 Black, 'W'는 White로 표시
    getCurrentPlayer(): string {
        return this.currentPiece === Piece.Black ? 'B' : 'W'; // Return 1 for Black, 2 for White
    }

    // 현재 보드 상태를 1차원 문자열로 반환
    getBoardString(): string {
        return this.board.getBoardString();
    }

    // JSON 형태로부터 Gomoku 객체를 생성
    static fromJSON(json_data: any): Gomoku {
        const gomoku = new Gomoku();
        gomoku.board = Board.fromJSON(json_data.board); // Assuming Board has a fromJSON method
        gomoku.currentPiece = json_data.currentPiece === 1 ? Piece.Black : Piece.White; // Determine current player based on ID
        return gomoku;
    }
}