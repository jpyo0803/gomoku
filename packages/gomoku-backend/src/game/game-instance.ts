import { Gomoku } from 'gomoku-core';
import { Player } from './player'

export class GameInstance {
    private game : Gomoku;
    private blackPlayer: Player;
    private whitePlayer: Player; // ai
    private currentPlayer: Player;

    constructor(blackPlayer: Player, whitePlayer: Player) {
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
        } else if (result === 'win') {
            return 'win'; // Player wins
        }

        // Switch to the next player
        this.currentPlayer = this.currentPlayer.getId() === this.blackPlayer.getId() ? this.whitePlayer: this.blackPlayer;
        return 'ok'; // Move accepted
    }

    getBoardString(): string {
        return this.game.getBoardString();
    }

    getCurrentPlayerId(): string {
        return this.currentPlayer.getId();
    }

    getHumanPlayerId(): string {
        return this.blackPlayer.getId(); // Assuming black is always the human player
    }

    getAIPlayerId(): string {
        return this.whitePlayer.getId(); // Assuming white is always the AI player
    }
}