export interface AiGatewayInterface {
  sendYourTurn(board: string): Promise<{ x: number; y: number }>;
}