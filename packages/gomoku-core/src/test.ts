import { createGomokuGame } from './gomoku';
import { printBoardFromString } from './utils';
import readlineSync from 'readline-sync';
import http from 'http';
import axios from 'axios';

async function main() {
  const boardSize: number = 15; // Gomoku board size
  const maxDepth: number = 4;

  const game = createGomokuGame();
  const agent = new http.Agent({ keepAlive: false });


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
      try {
        const response = await axios.post('http://localhost:8080/solve', {
          size: boardSize,
          max_depth: maxDepth,
          cells: game.getBoardString(),
        }, {
          httpAgent: agent
        });

        const { x, y } = response.data;
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
