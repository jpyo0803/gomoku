import { GameInstance } from "../game/game-instance"; // Assuming you have a GameInstance class defined somewhere
import { Lock } from 'redlock';

/*
   게임 인스턴스와 락 정보를 함께 묶는 '대여' 객체
   이 객체를 소유한 클라이언트만이 게임 상태를 변경 가능
*/
export interface GameLease {
    gameInstance: GameInstance;
    lock: Lock;
}

export interface NosqlInterface {
    popDataFromQueue(queueKey: string): Promise<string | null>;
    pushDataToQueue(queueKey: string, data: string): Promise<void>;

    registerGameInstance(gameId: string, gameInstance: any): Promise<void>;
    deleteGameInstance(gameId: string): Promise<void>;

    checkOutGameInstance(playerId: string): Promise<GameLease | null>;
    checkInGameInstance(playerId: string, lease: GameLease): Promise<void>;
}