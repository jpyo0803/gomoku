export interface GatewayInterface {
  sendYourTurn(playerId: string, payload: any): void;
  sendBoardState(playerId: string, board: string): void;
  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void;

  init(): void;
  deinit(): void;
}