import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { ValidationPipe } from '@nestjs/common';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // DTO 유효성 검사 활성화
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,        // DTO에 정의된 속성만 허용
      forbidNonWhitelisted: true, // 정의 안된 속성 요청 시 에러
      transform: true,        // 자동 형변환 (문자 → 숫자 등)
    }),
  );
  
  await app.listen(process.env.PORT ?? 3000, '0.0.0.0'); // Client can reach the docker container
}
bootstrap();
