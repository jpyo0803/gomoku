import {
  WebSocketGateway,
  SubscribeMessage,
  WebSocketServer,
  ConnectedSocket,
  MessageBody,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';

@WebSocketGateway({ cors: true })
export class WebsocketGateway {
  @WebSocketServer()
  server: Server;

  @SubscribeMessage('match_request')
  handleMatchRequest(
    @ConnectedSocket() client: Socket,
    @MessageBody() data: { userId: string },
  ) {
    console.log(`Received match_request from userId: ${data.userId}`);
    // 이후 매칭 큐에 넣거나 응답 보내는 로직은 다음 단계에서
  }
}