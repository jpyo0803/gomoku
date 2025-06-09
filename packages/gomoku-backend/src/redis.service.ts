import { Injectable } from '@nestjs/common';
import Redis from 'ioredis';
import Redlock from 'redlock';

@Injectable()
export class RedisService {
  private readonly redisClient: Redis;
  private readonly redlock: Redlock;

  constructor() {
    this.redisClient = new Redis({
      host: 'localhost',
      port: 6379,
    });
    this.redlock = new Redlock(
      [this.redisClient],
      {
        retryCount: 60,             // 락 획득 재시도 횟수
        retryDelay: 1000,            // 재시도 간격(ms)
        retryJitter: 100,           // 지연 랜덤화
      }
    );
  }

  getClient(): Redis {
    return this.redisClient;
  }

  getRedlock(): Redlock {
    return this.redlock;
  }
}