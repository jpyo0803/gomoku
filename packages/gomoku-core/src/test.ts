import { createEmptyBoard, Player } from './board';
import { printBoard } from './utils';
import readlineSync from 'readline-sync';

const boardSize: number = 15;

var board = createEmptyBoard(boardSize);

let isBlackTurn: boolean = true; // true: black, false: white

while (true) {
    // print the board
    console.log("Current board:");
    printBoard(board);


    if (isBlackTurn) {
        console.log("Black's turn");
    }
    else {
        console.log("White's turn");
    }

    // get user input x and y
    const x = parseInt(readlineSync.question('Enter x coordinate (0-14): '), 10);
    const y = parseInt(readlineSync.question('Enter y coordinate (0-14): '), 10);

    // play the game
    let result: number = board.play(x, y, isBlackTurn ? Player.Black : Player.White);
    if (result === -1) {
        console.log("Invalid move. Try again.");
        continue;
    }
    else if (result === 1) {
        console.log("Game over. Player " + (isBlackTurn ? "Black" : "White") + " wins!");
        break;
    }
    else {
        console.log("Continue the game.");
        isBlackTurn = !isBlackTurn; // switch turn
    }
}

