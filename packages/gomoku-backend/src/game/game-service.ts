import { Injectable } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';
import { GameInstance } from './game-instance';
import { assert } from 'console';

@Injectable()
export class GameService {
    private sessions: Map<string, GameInstance> = new Map(); // key is gameId, value is GameInstance

    createGame(blackPlayerId: string, whitePlayerId: string, size: number = 15): string {
        assert(size === 15, 'GameService only supports size 15');
        const gameId = uuidv4();
        this.sessions.set(gameId, new GameInstance(blackPlayerId, whitePlayerId, size));
        return gameId;
    }

    play(gameId: string, playerId: string, x: number, y: number): { 
        result: 'ok' | 'invalid' | 'unauthorized' | 'win';
        board: string;
        currentPlayer: number;
    } {
        const gameInstance = this.sessions.get(gameId);
        if (!gameInstance) {
            throw new Error('Game not found');
        }
        
        const result = gameInstance.play(x, y, playerId);
        return {
            result,
            board: gameInstance.getBoard(),
            currentPlayer: gameInstance.getCurrentPlayer(),
        };
    }

    getBoard(gameId: string): string {
        const gameInstance = this.sessions.get(gameId);
        if (!gameInstance) {
            throw new Error('Game not found');
        }
        return gameInstance.getBoard();
    }

    getCurrentPlayer(gameId: string): number {
        const gameInstance = this.sessions.get(gameId);
        if (!gameInstance) {
            throw new Error('Game not found');
        }
        return gameInstance.getCurrentPlayer();
    }
}