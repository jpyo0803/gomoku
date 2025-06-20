export interface ClientGatewayInterface {
  sendMatchMakingSuccess(playerId: string, opponentId: string, gameId: string, stoneColor: string): void;
  sendYourTurn(playerId: string, payload: any): void;
  sendBoardState(playerId: string, board: string, lastMove: {x: number, y: number}): void;
  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void;
}