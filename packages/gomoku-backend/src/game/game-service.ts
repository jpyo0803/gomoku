import { Injectable, Inject } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';
import { GameInstance } from './game-instance';
import { GatewayInterface } from './gateway-interface'; // 추가
import { Player } from './player';
import assert from 'assert';
import { SocketIoGateway } from './socketio-gateway';
import { WsGateway } from './ws-gateway';
import { forwardRef } from '@nestjs/common'; // forwardRef 사용을 위해 추가

import { RedisService } from './redis.service'; // RedisService 추가

@Injectable()
export class GameService {
  constructor(
    @Inject(forwardRef(() => SocketIoGateway))
    private readonly socketGateway: GatewayInterface,
    private readonly aiGateway: WsGateway,
    private readonly redisService: RedisService,
  ) {
    console.log('GameService initialized');
  }

  handleMatchRequest(playerId: string, wantAiOpponent: boolean) {
    // TODO: Matchmaking queue logic, here we just create a game directly for demo
    // In real setup, wait until two players are available
    if (wantAiOpponent) {
        // AI 상대 매칭, Queue 필요없음
      let player: Player = new Player(playerId, false);
      let aiPlayer: Player = new Player(uuidv4(), true); // AI 플레이어 생성

      const gameId = this.createGame(player, aiPlayer);
      return gameId;
    }
    // 일반 플레이어 매칭
    // 아직 구현 안됨
    assert(false);
  }

  async createGame(blackPlayer: Player, whitePlayer: Player): Promise<string> {
    const gameId = uuidv4();
    const game = new GameInstance(blackPlayer, whitePlayer);

    const blackPlayerId = blackPlayer.getId();
    const whitePlayerId = whitePlayer.getId();

    // redis에 game session 및 playerId -> gameId 매핑 저장
    const redis = this.redisService.getClient();
    await redis.set(`game:${gameId}`, JSON.stringify(game));
    await redis.set(`player:${blackPlayerId}`, gameId);
    await redis.set(`player:${whitePlayerId}`, gameId);

    console.log(`[Log] Game created, gameId: ${gameId}, blackPlayerId: ${blackPlayerId}, whitePlayerId: ${whitePlayer.getId()}`);

    this.socketGateway.sendMatchMakingSuccess(blackPlayerId, whitePlayerId, gameId, "black");
    // Notify black to start turn
    this.socketGateway.sendYourTurn(blackPlayerId, 30); // time limit is not used for now
    return gameId;
  }

  async GetGameByGameId(gameId: string): Promise<GameInstance | null> {
    const redis = this.redisService.getClient();
    const game = await redis.get(`game:${gameId}`);
    if (!game) {
      return null; // 게임이 존재하지 않음
    }
    const parsed = JSON.parse(game) as GameInstance;
    return GameInstance.fromJSON(parsed);
  }

  async GetGameIdByPlayerId(playerId: string): Promise<string | null> {
    const redis = this.redisService.getClient();
    const gameId = await redis.get(`player:${playerId}`);
    if (!gameId) {
      return null; // 플레이어가 게임에 참여하지 않음
    }
    return gameId; // 게임 ID 반환
  }


  async handlePlaceStone(playerId: string, x: number, y: number) {
    const redis = this.redisService.getClient();
    const redlock = this.redisService.getRedlock();

    // Game session을 Redis로부터 가져오기
    const gameId = await this.GetGameIdByPlayerId(playerId);
    if (!gameId) {
      throw new Error('Game ID not found for player');
    }

    let lock;
    // 현 구현에서는 Lock을 무한정 보유
    try {
      lock = await redlock.acquire([`lock:game:${gameId}`], 600000); // 10분 동안 Lock 획득
    } catch (err) {
      console.warn(`Lock for game ${gameId} could not be acquired: ${err.message}`);
      return; // Lock 획득 실패 시 아무 작업도 하지 않음
    }

    try {
      const game = await this.GetGameByGameId(gameId);
      if (!game) {
        throw new Error('Game not found');
      }

      const result = game.play(x, y, playerId);
      // 응답
      this.socketGateway.sendPlaceStoneResp(playerId, result); // return resp. to player

      if (result === 'invalid') return;

      const board = game.getBoardString();

      // 보드 상태 업데이트 전송
      this.socketGateway.sendBoardState(playerId, board);
      
      // AI에 다음 턴 알림
      const aiPlayerId = game.getAIPlayerId();
      const { x: x_ai, y: y_ai } = await this.aiGateway.sendYourTurn(board);
      const result_after_ai_turn = game.play(x_ai, y_ai, aiPlayerId);
      const board_after_ai_turn = game.getBoardString();
      
      this.socketGateway.sendBoardState(playerId, board_after_ai_turn);
      // Game session을 Redis에 업데이트, playerId는 굳이 업데이트 필요없음
      await redis.set(`game:${gameId}`, JSON.stringify(game));

      if (result_after_ai_turn === 'win') {
        this.socketGateway.sendPlaceStoneResp(playerId, 'lose'); // AI가 이겼을 때
        return;
      }

      this.socketGateway.sendYourTurn(playerId, 30); // 다음 턴 알림
    } finally {
      // Lock을 해제
      await lock.release().catch((e) => {
        console.warn(`Failed to release lock: ${e.message}`);
      });
    }
  }
}