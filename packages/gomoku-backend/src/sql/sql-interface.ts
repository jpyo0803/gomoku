import { User } from './user.entity';

export interface SqlInterface {
    createUser(username: string, password: string): Promise<User>;
    findUserByUsername(username: string): Promise<User | null>;
    updateUserResult(userId: number, result: 'win' | 'draw' | 'loss'): Promise<void>;
}