import { User } from './user.entity';

export interface SqlInterface {
    createUser(username: string, password: string): Promise<{
        success: boolean;
        message?: string;
        user?: User;
        error?: 'CONFLICT' | 'INTERNAL_ERROR';
    }>;
    findUserByUsername(username: string): Promise<User | null>;
    updateUserStatsByUsername(username: string, result: 'win' | 'draw' | 'loss'): Promise<void>;
    updateUserRefreshToken(username: string, refreshToken: string): Promise<void>;
}