import { Injectable } from '@nestjs/common';

import * as http from 'http';
import axios from 'axios';

const maxRetries = 1000;
const pollingInterval = 500; // 500ms

async function requestAI(board: string, serverBaseUrl: string): Promise<{ x: number; y: number }> {
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

@Injectable()
export class WsGateway {
  private readonly AI_ENDPOINT = 'http://localhost:8080';
  
  constructor() {}

  async sendYourTurn(board: string): Promise<{ x: number; y: number }> {
    const { x, y } = await requestAI(board, this.AI_ENDPOINT);
    return { x, y };
  }
}
