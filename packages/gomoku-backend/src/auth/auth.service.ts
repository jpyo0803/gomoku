import { Injectable, ConflictException, UnauthorizedException, NotFoundException } from '@nestjs/common';
import { SignupDto } from './dto/signup.dto';
import { LoginDto } from './dto/login.dto';
import { DBService } from '../db/db.service';
import * as bcrypt from 'bcrypt';
import { JwtService } from '@nestjs/jwt';

@Injectable()
export class AuthService {
  constructor(
    private readonly dbService: DBService,
    private readonly jwtService: JwtService,
  ) {}

  async signup(dto: SignupDto) {
    const { username, password } = dto;

    // 형식 오류는 class-validator가 자동으로 잡아줌 (400 Bad Request)

    const existing = await this.dbService.findByUsername(username);
    if (existing) {
      // 409 Conflict: 이미 존재하는 사용자
      throw new ConflictException('Username already exists');
    }

    const hashedPassword = await bcrypt.hash(password, 10);
    const user = await this.dbService.create(username, hashedPassword);

    // 201 Created + 유저 정보 반환
    return {
      message: 'User successfully registered',
    };
  }

  async login(dto: LoginDto) {
    const { username, password } = dto;

    const user = await this.dbService.findByUsername(username);
    if (!user || !(await bcrypt.compare(password, user.password))) {
      // 401 Unauthorized: 사용자 없음 or 비밀번호 틀림
      throw new UnauthorizedException('Invalid credentials');
    }

    const payload = { username: user.username, sub: user.id };
    const token = this.jwtService.sign(payload);

    // 200 OK + 유저 정보 반환
    return {
      message: 'Login successful',
      token,
    };
  }
}
