import { createGomokuGame } from './gomoku';
import { printBoardFromString } from './utils';
import readlineSync from 'readline-sync';
import WebSocket from 'ws';

async function main() {
  const boardSize: number = 15; // Gomoku board size
  const maxDepth: number = 4;

  const game = createGomokuGame();
  const socket = new WebSocket('ws://localhost:8080');

  await new Promise<void>((resolve) => socket.on('open', resolve));

  const playerId = 'ts-test-client';

  // 메시지 응답을 기다리는 Promise 래퍼
  function waitForAIResponse(): Promise<{ x: number; y: number }> {
    return new Promise((resolve, reject) => {
      socket.once('message', (data: WebSocket.RawData) => {
        try {
          const msg = JSON.parse(data.toString());
          if (msg.type === 'place_stone_ai' && msg.playerId === playerId) {
            resolve({ x: msg.x, y: msg.y });
          } else {
            reject(new Error('Unexpected message: ' + JSON.stringify(msg)));
          }
        } catch (e) {
          reject(e);
        }
      });
    });
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
      } else {
        isBlackTurn = false;
      }
    } else {
      console.log("White (AI)'s turn...");

      const payload = {
        type: 'your_turn_ai',
        board: game.getBoardString(),
        playerId: playerId
      };

      socket.send(JSON.stringify(payload));

      try {
        const { x, y } = await waitForAIResponse();
        console.log(`AI chose move: (${x}, ${y})`);

        const result = game.play(x, y);
        if (result === 'invalid') {
          console.log("AI made an invalid move. Aborting.");
          break;
        } else if (result === 'win') {
          console.log("Game over. Piece White (AI) wins!");
          break;
        } else {
          isBlackTurn = true;
        }
      } catch (error: any) {
        console.error("Failed to contact AI server:", error.message);
        break;
      }
    }
  }

  printBoardFromString(game.getBoardString());
}

main();
