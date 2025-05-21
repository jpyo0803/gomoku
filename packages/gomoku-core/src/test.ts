import { createEmptyBoard, Player } from './board';
import { printBoard } from './utils';
import readlineSync from 'readline-sync';
import http from 'http';
import axios from 'axios';

const boardSize: number = 15;
const maxDepth: number = 4;

var board = createEmptyBoard(boardSize);

let isBlackTurn: boolean = true; // true: black, false: white

const agent = new http.Agent({ keepAlive: false });

function nextTurn() {
    // print the board
    console.log("Current board:");
    printBoard(board);

    if (isBlackTurn) {
        console.log("Black's turn");

        // get user input x and y
        const x = parseInt(readlineSync.question('Enter x coordinate (0-14): '), 10);
        const y = parseInt(readlineSync.question('Enter y coordinate (0-14): '), 10);

        // play the game
        let result: number = board.play(x, y, Player.Black);
        if (result === -1) {
            console.log("Invalid move. Try again.");
            nextTurn(); // retry same turn
            return;
        } else if (result === 1) {
            console.log("Game over. Player Black wins!");
            return;
        } else {
            isBlackTurn = false;
            nextTurn(); // next turn
        }
    } else {
        console.log("White (AI)'s turn...");

        axios.post('http://localhost:8080/solve', {
            size: boardSize,
            max_depth: maxDepth,
            cells: board.getBoardRaw()
        }, {
            httpAgent: agent,
        })
        .then(response => {
            const { x, y } = response.data;
            console.log(`AI chose move: (${x}, ${y})`);

            let result: number = board.play(x, y, Player.White);
            if (result === -1) {
                console.log("AI made an invalid move. Aborting.");
                return;
            } else if (result === 1) {
                console.log("Game over. Player White (AI) wins!");
                return;
            } else {
                isBlackTurn = true;
                nextTurn();
            }
        })
        .catch(error => {
            console.error("Failed to contact AI server:", error.message);
        });
    }
}

// 시작
nextTurn();
