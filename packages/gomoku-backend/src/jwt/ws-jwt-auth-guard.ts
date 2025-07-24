import { CanActivate, ExecutionContext, Injectable, UnauthorizedException } from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { WsException } from '@nestjs/websockets';

@Injectable()
export class WsJwtAuthGuard implements CanActivate {
  constructor(private jwtService: JwtService) {}

  canActivate(context: ExecutionContext): boolean {
    const client = context.switchToWs().getClient();
    const token = client.handshake.query?.token as string;

    if (!token) throw new WsException('Missing token');

    try {
      const payload = this.jwtService.verify(token); // 서명 검증
      client.data.user = { userId: payload.sub, username: payload.username };
      console.log(`[Log] WebSocket connection authenticated for user: ${client.data.user.username}`);
      return true;
    } catch (err) {
      if (err.name === 'TokenExpiredError') {
        throw new WsException('Token expired');
      } else if (err.name === 'JsonWebTokenError') {
        throw new WsException('Invalid token');
      } else if (err.name === 'NotBeforeError') {
        throw new WsException('Token not active yet');
      }
      throw new UnauthorizedException('Authentication failed');
    }
  }
}