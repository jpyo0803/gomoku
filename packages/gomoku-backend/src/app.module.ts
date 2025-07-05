import { Module } from '@nestjs/common';
import { AppService } from './app.service';

import { TypeOrmModule } from '@nestjs/typeorm';
import { RedisModule } from '@nestjs-modules/ioredis';

import { SqlModule } from './sql/sql.module';
import { AuthModule } from './auth/auth.module';
import { NosqlModule } from './nosql/nosql.module';
import { ClientGatewayModule } from './client-gateway/client-gateway.module';
import { AiGatewayModule } from './ai-gateway/ai-gateway.module';
import { GameModule } from './game/game-module';

@Module({
  imports: [
    TypeOrmModule.forRoot({
      type: 'postgres',
      host: 'postgres',
      port: 5432,
      username: 'postgres',
      password: 'postgres',
      database: 'gomoku',
      entities: [__dirname + '/**/*.entity{.ts,.js}'],
      synchronize: true, // DB를 매번 새롭게 덮어쓰기 때문에 개발중에만 true 설정 허용
    }),
    RedisModule.forRoot({
      type: 'single',
      url: 'redis://redis:6379',
    }),
    SqlModule,
    AuthModule,
    NosqlModule,
    ClientGatewayModule,
    AiGatewayModule,
    GameModule,
  ],
  providers: [
    AppService,
  ],
})
export class AppModule {}
