import { Injectable, UnauthorizedException } from '@nestjs/common';
import { AuthGuard } from '@nestjs/passport';
import { TokenExpiredError, JsonWebTokenError } from 'jsonwebtoken';

@Injectable()
export class JwtAuthGuard extends AuthGuard('jwt') {
  handleRequest(err, user, info) {
    if (err || !user) {
      if (info instanceof TokenExpiredError) {
        throw new UnauthorizedException('Token expired');
      } else if (info instanceof JsonWebTokenError) {
        throw new UnauthorizedException('Invalid token');
      } else {
        throw new UnauthorizedException('Unauthorized');
      }
    }
    return user;
  }
}