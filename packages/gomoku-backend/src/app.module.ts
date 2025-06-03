import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';

import { SocketIoGateway } from './socketio-gateway';
import { WsGateway } from './ws-gateway';
import { GameService } from './game-service';

import { RedisService } from './redis.service';

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
