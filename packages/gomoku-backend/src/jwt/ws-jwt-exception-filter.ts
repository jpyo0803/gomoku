import {
  Catch,
  ArgumentsHost,
  WsExceptionFilter as INestWsExceptionFilter,
} from '@nestjs/common';
import { WsException } from '@nestjs/websockets';

import { Socket } from 'socket.io';

@Catch(WsException)
export class WsAuthExceptionFilter implements INestWsExceptionFilter {
  catch(exception: WsException, host: ArgumentsHost) {
    const client: Socket = host.switchToWs().getClient<Socket>();
   
    const error = exception.getError();
    const payload: any = typeof error === 'string' ? { result: "error", message: error } : error;

    // 클라이언트에 전송 (이벤트 이름은 자유 설정)
    client.emit('request_result', payload);
  }
}