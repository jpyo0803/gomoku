import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from './user.entity';

@Injectable()
export class DBService {
  constructor(
    @InjectRepository(User)
    private DBRepository: Repository<User>,
  ) {}

  // 유저 생성
  async create(username: string, password: string): Promise<User> {
    const user = this.DBRepository.create({ username, password }); // password는 실제로는 해싱해서 저장해야 함
    return this.DBRepository.save(user);
  }

  // 유저 정보 조회 
  async findByUsername(username: string): Promise<User | null> {
    return this.DBRepository.findOneBy({ username });
  }

  async updateResult(userId: number, result: 'win' | 'draw' | 'loss') {
    return this.DBRepository
      .createQueryBuilder()
      .update(User)
      .set({
        totalGames: () => '"totalGames" + 1',
        wins: result === 'win' ? () => '"wins" + 1' : undefined,
        draws: result === 'draw' ? () => '"draws" + 1' : undefined,
        losses: result === 'loss' ? () => '"losses" + 1' : undefined,
      })
      .where("id = :id", { id: userId })
      .execute();
  }
}
