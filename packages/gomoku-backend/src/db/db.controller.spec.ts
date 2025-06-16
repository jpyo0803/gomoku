import { Test, TestingModule } from '@nestjs/testing';
import { DBController } from './db.controller';

describe('DBController', () => {
  let controller: DBController;

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      controllers: [DBController],
    }).compile();

    controller = module.get<DBController>(DBController);
  });

  it('should be defined', () => {
    expect(controller).toBeDefined();
  });
});
