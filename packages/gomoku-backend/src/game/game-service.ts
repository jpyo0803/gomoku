import { Injectable, Inject } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';
import { GameInstance } from './game-instance';
import { GatewayInterface } from './gateway-interface'; // 추가
import { Player } from './player';
import assert from 'assert';
import { SocketIoGateway } from './socketio-gateway';
import { WsGateway } from './ws-gateway';
import { forwardRef } from '@nestjs/common'; // forwardRef 사용을 위해 추가

@Injectable()
export class GameService {
  private sessions: Map<string, GameInstance> = new Map();
  private playerIdToGameId: Map<string, string> = new Map();

  constructor(
    @Inject(forwardRef(() => SocketIoGateway))
    private readonly socketGateway: GatewayInterface,
    @Inject(forwardRef(() => WsGateway))
    private readonly aiGateway: GatewayInterface,
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

  createGame(blackPlayer: Player, whitePlayer: Player): string {
    const gameId = uuidv4();
    const game = new GameInstance(blackPlayer, whitePlayer);
    this.sessions.set(gameId, game);
    this.playerIdToGameId.set(blackPlayer.getId(), gameId);
    this.playerIdToGameId.set(whitePlayer.getId(), gameId);

    console.log(`[Log] Game created, gameId: ${gameId}, blackPlayerId: ${blackPlayer.getId()}, whitePlayerId: ${whitePlayer.getId()}`);

    // Notify black to start turn
    this.socketGateway.sendYourTurn(blackPlayer.getId(), 30); // time limit is not used for now
    return gameId;
  }

  async handlePlaceStone(playerId: string, x: number, y: number) {
    const gameId = this.playerIdToGameId.get(playerId);
    if (!gameId) throw new Error('Game ID not found for player');

    const game = this.sessions.get(gameId);
    if (!game) throw new Error('Game not found');

    const result = game.play(x, y, playerId);

    // 응답
    this.socketGateway.sendPlaceStoneResp(playerId, result); // return resp. to player

    if (result === 'invalid') return;

    const board = game.getBoardString();

    // 보드 상태 업데이트 전송
    this.socketGateway.sendBoardState(playerId, board);

    // AI에 다음 턴 알림
    const aiPlayerId = game.getAIPlayerId();
    this.aiGateway.sendYourTurn(aiPlayerId, board); // AI에게 턴 알림
  }

  async handlePlaceStoneAI(playerId: string, x: number, y: number) {
    const gameId = this.playerIdToGameId.get(playerId);
    if (!gameId) throw new Error('Game ID not found for player');

    const game = this.sessions.get(gameId);
    if (!game) throw new Error('Game not found');

    const result = game.play(x, y, playerId);
    const board = game.getBoardString();

    // 보드 상태 업데이트 전송
    this.socketGateway.sendBoardState(game.getHumanPlayerId(), board);

    // 만약 result가 승리이면 유저에게 Lose 알림
    if (result === 'win') {
        this.socketGateway.sendPlaceStoneResp(game.getHumanPlayerId(), 'lose');
        return;
    }

    // 다음 턴 알림
    const humanPlayerId = game.getHumanPlayerId();
    this.socketGateway.sendYourTurn(humanPlayerId, 30); 
  }
}