import { Module } from '@nestjs/common';
import { AppService } from './app.service';

import { TypeOrmModule } from '@nestjs/typeorm';
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
    ClientGatewayModule,
    AiGatewayModule,
    GameModule,
  ],
  providers: [
    AppService,
  ],
})
export class AppModule {}
