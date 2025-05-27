import { Injectable, OnModuleDestroy, Inject, forwardRef } from '@nestjs/common';
import { WebSocket } from 'ws';
import { GameService } from './game-service';
import { GatewayInterface } from './gateway-interface';

@Injectable()
export class WsGateway implements GatewayInterface, OnModuleDestroy {
  private socket?: WebSocket;
  private readonly AI_ENDPOINT = 'ws://localhost:8080';

  constructor(
    @Inject(forwardRef(() => GameService))
    private readonly gameService: GameService,
  ) {}

  /* ----------------------------------------------------------------
     턴 알림 – 소켓 상태에 따라 분기
  ---------------------------------------------------------------- */
  sendYourTurn(playerId: string, board: string): void {
    const payload = JSON.stringify({ type: 'your_turn_ai', playerId, board });

    // 1) 소켓이 없거나 이미 종료 → 새로 만들고 열린 뒤 전송
    if (!this.socket || this.socket.readyState >= WebSocket.CLOSING) {
      this.connect().then(() => this.socket!.send(payload));
      return;
    }

    // 2) 연결 중( CONNECTING ) → open 이벤트 후 전송
    if (this.socket.readyState === WebSocket.CONNECTING) {
      this.socket.once('open', () => this.socket!.send(payload));
      return;
    }

    // 3) 이미 OPEN → 즉시 전송
    console.log('[AI] sending your_turn to AI');
    this.socket.send(payload);
  }

  /* ----------------------------------------------------------------
     필요 없는 인터페이스 메서드 (AI 쪽)
  ---------------------------------------------------------------- */
  sendBoardState(): void {}
  sendPlaceStoneResp(): void {}
  init(): void {}
  deinit(): void {}

  /* ----------------------------------------------------------------
     애플리케이션 종료 시 소켓 정리
  ---------------------------------------------------------------- */
  onModuleDestroy() {
    if (this.socket && this.socket.readyState < WebSocket.CLOSING) {
      this.socket.terminate();
    }
  }

  /* ----------------------------------------------------------------
     소켓 생성 + 리스너 등록. open 시 resolve.
  ---------------------------------------------------------------- */
  private connect(): Promise<void> {
    return new Promise<void>(resolve => {
      this.socket = new WebSocket(this.AI_ENDPOINT);
      console.log('[AI] connecting →', this.AI_ENDPOINT);

      this.socket.once('open', () => {
        console.log('[AI] WebSocket OPEN');
        resolve();
      });

      this.socket.on('message', async raw => {
        try {
          const msg = JSON.parse(raw.toString());
          if (msg.type === 'place_stone_ai') {
            const { playerId, x, y } = msg;
            await this.gameService.handlePlaceStoneAI(playerId, x, y);
          }
        } catch {
          console.error('[AI] invalid message:', raw.toString());
        }
      });

      this.socket.on('close', () => (this.socket = undefined));
      this.socket.on('error', err => console.error('[AI] socket error:', err));
    });
  }
}
