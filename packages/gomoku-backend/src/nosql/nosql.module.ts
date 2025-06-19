import { Module } from '@nestjs/common';
import { NosqlRedisImpl } from './nosql-redis-impl';

@Module({
  providers: [
    {
      provide: 'NosqlInterface',
      useClass: NosqlRedisImpl,
    },
  ],
  exports: ['NosqlInterface'],
})
export class NosqlModule {}
