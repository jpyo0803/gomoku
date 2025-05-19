#!/bin/bash

# launch.sh가 있는 디렉토리의 부모 = top_directory
PROJECT_DIR=$(cd "$(dirname "$0")/.." && pwd)
IMAGE_NAME="ts-zsh-dev"

# Docker 이미지 빌드
docker build -t $IMAGE_NAME -f "$PROJECT_DIR/docker/Dockerfile" "$PROJECT_DIR"

# 컨테이너 실행 (top_directory를 /gomoku으로 마운트)
docker run --rm -it \
  -v "$PROJECT_DIR":/gomoku \
  -w /gomoku \
  $IMAGE_NAME
