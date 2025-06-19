import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';

import { SocketIoGateway } from './socketio-gateway';
import { WsGateway } from './ws-gateway';
import { GameService } from './game-service';

import { TypeOrmModule } from '@nestjs/typeorm';
import { SqlModule } from './sql/sql.module';
import { AuthModule } from './auth/auth.module';
import { NosqlModule } from './nosql/nosql.module';

@Module({
  imports: [
    TypeOrmModule.forRoot({
      type: 'postgres',
      host: 'localhost',
      port: 5432,
      username: 'postgres',
      password: '1234',
      database: 'gomoku',
      entities: [__dirname + '/**/*.entity{.ts,.js}'],
      synchronize: true, // 개발용일 때만 true
    }),
    SqlModule,
    AuthModule,
    NosqlModule,
  ],
  controllers: [AppController],
  providers: [
    AppService, 
    SocketIoGateway, 
    GameService, 
    WsGateway,
  ],
})
export class AppModule {}
