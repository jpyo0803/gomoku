import { User } from './user.entity';

export interface SqlInterface {
    createUser(username: string, password: string): Promise<{
        success: boolean;
        message?: string;
        user?: User;
        errorCode?: 'CONFLICT' | 'INTERNAL_ERROR';
    }>;
    findUserByUsername(username: string): Promise<{
        success: boolean;
        message: string;
        user?: User;
        errorCode?: 'USER_NOT_FOUND' | 'INTERNAL_ERROR';
    }>;
    updateUserStatsByUsername(username: string, result: 'win' | 'draw' | 'loss'): Promise<void>;
    updateUserRefreshToken(username: string, refreshToken: string): Promise<void>;
}