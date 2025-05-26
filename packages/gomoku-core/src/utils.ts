import { Piece } from './board'; // Piece enum 가져오기
import type { Board as BoardClass } from './board'; // Board 클래스의 타입 지정

export function printBoard(board: BoardClass): void {
  const symbols = {
    [Piece.Empty]: '·',
    [Piece.Black]: '●',
    [Piece.White]: '○',
  };

  const size = board.getSize();
  const rawBoard = board.getBoard();

  // 열 인덱스 출력 (간격을 1칸으로 조정)
  const colHeader = '   ' + Array.from({ length: size }, (_, i) =>
    i.toString().padStart(2, ' ')
  ).join(' ');
  console.log(colHeader);

  // 헤더 밑줄
  console.log('   ' + '-'.repeat(colHeader.length - 3));

  // 행 출력
  rawBoard.forEach((row, y) => {
    const rowString = row.map(cell => symbols[cell]).join(' ');
    console.log(y.toString().padStart(2, ' ') + ' ' + rowString);
  });
}
