import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';

import { SocketIoGateway } from './game/socketio-gateway';
import { WsGateway } from './game/ws-gateway';
import { GameService } from './game/game-service';

import { RedisService } from './game/redis.service';

@Module({
  imports: [],
  controllers: [AppController],
  providers: [
    AppService, 
    SocketIoGateway, 
    GameService, 
    WsGateway,
    RedisService,
  ],
})
export class AppModule {}
