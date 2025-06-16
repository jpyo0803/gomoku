import { IsString, MinLength, MaxLength, Matches } from 'class-validator';

export class SignupDto {
  @IsString()
  @MinLength(3)
  @MaxLength(20)
  username: string;

  @IsString()
  @MinLength(6)
  @MaxLength(32)
  // 영문자/숫자 포함 조건: 필요 시 아래와 같이 추가
  // @Matches(/^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$/, {
  //   message: 'Password too weak',
  // })
  password: string;
}