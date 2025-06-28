import { Module, forwardRef } from '@nestjs/common';
import { ClientGatewayImpl } from './client-gateway-impl';
import { GameModule } from 'src/game/game-module';
import { JwtModule } from 'src/jwt/jwt.module';

@Module({
  imports: [
    forwardRef(() => GameModule),
    JwtModule,
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
