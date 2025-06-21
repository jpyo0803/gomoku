import { Module, forwardRef } from '@nestjs/common';
import { ClientGatewayImpl } from './client-gateway-impl';
import { GameModule } from 'src/game/game-module';
import { AuthModule } from 'src/auth/auth.module';

@Module({
  imports: [
    forwardRef(() => GameModule),
    AuthModule,
  ],
  providers: [
    ClientGatewayImpl,
    {
      provide: 'ClientGatewayInterface',
      useExisting: ClientGatewayImpl,
    },
  ],
  exports: ['ClientGatewayInterface'],
})
export class ClientGatewayModule {}
