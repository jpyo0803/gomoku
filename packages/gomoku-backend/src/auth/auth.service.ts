import { Injectable, UnauthorizedException, Inject } from '@nestjs/common';
import { SignupDto } from './dto/signup.dto';
import { LoginDto } from './dto/login.dto';
import { SqlInterface } from '../sql/sql-interface';
import * as bcrypt from 'bcrypt';
import { JwtService } from '@nestjs/jwt';


@Injectable()
export class AuthService {
  constructor(
    @Inject('SqlInterface') private readonly sqlService: SqlInterface,
    private readonly jwtService: JwtService,
  ) {}

  async signup(dto: SignupDto) {
    const { username, password } = dto;

    // 형식 오류는 class-validator가 자동으로 잡아줌 (400 Bad Request)

    const hashedPassword = await bcrypt.hash(password, 10);
    const result = await this.sqlService.createUser(username, hashedPassword);

    if (!result.success) {
      // Result 객체 방식으로 에러 정보 반환
      return {
        success: false,
        message: result.message,

        errorCode: result.errorCode
      };
    }

    // 201 Created + 표준 응답
    return {
      success: true,
      message: result.message,

      username: result.user!.username,
      createdAt: result.user!.createdAt, // createdAt은 User 엔티티에 자동 포함됨
    };
  }

  async login(dto: LoginDto) {
    const { username, password } = dto;

    const userResult = await this.sqlService.findUserByUsername(username);
    if (!userResult.success) {
      // 존재하지 않는 사용자이거나 DB 오류
      return {
        success: false,
        message: userResult.message,
        errorCode: userResult.errorCode
      };
    }

    // 이 밑으로 user는 항상 존재함 

    if (!(await bcrypt.compare(password, userResult.user!.password))) {
      // 비밀번호 틀림
      return {
        success: false,
        message: 'Invalid credentials',
        errorCode: 'INVALID_CREDENTIALS'
      };
    }

    const user = userResult.user;
    const payload = { username: user!.username, sub: user!.id };
    const accessToken = this.jwtService.sign(payload, { expiresIn: '1h' });
    const refreshToken = this.jwtService.sign(payload, { expiresIn: '7d' });

    const hashedRefreshToken = await bcrypt.hash(refreshToken, 10);
    await this.sqlService.updateUserRefreshToken(user!.username, hashedRefreshToken);

    // 200 OK + 유저 정보 반환
    return {
      success: true,
      message: 'Login successful',

      username: user!.username,
      accessToken,
      refreshToken,
    };  
  }

  async refresh(refreshToken: string) {
    try {
      // 1. refreshToken 검증 및 payload 추출
      const payload = this.jwtService.verify(refreshToken);

      const userId = payload.sub;
      const userResult = await this.sqlService.findUserByUsername(payload.username);

      if (!userResult.success || !userResult.user || !userResult.user.refreshToken) {
        throw new UnauthorizedException('Invalid refresh token');
      }

      const user = userResult.user;

      // 2. DB에 저장된 해시와 비교
      const isValid = await bcrypt.compare(refreshToken, user.refreshToken);
      if (!isValid) {
        throw new UnauthorizedException('Invalid refresh token');
      }

      // 3. 새로운 access token 발급
      const newPayload = { username: user.username, sub: user.id };
      const accessToken = this.jwtService.sign(newPayload, { expiresIn: '1h' });

      return {
        message: 'Refresh successful',
        accessToken,
      };
    } catch (err) {
      throw new UnauthorizedException('Invalid or expired refresh token');
    }
  }
}
