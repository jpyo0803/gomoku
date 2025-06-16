import { Controller, Post, Body, Patch, Param, ParseIntPipe, Get } from '@nestjs/common';
import { DBService } from './db.service';

@Controller('DB')
export class DBController {
  constructor(private readonly DBService: DBService) {}

  @Post()
  async createUser(
    @Body('username') username: string,
    @Body('password') password: string,
  ) {
    const user = await this.DBService.create(username, password);
    return { message: 'User created', user };
  }

  @Get('by-username/:username')
  async getUserByUsername(@Param('username') username: string) {
    const user = await this.DBService.findByUsername(username);
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
    await this.DBService.updateResult(id, result);
    return { message: 'Result updated' };
  }
}