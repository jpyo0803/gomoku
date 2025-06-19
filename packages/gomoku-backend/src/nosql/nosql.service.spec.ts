import { Test, TestingModule } from '@nestjs/testing';
import { NosqlService } from './nosql.service';

describe('NosqlService', () => {
  let service: NosqlService;

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      providers: [NosqlService],
    }).compile();

    service = module.get<NosqlService>(NosqlService);
  });

  it('should be defined', () => {
    expect(service).toBeDefined();
  });
});
