import { Controller, Post, Body, Get, Query, HttpCode, Res } from '@nestjs/common';
import { Response } from 'express';
import { AuthService } from './auth.service';
import { SignupDto } from './dto/signup.dto';
import { LoginDto } from './dto/login.dto';

@Controller('auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post('signup')
  async signup(@Body() dto: SignupDto, @Res() res: Response) {
    const result = await this.authService.signup(dto);
    
    if (result.success) {
      // 성공 응답 - Unity HttpResponseMessage 구조에 맞춤
      res.status(201)
         .header('Content-Type', 'application/json')
         .header('Server', 'NestJS/1.0')
         .header('Date', new Date().toUTCString())
         .json({
           // Unity에서 Content 속성으로 접근할 실제 데이터
           content: {
             success: true,
             message: result.message,
             data: {
               username: result.username,
               createdAt: result.createdAt?.toISOString() // 명시적으로 ISO 문자열 변환
             },
           },
           // Unity에서 StatusCode 속성으로 접근
           statusCode: 201,
           // Unity에서 ReasonPhrase 속성으로 접근
           reasonPhrase: 'Created',
           // Unity에서 IsSuccessStatusCode 속성으로 접근
           isSuccessStatusCode: true,
           // Unity에서 Version 속성으로 접근
           version: '1.1'
         });
    } else {
      // 실패 응답 (비즈니스 로직 에러)
      res.status(409)
         .header('Content-Type', 'application/json')
         .header('Server', 'NestJS/1.0')
         .header('Date', new Date().toUTCString())
         .json({
           content: {
             success: false,
             message: result.message,
             error: {
               code: result.errorCode,
               details: 'Please choose a different username'
             }
           },
           statusCode: 409,
           reasonPhrase: 'Conflict',
           isSuccessStatusCode: false,
           version: '1.1'
         });
    }
  }

  @Post('login')
  @HttpCode(200) // 200 OK 응답을 명시적으로 설정
  login(@Body() dto: LoginDto) {
    return this.authService.login(dto);
  }

  @Post('refresh')
  @HttpCode(200) // 200 OK 응답을 명시적으로 설정
  async refresh(@Body('refreshToken') refreshToken: string) {
    return this.authService.refresh(refreshToken);
  }
}
