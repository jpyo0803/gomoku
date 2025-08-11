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

  /*
    유저 생성 메서드

    반환값:
    - success: 생성 성공 여부
    - user: 생성된 사용자 객체 (성공 시)
    - error: 오류 메시지 (실패 시)
    - errorType: 오류 유형 (실패 시)
  */
  async createUser(username: string, password: string): Promise<{
    success: boolean;
    message?: string;
    user?: User;
    errorCode?: 'CONFLICT' | 'INTERNAL_ERROR';
  }> {
    try {
      // 바로 사용자 생성 시도
      const user = this.SqlRepository.create({ username, password });
      const savedUser = await this.SqlRepository.save(user);

      // savedUser에는 이미 createdAt, updatedAt이 포함되어 있음
      return {
        success: true,
        message: 'User successfully created',
        user: savedUser,  // savedUser.createdAt 자동 포함
      };
    } catch (error) {
      // PostgreSQL UNIQUE 제약조건 위반 에러 코드: 23505
      if (error.code === '23505' && error.detail?.includes('(username)')) {
        return {
          success: false,
          message: 'Username already exists',
          errorCode: 'CONFLICT'
        };
      }

      // 기타 데이터베이스 에러
      return {
        success: false,
        message: 'Database error occurred during user creation',
        errorCode: 'INTERNAL_ERROR'
      };
    }
  }

  // 유저 정보 조회 
  async findUserByUsername(username: string): Promise<{
    success: boolean;
    message: string;
    user?: User;
    errorCode?: 'USER_NOT_FOUND' | 'INTERNAL_ERROR';
  }> {
    try {
      const user = await this.SqlRepository.findOneBy({ username });
      
      if (user) {
        return {
          success: true,
          message: 'User found successfully',
          user: user
        };
      } else {
        return {
          success: false,
          message: 'User not found',
          errorCode: 'USER_NOT_FOUND'
        };
      }
    } catch (error) {
      return {
        success: false,
        message: 'Database error occurred while finding user',
        errorCode: 'INTERNAL_ERROR'
      };
    }
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
    const result = await this.findUserByUsername(username);
    if (!result.success || !result.user) {
      throw new Error(`User with username ${username} not found`);
    }

    result.user.refreshToken = refreshToken;
    await this.SqlRepository.save(result.user);
  }
}
