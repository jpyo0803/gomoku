import { Module } from '@nestjs/common';
import { AuthService } from './auth.service';
import { AuthController } from './auth.controller';
import { DBModule } from '../db/db.module'; // Adjust the import path as necessary
import { JwtModule } from '@nestjs/jwt';

@Module({
  imports: [
    DBModule,
    JwtModule.register({
      secret: 'your_jwt_secret_key', // 나중에 .env로 빼는 게 좋음
      signOptions: { expiresIn: '1h' },
    }),
  ],
  controllers: [AuthController],
  providers: [AuthService],
})
export class AuthModule {}
