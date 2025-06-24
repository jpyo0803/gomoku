export interface NosqlInterface {
    popDataFromQueue(queueKey: string): Promise<string | null>;
    pushDataToQueue(queueKey: string, data: string): Promise<void>;

    registerGameInstance(gameId: string, gameInstance: any): Promise<void>;
    deleteGameInstance(gameId: string): Promise<void>;

    getGameInstance(playerId: string): Promise<any | null>;
    setGameInstance(playerId: string, gameInstance: any): Promise<void>;
}