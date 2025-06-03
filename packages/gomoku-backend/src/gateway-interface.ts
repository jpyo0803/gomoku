export interface GatewayInterface {
  sendMatchMakingSuccess(playerId: string, opponentId: string, gameId: string, stoneColor: string): void;
  sendYourTurn(playerId: string, payload: any): void;
  sendBoardState(playerId: string, board: string): void;
  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void;

  init(): void;
  deinit(): void;
}