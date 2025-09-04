# Gomoku Online

Gomoku Online은 온라인으로 즐기는 오목 게임으로, NestJS 기반 백엔드와 Unity 기반 클라이언트로 구성되어 있습니다.

---

## 백엔드 서버 실행 방법
> **사전 준비**: [Docker](https://docs.docker.com/get-docker/) 설치 필요

```bash
# docker 디렉토리로 이동
cd docker

# 백엔드 및 필요한 서비스(PostgreSQL, Redis 등) 실행
docker-compose up -d
```

---
## 클라이언트 실행 방법
1. Unity에 'unity/gomoku-client'를 등록 
2. Unity에서 Scenes에 있는 'BootScene', 'AuthScene', 'GameSettingScene', 'PlayScene'을 Hierarchy에 추가
3. 해상도를 WXGA (1366x768)로 설정
4. 빌드 후 실행
