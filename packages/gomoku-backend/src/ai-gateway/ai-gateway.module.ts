import { Module } from '@nestjs/common';
import { AiGatewayImpl } from './ai-gateway-impl';

@Module({
  providers: [
    {
      provide: 'AiGatewayInterface',
      useClass: AiGatewayImpl,
    },
  ],
  exports: ['AiGatewayInterface'],
})
export class AiGatewayModule {}
