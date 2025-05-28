import {
  WebSocketGateway,
  SubscribeMessage,
  ConnectedSocket,
  MessageBody,
  WebSocketServer,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { Injectable } from '@nestjs/common';
import { GameService } from './game-service';
import { GatewayInterface } from './gateway-interface';
import { Inject, forwardRef } from '@nestjs/common';


@WebSocketGateway({ cors: true })
@Injectable()
export class SocketIoGateway implements GatewayInterface {
  @WebSocketServer()
  server: Server;

  private playerIdToSocket = new Map<string, Socket>();

  constructor(
    @Inject(forwardRef(() => GameService))
    private readonly gameService: GameService) {
    console.log('SocketIoGateway initialized');
  }

  init(): void {
    // Nothing required — auto-initialized by NestJS WebSocketGateway
  }

  deinit(): void {
    this.playerIdToSocket.clear();
  }

  @SubscribeMessage('match_request')
  handleMatchRequest(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { playerId: string; wantAiOpponent: boolean },
  ) {
    console.log(`[Log] Receive 'match_request' from \'${data.playerId}\' (playerId), want AI: ${data.wantAiOpponent}`);
    this.playerIdToSocket.set(data.playerId, socket);
    this.gameService.handleMatchRequest(data.playerId, data.wantAiOpponent);
  }

  @SubscribeMessage('place_stone')
  async handlePlaceStone(
    @ConnectedSocket() socket: Socket,
    @MessageBody() data: { playerId: string; x: number; y: number },
  ) {
    console.log(`[Log] Receive 'place_stone' from \'${data.playerId}\' (playerId), x: ${data.x}, y: ${data.y}`);
    await this.gameService.handlePlaceStone(data.playerId, data.x, data.y);
  }

  sendYourTurn(playerId: string, timeLimit: number): void {
    console.log(`[Log] Send \'your_turn\' to \'${playerId}\' (playerId), time_limit: ${timeLimit}`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('your_turn', { time_limit: timeLimit });
  }

  sendBoardState(playerId: string, board: string): void {
    // console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId), board: ${board}`);
    console.log(`[Log] Send \'board_state\' to \'${playerId}\' (playerId)`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('board_state', { board_state: board });
  }

  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void {
    console.log(`[Log] Send \'place_stone_resp\' to \'${playerId}\' (playerId), result: ${result}`);
    const socket = this.playerIdToSocket.get(playerId);
    socket?.emit('place_stone_resp', { result });
  }
}