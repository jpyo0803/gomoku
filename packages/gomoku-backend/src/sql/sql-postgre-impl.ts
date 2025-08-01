import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from './user.entity';
import { SqlInterface } from './sql-interface'; 

@Injectable()
export class SqlPostgreImpl implements SqlInterface {
  constructor(
    @InjectRepository(User)
    private SqlRepository: Repository<User>,
  ) {
    console.log('SqlPostgreImpl initialized');
  }

  // 유저 생성
  async createUser(username: string, password: string): Promise<User> {
    const user = this.SqlRepository.create({ username, password }); // password는 실제로는 해싱해서 저장해야 함
    return this.SqlRepository.save(user);
  }

  // 유저 정보 조회 
  async findUserByUsername(username: string): Promise<User | null> {
    return this.SqlRepository.findOneBy({ username });
  }

  // 유저 결과 업데이트
  async updateUserStatsByUsername(username: string, result: 'win' | 'draw' | 'loss'): Promise<void> {
    const qb = this.SqlRepository
      .createQueryBuilder()
      .update(User)
      .set({
        totalGames: () => '"totalGames" + 1',
        wins: result === 'win' ? () => '"wins" + 1' : undefined,
        draws: result === 'draw' ? () => '"draws" + 1' : undefined,
        losses: result === 'loss' ? () => '"losses" + 1' : undefined,
      })
      .where('username = :username', { username });

    const resultInfo = await qb.execute();

    if (resultInfo.affected === 0) {
      throw new Error(`User with username ${username} not found`);
    }
  }

  async updateUserRefreshToken(username: string, refreshToken: string): Promise<void> {
    const user = await this.findUserByUsername(username);
    if (!user) {
      throw new Error(`User with username ${username} not found`);
    }

    user.refreshToken = refreshToken;
    await this.SqlRepository.save(user);
  }
}
