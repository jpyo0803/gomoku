import { Module } from '@nestjs/common';
import { JwtModule as NestJwtModule } from '@nestjs/jwt';
import { PassportModule } from '@nestjs/passport';
import { JwtStrategy } from './jwt.strategy';
import { JwtAuthGuard } from './jwt-auth-guard';
import { WsJwtAuthGuard } from './ws-jwt-auth-guard';

@Module({
  imports: [
    PassportModule,
    NestJwtModule.register({
      secret: 'your_jwt_secret_key',
      signOptions: { expiresIn: '1d' },
    }),
  ],
  providers: [JwtStrategy, JwtAuthGuard, WsJwtAuthGuard],
  exports: [JwtAuthGuard, WsJwtAuthGuard, NestJwtModule], // 다른 모듈에서 쉽게 쓸 수 있도록
})
export class JwtModule {}