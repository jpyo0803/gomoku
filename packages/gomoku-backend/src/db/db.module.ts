import { Module } from '@nestjs/common';
import { DBService } from './db.service';
import { DBController } from './db.controller';
import { TypeOrmModule } from '@nestjs/typeorm';
import { User } from './user.entity';

@Module({
  imports: [TypeOrmModule.forFeature([User])],
  providers: [DBService],
  controllers: [DBController],
  exports: [DBService],
})
export class DBModule {}
