import { Module } from '@nestjs/common';
import { SqlPostgreImpl } from './sql-postgre-impl';
import { SqlController } from './sql.controller';
import { TypeOrmModule } from '@nestjs/typeorm';
import { User } from './user.entity';
import { JwtModule } from '@nestjs/jwt';

@Module({
  imports: [
    TypeOrmModule.forFeature([User]),
    JwtModule,
  ],
  controllers: [SqlController], // Testing 용도 
  providers: [
    {
      provide: 'SqlInterface',
      useClass: SqlPostgreImpl,
    },
  ],
  exports: ['SqlInterface'],
})
export class SqlModule {}
