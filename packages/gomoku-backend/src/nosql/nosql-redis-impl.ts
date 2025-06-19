import { NosqlInterface } from "./nosql-interface";
import { GameInstance } from "../game-instance"; // Assuming you have a GameInstance class defined somewhere
import { Injectable } from '@nestjs/common';
import Redis from 'ioredis';
import Redlock from 'redlock';

@Injectable()
export class NosqlRedisImpl implements NosqlInterface {
  private redisClient: Redis;
  private redlock: Redlock;

  constructor() {
    this.redisClient = new Redis({
        host: 'localhost',
        port: 6379,
    });

    this.redlock = new Redlock(
        [this.redisClient],
        {
            retryCount: 1000,  // 재시도 횟수, 현재는 무한정 재시도
            retryDelay: 500,     // 재시도 간격(ms)
            retryJitter: 100,    // 지연 랜덤화
        }
    );
  }

    async popDataFromQueue(queueKey: string): Promise<string | null> {
        let lock;
        let data;
        // Lock을 획득할 때까지 대기
        
        try {
        lock = await this.redlock.acquire([`lock:${queueKey}`], 600000); // 10분 동안 Lock 획득
        console.log(`Lock for '${queueKey}' acquired successfully.`);

        // Lock 획득 후 대기열에서 데이터 가져오기
        data = await this.redisClient.lpop(queueKey);
        } catch (err) {
        console.warn(`Lock for '${queueKey}' could not be acquired: ${err.message}`);
        return null; // Lock 획득 실패 시 null 반환
        } finally {
        if (lock) {
            try {
            await lock.release();
            console.log(`Lock for '${queueKey}' released successfully.`);
            } catch (releaseErr) {
            console.error(`Failed to release lock for '${queueKey}': ${releaseErr.message}`);
            }
        }
        }
        // 대기열에서 가져온 데이터가 없으면 null 반환
        if (!data) {
        console.log(`No data found in queue '${queueKey}'.`);
        return null;
        }
        console.log(`Data retrieved from queue '${queueKey}': ${data}`);
        return data;
    }

    async pushDataToQueue(queueKey: string, data: string): Promise<void> {
        let lock;
        // Lock을 획득할 때까지 대기
        try {
        lock = await this.redlock.acquire([`lock:${queueKey}`], 600000); // 10분 동안 Lock 획득
        console.log(`Lock for '${queueKey}' acquired successfully.`);

        // Lock 획득 후 대기열에 데이터 추가
        await this.redisClient.rpush(queueKey, data);
        console.log(`Data pushed to queue '${queueKey}': ${data}`);
        } catch (err) {
        console.warn(`Lock for '${queueKey}' could not be acquired: ${err.message}`);
        } finally {
        if (lock) {
            try {
            await lock.release();
            console.log(`Lock for '${queueKey}' released successfully.`);
            } catch (releaseErr) {
            console.error(`Failed to release lock for '${queueKey}': ${releaseErr.message}`);
            }
        }
        }
    }

    async registerGameInstance(gameId: string, gameInstance: GameInstance): Promise<void> {
        // 게임 인스턴스를 Redis에 저장
        try {
            const blackPlayerId = gameInstance.getBlackPlayer().getId();
            const whitePlayerId = gameInstance.getWhitePlayer().getId();

            await this.redisClient.set(`game:${gameId}`, JSON.stringify(gameInstance));
            await this.redisClient.set(`player:${blackPlayerId}`, gameId);
            await this.redisClient.set(`player:${whitePlayerId}`, gameId);
            console.log(`Game instance for '${gameId}' registered successfully.`);
        } catch (err) {
            console.error(`Failed to register game instance for '${gameId}': ${err.message}`);
        }
    }

    async getGameInstance(playerId: string): Promise<GameInstance | null> {
        const gameId = await this.redisClient.get(`player:${playerId}`);
        if (!gameId) {
            console.warn(`No game instance found for player '${playerId}'.`);
            return null; // 플레이어가 게임에 참여하고 있지 않음
        }

        let lock;
        try {
            lock = await this.redlock.acquire([`lock:game:${gameId}`], 600000); // 10분 동안 Lock 획득
            console.log(`Lock for game '${gameId}' acquired successfully.`);

            const gameData = await this.redisClient.get(`game:${gameId}`);
            if (!gameData) {
                console.warn(`No game instance found for game '${gameId}'.`);
                return null; // 게임 인스턴스가 없음
            }

            const gameInstance = GameInstance.fromJSON(JSON.parse(gameData));
            return gameInstance;
        }
        catch (err) {
            console.error(`Failed to get game instance for player '${playerId}': ${err.message}`);
            return null; // 에러 발생 시 null 반환
        }
        finally {
            if (lock) {
                try {
                    await lock.release();
                    console.log(`Lock for game '${gameId}' released successfully.`);
                } catch (releaseErr) {
                    console.error(`Failed to release lock for game '${gameId}': ${releaseErr.message}`);
                }
            }
        }
    }

    async setGameInstance(playerId: string, gameInstance: GameInstance): Promise<void> {
        const gameId = await this.redisClient.get(`player:${playerId}`);
        if (!gameId) {
            console.warn(`No game instance found for player '${playerId}'.`);
            return; // 플레이어가 게임에 참여하고 있지 않음
        }

        let lock;
        try {
            lock = await this.redlock.acquire([`lock:game:${gameId}`], 600000); // 10분 동안 Lock 획득
            console.log(`Lock for game '${gameId}' acquired successfully.`);

            await this.redisClient.set(`game:${gameId}`, JSON.stringify(gameInstance));
            console.log(`Game instance for '${gameId}' updated successfully.`);
        } catch (err) {
            console.error(`Failed to set game instance for '${gameId}': ${err.message}`);
        } finally {
            if (lock) {
                try {
                    await lock.release();
                    console.log(`Lock for game '${gameId}' released successfully.`);
                } catch (releaseErr) {
                    console.error(`Failed to release lock for game '${gameId}': ${releaseErr.message}`);
                }
            }
        }
    }
}