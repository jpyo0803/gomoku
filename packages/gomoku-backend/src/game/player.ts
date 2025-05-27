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
}