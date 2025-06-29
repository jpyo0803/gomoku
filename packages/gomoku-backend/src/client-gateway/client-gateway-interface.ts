export type PlayerInfo = {
  username: string;
  isAI: boolean;
  stoneColor: string; // 'black' or 'white'
  totalGames: number;
  wins: number;
  draws: number;
  losses: number;
};

export interface ClientGatewayInterface {
  sendMatchMakingSuccess(
    myInfo: PlayerInfo,
    opponentInfo: PlayerInfo,
    gameId: string,
  ): void;
  sendYourTurn(playerId: string, payload: any): void;
  sendBoardState(playerId: string, board: string, lastMove: {x: number, y: number}): void;
  sendPlaceStoneResp(playerId: string, result: 'ok' | 'invalid' | 'win' | 'lose'): void;
}