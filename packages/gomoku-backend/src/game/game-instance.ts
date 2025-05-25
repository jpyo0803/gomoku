import { Board, Player } from 'gomoku-core';

export class GameInstance {
    private board: Board;
    private currentPlayer: Player;
    private playerRoles: Record<string, Player>;

    constructor(playerBlackId: string, playerWhiteId: string, size: number = 15) {
        this.board = new Board(size);
        this.currentPlayer = Player.Black;
        this.playerRoles = {
            [playerBlackId]: Player.Black,
            [playerWhiteId]: Player.White,
        };
    }

    play(x: number, y: number, playerId: string): 'ok' | 'invalid' | 'unauthorized' | 'win' {
        const player = this.playerRoles[playerId];
        if (player !== this.currentPlayer) {
            return 'unauthorized'; // Not the player's turn
        }

        const result = this.board.play(x, y, player);
        if (result === -1) {
            return 'invalid'; // Invalid move
        } else if (result === 1) {
            return 'win'; // Player wins
        }
        this.currentPlayer = this.currentPlayer === Player.Black ? Player.White : Player.Black; // Switch player
        return 'ok'; // Move accepted
    }

    getCurrentPlayer(): number {
        return this.currentPlayer === Player.Black ? 1 : 2; // Return 1 for Black, 2 for White
    }

    getBoard(): string {
        return this.board.getBoardString();
    }
}