import { User } from './user.entity';

export interface SqlInterface {
    createUser(username: string, password: string): Promise<User>;
    findUserByUsername(username: string): Promise<User | null>;
    updateUserStatsByUsername(username: string, result: 'win' | 'draw' | 'loss'): Promise<void>;
}