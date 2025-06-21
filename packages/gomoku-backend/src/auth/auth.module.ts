import { Module } from '@nestjs/common';
import { AuthService } from './auth.service';
import { AuthController } from './auth.controller';
import { SqlModule } from '../sql/sql.module'; // Adjust the import path as necessary
import { JwtModule } from '@nestjs/jwt';
import { JwtStrategy } from './jwt.strategy';
import { WsJwtAuthGuard } from './ws-jwt-auth-guard';

@Module({
  imports: [
    SqlModule,
    JwtModule.register({
      secret: 'your_jwt_secret_key',
      signOptions: { expiresIn: '1h' },
    }),
  ],
  controllers: [AuthController],
  providers: [
    AuthService, 
    JwtStrategy,
    WsJwtAuthGuard,
  ],
  exports: [
    JwtModule,
    AuthService,
    WsJwtAuthGuard,
  ],
})
export class AuthModule {}