import { Controller, Get, UseGuards, Req, Inject } from '@nestjs/common';
import { Request } from 'express';
import { JwtAuthGuard } from 'src/jwt/jwt-auth-guard';
import { SqlInterface } from './sql-interface';

@Controller('Sql')
export class SqlController {
  constructor(
    @Inject('SqlInterface') private readonly SqlService: SqlInterface,
  ) {}

  @Get('my-match-history')
  @UseGuards(JwtAuthGuard)
  async getMyMatchHistory(@Req() req: Request) {
    const user = (req as any).user as { userId: number; username: string };
    const username = user.username;

    console.log(`[Log] Get match history for user: ${username}`);

    const userResult = await this.SqlService.findUserByUsername(username);

    if (!userResult.success || !userResult.user) {
      return { message: `No match history found for user '${username}'` };
    }

    return {
      username,
      totalGames: userResult.user!.totalGames,
      wins: userResult.user!.wins,
      draws: userResult.user!.draws,
      losses: userResult.user!.losses,
    };
  }
}