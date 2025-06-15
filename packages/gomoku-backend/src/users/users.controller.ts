import { Controller, Post, Body, Patch, Param, ParseIntPipe, Get } from '@nestjs/common';
import { UsersService } from './users.service';

@Controller('users')
export class UsersController {
  constructor(private readonly usersService: UsersService) {}

  @Post()
  async createUser(
    @Body('username') username: string,
    @Body('password') password: string,
  ) {
    const user = await this.usersService.create(username, password);
    return { message: 'User created', user };
  }

  @Get('by-username/:username')
  async getUserByUsername(@Param('username') username: string) {
    const user = await this.usersService.findByUsername(username);
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
    await this.usersService.updateResult(id, result);
    return { message: 'Result updated' };
  }
}