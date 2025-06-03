import { createGomokuGame } from './gomoku';
import { printBoardFromString } from './utils';
import readlineSync from 'readline-sync';

import http from 'http';
import axios from 'axios';

async function main() {
  const game = createGomokuGame();
  const playerId = 'ts-test-client';
  const serverBaseUrl = 'http://localhost:8080';
  const maxRetries = 1000;
  const pollingInterval = 500; // 500ms

  async function requestAI(board: string): Promise<{ x: number; y: number }> {
    try {
      const agent = new http.Agent({ keepAlive: false }); // 매 요청마다 새로운 연결

      const solveRes = await axios.post(`${serverBaseUrl}/solve`, { board }, 
        {httpAgent: agent}  // keepAlive false로 설정
      );
      const taskId = solveRes.data.task_id;

      // Poll until result is ready
      for (let i = 0; i < maxRetries; ++i) { // up to 1000 tries
        try {
          const resultRes = await axios.get(`${serverBaseUrl}/result/${taskId}`);
          const data = resultRes.data;
          if (data.status === 'done') {
            return { x: data.x, y: data.y };
          } 
        } catch (err: any) {
          if (err.response && err.response.status === 404) {
            // 서버쪽에서 아직 task 등록을 못했을 수도 있음. 
          }
        }

        await new Promise((resolve) => setTimeout(resolve, pollingInterval));
      }

      throw new Error('Timeout waiting for AI response');
    } catch (err: any) {
      throw new Error(err.message);
    }
  }

  while (true) {
    console.log("Current board:");
    printBoardFromString(game.getBoardString());

    let isBlackTurn = game.getCurrentPlayer() === 'B';

    if (isBlackTurn) {
      console.log("Black's turn");
      const x = parseInt(readlineSync.question('Enter x coordinate (0-14): '), 10);
      const y = parseInt(readlineSync.question('Enter y coordinate (0-14): '), 10);

      const result = game.play(x, y);
      if (result === 'invalid') {
        console.log("Invalid move. Try again.");
        continue;
      } else if (result === 'win') {
        console.log("Game over. Piece Black wins!");
        break;
      }
    } else {
      console.log("White (AI)'s turn...");

      try {
        const { x, y } = await requestAI(game.getBoardString());
        console.log(`AI chose move: (${x}, ${y})`);

        const result = game.play(x, y);
        if (result === 'invalid') {
          console.log("AI made an invalid move. Aborting.");
          break;
        } else if (result === 'win') {
          console.log("Game over. Piece White (AI) wins!");
          break;
        }
      } catch (err: any) {
        console.error("Failed to get AI move:", err.message);
        break;
      }
    }
  }

  printBoardFromString(game.getBoardString());
}

main();
