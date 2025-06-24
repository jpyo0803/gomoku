import { Injectable, Inject } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';
import { GameInstance } from './game-instance';
import { ClientGatewayInterface } from '../client-gateway/client-gateway-interface'; // 추가
import { Player } from './player';
import assert from 'assert';
import { AiGatewayInterface } from '../ai-gateway/ai-gateway-interface';

// import { RedisService } from './redis.service'; // RedisService 추가
import { NosqlInterface } from '../nosql/nosql-interface';
import { SqlInterface } from '../sql/sql-interface';

@Injectable()
export class GameService {
  constructor(
    @Inject('ClientGatewayInterface')
    private readonly clientGateway: ClientGatewayInterface,
    @Inject('AiGatewayInterface')
    private readonly aiGateway: AiGatewayInterface,
    @Inject('NosqlInterface')
    private readonly nosqlService: NosqlInterface,
    @Inject('SqlInterface')
    private readonly sqlService: SqlInterface,
  ) {
    console.log('GameService initialized');
  }

  async updateUserResult(playerId: string, result: 'win' | 'draw' | 'loss') {
    // SqlService를 통해 유저 결과 업데이트
    const user = await this.sqlService.findUserByUsername(playerId);
    if (!user) {
      throw new Error(`[Log] User with ID ${playerId} not found`);
    }
    await this.sqlService.updateUserResult(user.id, result);
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
      // 일반 플레이어 상대 매칭
      const opponentId = await this.nosqlService.popDataFromQueue('matchmaking_queue');
      if (!opponentId) {
        // 상대가 없으면 현재 플레이어를 대기열에 추가
        await this.nosqlService.pushDataToQueue('matchmaking_queue', playerId);
        console.log(`[Log] Player ${playerId} added to matchmaking queue`);
      }
      else {
        // 상대가 있으면 게임 생성
        assert(opponentId !== playerId, 'Cannot match with yourself');

        // NOTE(jpyo0803): 간단함을 위해 흑돌과 백돌을 고정
        const blackPlayer = new Player(playerId, false);
        const whitePlayer = new Player(opponentId, false);

        console.log(`[Log] Matchmaking success: ${blackPlayer.getId()} vs ${whitePlayer.getId()}`);
        const gameId = await this.createGame(blackPlayer, whitePlayer);
        return gameId;
      }
    }
  }

  async createGame(blackPlayer: Player, whitePlayer: Player): Promise<string> {
    const gameId = uuidv4();
    const game = new GameInstance(gameId, blackPlayer, whitePlayer);

    const blackPlayerId = blackPlayer.getId();
    const whitePlayerId = whitePlayer.getId();

    // redis에 game session 및 playerId -> gameId 매핑 저장
    this.nosqlService.registerGameInstance(gameId, game);

    console.log(`[Log] Game created, gameId: ${gameId}, blackPlayerId: ${blackPlayerId}, whitePlayerId: ${whitePlayer.getId()}`);

    // AI 플레이어가 아닌 경우 매치 메이킹 결과 통보
    if (!blackPlayer.isAIPlayer()) {
      this.clientGateway.sendMatchMakingSuccess(blackPlayerId, whitePlayerId, gameId, "black");
    }
    if (!whitePlayer.isAIPlayer()) {
      this.clientGateway.sendMatchMakingSuccess(whitePlayerId, blackPlayerId, gameId, "white");
    }

    // Notify black to start turn
    this.clientGateway.sendYourTurn(blackPlayerId, 30); // time limit is not used for now
    return gameId;
  }

  async handlePlaceStone(playerId: string, x: number, y: number) {
    const game =  await this.nosqlService.getGameInstance(playerId);
    if (!game) {
      throw new Error(`No game instance found for player ${playerId}`);
    }

    const opponentPlayer = game.getOpponentPlayer();
    
    const result = game.play(x, y, playerId);
    if (result === 'invalid') {
      this.clientGateway.sendPlaceStoneResp(playerId, 'invalid');
      return;
    } else if (result === 'win') {
      const board = game.getBoardString();

      // 현재 플레이어에게 승리 알림
      this.clientGateway.sendBoardState(playerId, board, { x, y });
      this.clientGateway.sendPlaceStoneResp(playerId, 'win'); 

      // 상대 플레이어에게 패배 알림
      this.clientGateway.sendPlaceStoneResp(opponentPlayer.getId(), 'lose'); // 상대 플레이어에게 패배 알림
      this.clientGateway.sendBoardState(opponentPlayer.getId(), board, { x, y });

      // 게임 결과를 Sql에 업데이트
      await this.updateUserResult(playerId, 'win');

      this.nosqlService.deleteGameInstance(game.getGameId()); // 게임 삭제
      return;
    } else { // result === 'ok'
      const board = game.getBoardString();

      // 현재 플레이어에게 돌을 놓은 후 보드 상태 전어
      this.clientGateway.sendBoardState(playerId, board, { x, y });
      this.clientGateway.sendPlaceStoneResp(playerId, 'ok'); // 플레이어가 돌을 놓았을 때
    }

    this.nosqlService.setGameInstance(playerId, game);

    // ok 상태일 때 상대 플레이어에게 돌을 놓은 후 보드 상태 전어
    if (opponentPlayer.isAIPlayer()) {
      const board = game.getBoardString();
      const { x: x_ai, y: y_ai } = await this.aiGateway.sendYourTurn(board);

      const result_after_ai_turn = game.play(x_ai, y_ai, opponentPlayer.getId());
      const board_after_ai_turn = game.getBoardString();

      if (result_after_ai_turn === 'win') {
        // AI가 이겼을 때
        this.clientGateway.sendBoardState(playerId, board_after_ai_turn, { x: x_ai, y: y_ai });
        this.clientGateway.sendPlaceStoneResp(playerId, 'lose'); // 플레이어가 졌을 때

        // 패배 결과를 Sql에 업데이트
        await this.updateUserResult(playerId, 'loss');

        // Game 삭제
        this.nosqlService.deleteGameInstance(game.getGameId());
      } else if (result_after_ai_turn === 'invalid') {
        // AI는 항상 유효한 돌을 놓는다고 가정
        assert.fail('AI made an invalid move, which should not happen');
      } else {
        // AI가 돌을 놓았을 때
        this.clientGateway.sendBoardState(playerId, board_after_ai_turn, { x: x_ai, y: y_ai });
        this.clientGateway.sendYourTurn(playerId, 30); // 플레이어에게 다음 턴 알림
      }
    } else {
      // 상대 플레이어에게 턴 알림
      const board = game.getBoardString();

      this.clientGateway.sendBoardState(opponentPlayer.getId(), board, { x, y });
      this.clientGateway.sendYourTurn(opponentPlayer.getId(), 30); // time limit is not used for now
    }

    this.nosqlService.setGameInstance(playerId, game);
  }
}