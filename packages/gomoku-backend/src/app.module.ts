import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';

import { SocketIoGateway } from './game/socketio-gateway';
import { WsGateway } from './game/ws-gateway';
import { GameService } from './game/game-service';

@Module({
  imports: [],
  controllers: [AppController],
  providers: [
    AppService, 
    SocketIoGateway, 
    GameService, 
    WsGateway,
  ],
})
export class AppModule {}
