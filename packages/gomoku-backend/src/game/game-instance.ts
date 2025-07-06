import { Gomoku } from 'gomoku-core';
import { Player } from './player'

export class GameInstance {
    private game : Gomoku;

    private gameId: string; // Unique identifier for the game instance

    private blackPlayer: Player;
    private whitePlayer: Player;
    private currentPlayer: Player;

    constructor(gameId: string, blackPlayer: Player, whitePlayer: Player) {
        this.gameId = gameId;
        this.blackPlayer = blackPlayer;
        this.whitePlayer = whitePlayer;
        this.currentPlayer = blackPlayer; // Black starts first
        this.game = new Gomoku();
    }

    play(x: number, y: number, playerId: string): 'ok' | 'invalid' | 'win' {
        if (playerId !== this.currentPlayer.getId()) {
            return 'invalid'; // Not the player's turn
        }
        
        const result = this.game.play(x, y);
        if (result === 'invalid') {
            return 'invalid'; // Invalid move
        } 

        // Switch to the next player
        this.currentPlayer = this.currentPlayer.getId() === this.blackPlayer.getId() ? this.whitePlayer: this.blackPlayer;
        return result; // Move accepted
    }

    getLastMove(): { x: number; y: number } {
        return this.game.getLastMove(); // Assuming Gomoku has a method to get the last move
    }

    getBoardString(): string {
        return this.game.getBoardString();
    }

    getCurrentPlayer(): Player {
        return this.currentPlayer;
    }

    getOpponentPlayer(): Player {
        if (this.currentPlayer.getId() === this.blackPlayer.getId()) {
            return this.whitePlayer; // If current player is black, opponent is white
        }
        return this.blackPlayer; // If current player is white, opponent is black
    }

    getGameId(): string {
        return this.gameId;
    }

    getBlackPlayer(): Player {
        return this.blackPlayer; // Assuming black is always the human player
    }

    getWhitePlayer(): Player {
        return this.whitePlayer; // Assuming white is always the AI player
    }

    static fromJSON(json_data: any): GameInstance {
        const gameId = json_data.gameId;
        const blackPlayer = Player.fromJSON(json_data.blackPlayer);
        const whitePlayer = Player.fromJSON(json_data.whitePlayer);

        const gameInstance = new GameInstance(gameId, blackPlayer, whitePlayer);
        gameInstance.game = Gomoku.fromJSON(json_data.game); // Assuming Gomoku has a fromJSON method
        gameInstance.currentPlayer = json_data.currentPlayer.id === blackPlayer.getId() ? blackPlayer : whitePlayer;
        return gameInstance;
    }
}