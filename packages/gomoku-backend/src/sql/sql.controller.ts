/*
  db.controller.ts는 직접 테스트용으로 존재
  game-service.ts에서 SqlService를 주입받아 사용 
*/

import { Controller, Post, Body, Patch, Param, ParseIntPipe, Get } from '@nestjs/common';
import { SqlService } from './sql.service';

@Controller('Sql')
export class SqlController {
  constructor(private readonly SqlService: SqlService) {}

  @Post()
  async createUser(
    @Body('username') username: string,
    @Body('password') password: string,
  ) {
    const user = await this.SqlService.create(username, password);
    return { message: 'User created', user };
  }

  @Get('by-username/:username')
  async getUserByUsername(@Param('username') username: string) {
    const user = await this.SqlService.findByUsername(username);
    if (!user) {
      return { message: `User '${username}' not found` };
    }
    return user;
  }

  @Patch(':id/result')
  async updateUserResult(
    @Param('id', ParseIntPipe) id: number,
    @Body('result') result: 'win' | 'draw' | 'loss',
  ) {
    await this.SqlService.updateResult(id, result);
    return { message: 'Result updated' };
  }
}