export class Player {
    private id: string;
    private isAI: boolean;

    constructor(id: string, isAI: boolean) {
        this.id = id;
        this.isAI = isAI;
    }
    
    getId(): string {
        return this.id;
    }

    isAIPlayer(): boolean {
        return this.isAI;
    }

    static fromJSON(json_data: any): Player {
        return new Player(json_data.id, json_data.isAI);
    }
}