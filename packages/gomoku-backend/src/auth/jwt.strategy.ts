import { Injectable } from '@nestjs/common';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';

@Injectable()
export class JwtStrategy extends PassportStrategy(Strategy) {
  constructor() {
    super({
      jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
      secretOrKey: 'your_jwt_secret_key', // 보통 환경변수로 분리함
    });
  }

  async validate(payload: any) {
    // payload = { username: '...', sub: 'userId' }
    return { userId: payload.sub, username: payload.username };
  }
}