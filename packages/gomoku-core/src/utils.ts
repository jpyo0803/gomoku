export function printBoardFromString(boardString: string): void {
  const size = 15;

  if (boardString.length !== size * size) {
    throw new Error(`Invalid board string length: expected ${size * size}, got ${boardString.length}`);
  }

  const symbols = {
    '.': '·',
    'B': '●',
    'W': '○',
  };

  // 열 인덱스 출력
  const colHeader = '   ' + Array.from({ length: size }, (_, i) =>
    i.toString().padStart(2, ' ')
  ).join(' ');
  console.log(colHeader);

  // 헤더 밑줄
  console.log('   ' + '-'.repeat(colHeader.length - 3));

  // 행 출력
  for (let y = 0; y < size; y++) {
    const start = y * size;
    const row = boardString.slice(start, start + size);
    const rowString = row
      .split('')
      .map(ch => symbols[ch as '.' | 'B' | 'W'] || '?')
      .join(' ');
    console.log(y.toString().padStart(2, ' ') + ' ' + rowString);
  }
}
