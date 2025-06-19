import { Module } from '@nestjs/common';
import { SqlService } from './sql.service';
import { SqlController } from './sql.controller';
import { TypeOrmModule } from '@nestjs/typeorm';
import { User } from './user.entity';

@Module({
  imports: [TypeOrmModule.forFeature([User])],
  providers: [SqlService],
  controllers: [SqlController],
  exports: [SqlService],
})
export class SqlModule {}
