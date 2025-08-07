import { Injectable, ConflictException, InternalServerErrorException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from './user.entity';
import { SqlInterface } from './sql-interface'; 

@Injectable()
export class SqlPostgreImpl implements SqlInterface {
  constructor(
    @InjectRepository(User)
    private SqlRepository: Repository<User>,
  ) {}

  // 유저 생성
  async createUser(username: string, password: string): Promise<User> {
    try {
      // 바로 사용자 생성 시도
      const user = this.SqlRepository.create({ username, password });
      const savedUser = await this.SqlRepository.save(user);

      return savedUser;
    } catch (error) {
      // PostgreSQL UNIQUE 제약조건 위반 에러 코드: 23505
      if (error.code === '23505' && error.detail?.includes('(username)')) {
        throw new ConflictException('Username already exists');
      }

      // 기타 데이터베이스 에러
      throw new InternalServerErrorException('Database error occurred during user creation');
    }
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
