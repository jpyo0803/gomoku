import { Module } from '@nestjs/common';
import { AuthService } from './auth.service';
import { AuthController } from './auth.controller';
import { DBModule } from '../db/db.module'; // Adjust the import path as necessary

@Module({
  imports: [DBModule],
  controllers: [AuthController],
  providers: [AuthService],
})
export class AuthModule {}
