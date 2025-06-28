import {
  WebSocketGateway,
  SubscribeMessage,
  ConnectedSocket,
  MessageBody,
  WebSocketServer,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { Injectable, Inject, forwardRef } from '@nestjs/common';
import { GameService } from '../game/game-service';
import { ClientGatewayInterface } from './client-gateway-interface';
import { UseGuards } from '@nestjs/common';
import { WsJwtAuthGuard } from 'src/jwt/ws-jwt-auth-guard';

@WebSocketGateway({ cors: true })
@Injectable()
export class ClientGatewayImpl implements ClientGatewayInterface {
  @WebSocketServer()
  server: Server;

  private playerIdToSocket = new Map<string, Socket>();

  constructor(
    @Inject(forwardRef(() => GameService))
    private readonly gameService: GameService) {
    console.log('ClientGateway initialized');
  }

  @SubscribeMessage('match_request')
  @UseGuards(WsJwtAuthGuard)
  handleMatchRequest(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { wantAiOpponent: boolean },
  ) {
    // playerId는 JWT 토큰에서 가져옴 (username)
    const playerId = socket.data.user.username;
    console.log(`[Log] Receive 'match_request' from \'${playerId}\' (playerId), want AI: ${data.wantAiOpponent}`);
    this.playerIdToSocket.set(playerId, socket);
    this.gameService.handleMatchRequest(playerId, data.wantAiOpponent);
  }

  @SubscribeMessage('place_stone')
  @UseGuards(WsJwtAuthGuard)
  async handlePlaceStone(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { x: number; y: number },
  ) {
    // playerId는 JWT 토큰에서 가져옴 (username)
    const playerId = socket.data.user.username;
    console.log(`[Log] Receive 'place_stone' from \'${playerId}\' (playerId), x: ${data.x}, y: ${data.y}`);
    await this.gameService.handlePlaceStone(playerId, data.x, data.y);
  }

  sendMatchMakingSuccess(playerId: string, opponentId: string, gameId: string, stoneColor: string): void {
    console.log(`[Log] Send 'match_making_success' to \'${playerId}\' (playerId), \'${opponentId}\' (opponentId), gameId: ${gameId}, stone_color: ${stoneColor}`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('match_making_success', { opponent_id: opponentId , game_id: gameId, stone_color: stoneColor });
  }

  sendYourTurn(playerId: string, timeLimit: number): void {
    console.log(`[Log] Send \'your_turn\' to \'${playerId}\' (playerId), time_limit: ${timeLimit}`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('your_turn', { time_limit: timeLimit });
  }

  sendBoardState(playerId: string, board: string, lastMove: {x: number, y: number}): void {
    // console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId), board: ${board}`);
    console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId)`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('board_state', { board_state: board, last_move_x: lastMove.x, last_move_y: lastMove.y });
  }

  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void {
    console.log(`[Log] Send \'place_stone_resp\' to \'${playerId}\' (playerId), result: ${result}`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('place_stone_resp', { result: result });
  }
}