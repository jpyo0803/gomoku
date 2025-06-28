import { Module } from '@nestjs/common';
import { AuthService } from './auth.service';
import { AuthController } from './auth.controller';
import { SqlModule } from '../sql/sql.module';
import { JwtModule } from '../jwt/jwt.module'; // 바뀐 경로

@Module({
  imports: [
    SqlModule,
    JwtModule, // 이제 JwtModule만 import (strategy와 guard가 포함됨)
  ],
  controllers: [AuthController],
  providers: [AuthService],
  exports: [AuthService], // AuthService만 내보내면 됨
})
export class AuthModule {}