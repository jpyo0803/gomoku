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
import type { PlayerInfo } from './client-gateway-interface';
import { UseGuards } from '@nestjs/common';
import { WsJwtAuthGuard } from 'src/jwt/ws-jwt-auth-guard';
import { UseFilters } from '@nestjs/common';
import { WsAuthExceptionFilter } from 'src/jwt/ws-jwt-exception-filter';

@WebSocketGateway({ cors: true })
@Injectable()
export class ClientGatewayImpl implements ClientGatewayInterface {
  @WebSocketServer()
  server: Server;

  private usernameToSocket = new Map<string, Socket>();

  constructor(
    @Inject(forwardRef(() => GameService))
    private readonly gameService: GameService) {
    console.log('ClientGateway initialized');
  }

  @SubscribeMessage('bind')
  @UseGuards(WsJwtAuthGuard)
  handleBind(@ConnectedSocket() socket: Socket) {
    const playerId = socket.data.user.username;
    // 만약 playerId가 이미 존재한다면 기존 소켓을 제거
    if (this.usernameToSocket.has(playerId)) {
      const existingSocket = this.usernameToSocket.get(playerId);
      if (existingSocket) {
        existingSocket.disconnect();
        console.log(`[Log] Existing socket for playerId ${playerId} disconnected`);
      }
    }
    this.usernameToSocket.set(playerId, socket);
    console.log(`[Log] Client bound: ${playerId}`);
  }

  @SubscribeMessage('match_request') // handle match request from client
  @UseFilters(WsAuthExceptionFilter) // handle exceptions for this event
  @UseGuards(WsJwtAuthGuard) // first check JWT authentication
  async handleMatchRequest(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { wantAiOpponent: boolean },
  ) {
    // playerId는 JWT 토큰에서 가져옴 (username)
    const playerId = socket.data.user.username;
    console.log(`[Log] Receive 'match_request' from \'${playerId}\' (playerId), want AI: ${data.wantAiOpponent}`);
    await this.gameService.handleMatchRequest(playerId, data.wantAiOpponent);
    socket?.emit('request_result', { result: 'ok', message: 'Match request handled successfully' });
  }

  @SubscribeMessage('place_stone')
  @UseFilters(WsAuthExceptionFilter)
  @UseGuards(WsJwtAuthGuard)
  async handlePlaceStone(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { x: number; y: number },
  ) {
    // playerId는 JWT 토큰에서 가져옴 (username)
    const playerId = socket.data.user.username;
    console.log(`[Log] Receive 'place_stone' from \'${playerId}\' (playerId), x: ${data.x}, y: ${data.y}`);
    await this.gameService.handlePlaceStone(playerId, data.x, data.y);
    socket?.emit('request_result', { result: 'ok', message: 'Stone placed successfully' });
  }

  sendMatchMakingSuccess(myInfo: PlayerInfo, opponentInfo: PlayerInfo, gameId: string): void {
    console.log(`[Log] Send 'match_making_success' to \'${myInfo.username}\' (playerId), \'${opponentInfo.username}\' (opponentId), gameId: ${gameId}, stone_color: ${myInfo.stoneColor}`);
    const socket = this.usernameToSocket.get(myInfo.username);

    socket?.emit('match_making_success',
    {
      my_info: myInfo,
      opponent_info: opponentInfo,
      game_id: gameId,
    });
  }

  sendYourTurn(playerId: string, timeLimit: number): void {
    console.log(`[Log] Send \'your_turn\' to \'${playerId}\' (playerId), time_limit: ${timeLimit}`);
    const socket = this.usernameToSocket.get(playerId);
    socket?.emit('your_turn', { time_limit: timeLimit });
  }

  sendBoardState(playerId: string, board: string, lastMove: {x: number, y: number}): void {
    // console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId), board: ${board}`);
    console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId)`);
    const socket = this.usernameToSocket.get(playerId);
    socket?.emit('board_state', { board_state: board, last_move_x: lastMove.x, last_move_y: lastMove.y });
  }

  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void {
    console.log(`[Log] Send \'place_stone_resp\' to \'${playerId}\' (playerId), result: ${result}`);
    const socket = this.usernameToSocket.get(playerId);
    socket?.emit('place_stone_resp', { result: result });
  }
}