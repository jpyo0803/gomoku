import { Module, forwardRef } from '@nestjs/common';
import { GameService } from './game-service';
import { NosqlModule } from '../nosql/nosql.module';
import { SqlModule } from '../sql/sql.module';
import { ClientGatewayModule } from '../client-gateway/client-gateway.module';
import { AiGatewayModule } from '../ai-gateway/ai-gateway.module';

@Module({
  imports: [
    forwardRef(() => ClientGatewayModule),
    AiGatewayModule,
    NosqlModule,
    SqlModule,
  ],
  providers: [GameService],
  exports: [GameService],
})
export class GameModule {}
