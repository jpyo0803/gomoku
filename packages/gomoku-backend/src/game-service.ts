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

  async handleMatchRequest(playerId: string, wantAiOpponent: boolean) {
    // TODO: Matchmaking queue logic, here we just create a game directly for demo
    // In real setup, wait until two players are available
    if (wantAiOpponent) {
        // AI 상대 매칭, Queue 필요없음
      let player: Player = new Player(playerId, false);
      let aiPlayer: Player = new Player(uuidv4(), true); // AI 플레이어 생성

      // TODO(jpyo0803): AI 플레이어도 흑돌이 될 수 있도록 구현
      const gameId = this.createGame(player, aiPlayer);
      return gameId;
    } else {
      // 일반 플레이어 상대 매칭, Redis Queue 가져오기
      const redis = this.redisService.getClient();

      const redlock = this.redisService.getRedlock();

      let lock;
      // 현 구현에서는 Lock을 무한정 보유
      try {
        lock = await redlock.acquire(['lock:matchmaking_queue'], 600000); // 10분 동안 Lock 획득
      } catch (err) {
        console.warn(`Lock for 'matchmaking_queue' could not be acquired: ${err.message}`);
        return; // Lock 획득 실패 시 아무 작업도 하지 않음
      }

      // queue가 비어있으면 플레이어를 대기열에 추가, 만약 대기열에 플레이어가 있으면 바로 게임 생성
      try {
        const queueKey = 'matchmaking_queue';
        const playerData = await redis.lpop(queueKey); // 대기열에서 플레이어 ID 가져오기

        if (!playerData) {
          // 대기열이 비어있으면 현재 플레이어를 대기열에 추가
          await redis.rpush(queueKey, playerId);
          console.log(`[Log] Player '${playerId}' added to matchmaking queue`);
          return; // 대기열에 추가 후 종료
        } else {
          // 대기열에 플레이어가 있으면 게임 생성
          const opponentPlayerId = playerData;

          // Random하게 플레이어를 흑돌과 백돌로 배정

          let blackPlayer, whitePlayer : Player;
          if (Math.random() < 0.5) {
            // 현재 플레이어가 흑돌, 상대 플레이어가 백돌
            blackPlayer = new Player(playerId, false);
            whitePlayer = new Player(opponentPlayerId, false);
          } else {
            // 현재 플레이어가 백돌, 상대 플레이어가 흑돌
            blackPlayer = new Player(opponentPlayerId, false);
            whitePlayer = new Player(playerId, false);
          }

          const gameId = await this.createGame(blackPlayer, whitePlayer);
          return gameId; // 게임 ID 반환
        }
      } finally {
        // Lock을 해제
        await lock.release().catch((e) => {
          console.warn(`Failed to release lock: ${e.message}`);
        });
      }
    }
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

    // AI 플레이어가 아닌 경우 매치 메이킹 결과 통보
    if (!blackPlayer.isAIPlayer()) {
      this.socketGateway.sendMatchMakingSuccess(blackPlayerId, whitePlayerId, gameId, "black");
    }
    if (!whitePlayer.isAIPlayer()) {
      this.socketGateway.sendMatchMakingSuccess(whitePlayerId, blackPlayerId, gameId, "white");
    }

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

      const opponentPlayer = game.getOpponentPlayer();

      const result = game.play(x, y, playerId);
      // 응답
      
      if (result === 'invalid') {
        this.socketGateway.sendPlaceStoneResp(playerId, 'invalid'); // return resp. to player
        return;
      } else if (result === 'win') {
        const board = game.getBoardString();

        // 현재 플레이어에게 승리 알림
        this.socketGateway.sendBoardState(playerId, board, { x, y });
        this.socketGateway.sendPlaceStoneResp(playerId, 'win'); 

        // 상대 플레이어에게 패배 알림
        this.socketGateway.sendPlaceStoneResp(opponentPlayer.getId(), 'lose'); // 상대 플레이어에게 패배 알림
        this.socketGateway.sendBoardState(opponentPlayer.getId(), board, { x, y });
        return;
      } else { // result === 'ok'
        const board = game.getBoardString();

        // 현재 플레이어에게 돌을 놓은 후 보드 상태 전어
        this.socketGateway.sendBoardState(playerId, board, { x, y });
        this.socketGateway.sendPlaceStoneResp(playerId, 'ok'); // 플레이어가 돌을 놓았을 때
      }

      // ok인 경우에만 여기까지 진행
      
      if (opponentPlayer.isAIPlayer()) {
        const board = game.getBoardString();
        const { x: x_ai, y: y_ai } = await this.aiGateway.sendYourTurn(board);

        const result_after_ai_turn = game.play(x_ai, y_ai, opponentPlayer.getId());
        const board_after_ai_turn = game.getBoardString();

        if (result_after_ai_turn === 'win') {
          // AI가 이겼을 때
          this.socketGateway.sendBoardState(playerId, board_after_ai_turn, { x: x_ai, y: y_ai });
          this.socketGateway.sendPlaceStoneResp(playerId, 'lose'); // 플레이어가 졌을 때
        } else if (result_after_ai_turn === 'invalid') {
          // AI는 항상 유효한 돌을 놓는다고 가정
          assert.fail('AI made an invalid move, which should not happen');
        } else {
          // AI가 돌을 놓았을 때
          this.socketGateway.sendBoardState(playerId, board_after_ai_turn, { x: x_ai, y: y_ai });
          this.socketGateway.sendYourTurn(playerId, 30); // 플레이어에게 다음 턴 알림
        }
      } else {
        // 상대 플레이어에게 턴 알림
        const board = game.getBoardString();

        this.socketGateway.sendBoardState(opponentPlayer.getId(), board, { x, y });
        this.socketGateway.sendYourTurn(opponentPlayer.getId(), 30); // time limit is not used for now
      }
      // Game session을 Redis에 업데이트, playerId는 굳이 업데이트 필요없음
      await redis.set(`game:${gameId}`, JSON.stringify(game));
    } finally {
      // Lock을 해제
      await lock.release().catch((e) => {
        console.warn(`Failed to release lock: ${e.message}`);
      });
    }
  }
}