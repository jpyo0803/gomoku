import { Injectable, Inject } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';
import { GameInstance } from './game-instance';
import { ClientGatewayInterface } from '../client-gateway/client-gateway-interface';
import type { PlayerInfo } from '../client-gateway/client-gateway-interface';
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

  boardcastUpdateBoardStateByGameInstance(game: GameInstance) {
    const board = game.getBoardString();
    const lastMove = game.getLastMove();

    const blackPlayer = game.getBlackPlayer();
    const whitePlayer = game.getWhitePlayer();

    if (blackPlayer.isAIPlayer() === false) {
      // 흑돌 플레이어가 AI가 아닌 경우에만 보드 상태 전송
      this.clientGateway.sendBoardState(blackPlayer.getId(), board, lastMove);
    }

    if (whitePlayer.isAIPlayer() === false) {
      // 백돌 플레이어가 AI가 아닌 경우에만 보드 상태 전송
      this.clientGateway.sendBoardState(whitePlayer.getId(), board, lastMove);
    }
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
        console.log(`[Log] Player ${playerId} matched with opponent ${opponentId}`);

        // assert(opponentId != playerId, 'Cannot match with yourself');

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

    
    const blackPlayerInfo: PlayerInfo = {
      username: blackPlayerId,
      isAI: blackPlayer.isAIPlayer(),
      stoneColor: 'black',
      totalGames: 0,
      wins: 0,
      draws: 0,
      losses: 0,
    };

    const whitePlayerInfo: PlayerInfo = {
      username: whitePlayerId,
      isAI: whitePlayer.isAIPlayer(),
      stoneColor: 'white',
      totalGames: 0,
      wins: 0,
      draws: 0,
      losses: 0,
    };

    if (!blackPlayer.isAIPlayer()) {
      // Match History 조회 후 Info 업데이트
      const blackPlayerRecord = await this.sqlService.findUserByUsername(blackPlayerId);
      if (blackPlayerRecord) {
        blackPlayerInfo.totalGames = blackPlayerRecord.totalGames;
        blackPlayerInfo.wins = blackPlayerRecord.wins;
        blackPlayerInfo.losses = blackPlayerRecord.losses;
        blackPlayerInfo.draws = blackPlayerRecord.draws;
      }
    }

    if (!whitePlayer.isAIPlayer()) {
      // Match History 조회 후 Info 업데이트
      const whitePlayerRecord = await this.sqlService.findUserByUsername(whitePlayerId);
      if (whitePlayerRecord) {
        whitePlayerInfo.totalGames = whitePlayerRecord.totalGames;
        whitePlayerInfo.wins = whitePlayerRecord.wins;
        whitePlayerInfo.losses = whitePlayerRecord.losses;
        whitePlayerInfo.draws = whitePlayerRecord.draws;
      }
    }

    // AI 플레이어가 아닌 경우 매치 메이킹 결과 통보
    if (!blackPlayer.isAIPlayer()) {
      this.clientGateway.sendMatchMakingSuccess(blackPlayerInfo, whitePlayerInfo, gameId);
    }
    if (!whitePlayer.isAIPlayer()) {
      this.clientGateway.sendMatchMakingSuccess(whitePlayerInfo, blackPlayerInfo, gameId);
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
    const opponentPlayerId = opponentPlayer.getId();

    const result = game.play(x, y, playerId);
    if (result === 'invalid') {
      // 플레이어가 돌을 놓았을 때 유효하지 않은 경우
      this.clientGateway.sendPlaceStoneResp(playerId, 'invalid');
      return;
    }

    // Nosql에 게임 인스턴스 저장
    this.nosqlService.setGameInstance(playerId, game);

    // 업데이트된 보드 상태를 현재 플레이어와 상대 플레이어에게 전송
    await this.boardcastUpdateBoardStateByGameInstance(game);

    if (result === 'win') {
      // 현재 플레이어에게 승리 알림 및 Sql에 업데이트
      this.clientGateway.sendPlaceStoneResp(playerId, 'win'); 
      await this.sqlService.updateUserStatsByUsername(playerId, 'win');

      // 상대 플레이어가 AI 플레이어가 아닌 경우에만 패배 알림 및 Sql에 업데이트
      if (opponentPlayer.isAIPlayer() === false) {
        this.clientGateway.sendPlaceStoneResp(opponentPlayerId, 'lose'); // 상대 플레이어에게 패배 알림
        await this.sqlService.updateUserStatsByUsername(opponentPlayerId, 'loss');
      }
      
      // 게임 인스턴스 삭제
      this.nosqlService.deleteGameInstance(game.getGameId()); // 게임 삭제
      return;
    } else { 
      // 현재 플레이어의 착수 요청이 성공했음을 알림
      this.clientGateway.sendPlaceStoneResp(playerId, 'ok');

      if (opponentPlayer.isAIPlayer() === false) {
        // 상대 플레이어에게 턴 알림
        this.clientGateway.sendYourTurn(opponentPlayerId, 30); // time limit is not used for now
      } else {
        console.log(`[Log] AI opponent's turn, playerId: ${opponentPlayerId}`);

        const { x: x_ai, y: y_ai } = await this.aiGateway.sendYourTurn(game.getBoardString());
        console.log(`[Log] AI placed stone at (${x_ai}, ${y_ai})`);

        const result_after_ai_turn = game.play(x_ai, y_ai, opponentPlayerId);

        this.nosqlService.setGameInstance(playerId, game); // AI가 돌을 놓은 후 게임 인스턴스 저장

        // AI가 돌을 놓은 후 보드 상태 브로드캐스드
        this.boardcastUpdateBoardStateByGameInstance(game);

        if (result_after_ai_turn === 'win') {
          // AI가 이겼을 때
          this.clientGateway.sendPlaceStoneResp(playerId, 'lose'); // 플레이어가 졌을 때
          await this.sqlService.updateUserStatsByUsername(playerId, 'loss');

          // 게임 인스턴스 삭제
          this.nosqlService.deleteGameInstance(game.getGameId());
          return;
        } else if (result_after_ai_turn === 'invalid') {
          // AI는 항상 유효한 돌을 놓는다고 가정
          assert.fail('AI made an invalid move, which should not happen');
        } else {
          // 인간 플레이어에게 턴 알림
          this.clientGateway.sendYourTurn(playerId, 30); // 플레이어에게 다음
        }
      }
    }
  }
}