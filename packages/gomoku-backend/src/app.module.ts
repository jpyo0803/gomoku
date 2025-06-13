import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';

import { SocketIoGateway } from './socketio-gateway';
import { WsGateway } from './ws-gateway';
import { GameService } from './game-service';

import { NoSqlRedisImpl } from './nosql-redis-impl';

@Module({
  imports: [],
  controllers: [AppController],
  providers: [
    AppService, 
    SocketIoGateway, 
    GameService, 
    WsGateway,
    {
      provide: 'NoSqlInterface',
      useClass: NoSqlRedisImpl,
    },
  ],
})
export class AppModule {}
