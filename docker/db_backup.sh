#!/bin/bash

# Sidecar 컨테이너에서 사용할 PostgreSQL 백업 스크립트

# docker-compose.yml에서 설정한 환경 변수를 가져와 DB 연결 정보 설정
DB_HOST=${POSTGRES_HOST:-postgres}
DB_USER=${POSTGRES_USER:-postgres}
DB_NAME=${POSTGRES_DB:-postgres}
DB_PASS=${POSTGRES_PASSWORD:-postgres}

# 백업 파일을 저장할 디렉토리
BACKUP_DIR="/backups"

# 파일명 형식: DBNAME_YYYY-MM-DD_HH-MM-SS.sql
FILENAME="${BACKUP_DIR}/${DB_NAME}_$(date "+%Y-%m-%d_%H-%M-%S").sql.gz"

# 백업 디렉토리 생성
mkdir -p "${BACKUP_DIR}"

echo "[$(date)] -> Starting backup of '${DB_NAME}' from host '${DB_HOST}'..."

# PGPASSWORD를 사용하여 pg_dump 실행하고, 결과를 gzip으로 바로 압축해 용량 최소화
PGPASSWORD=${DB_PASS} pg_dump -h "${DB_HOST}" -U "${DB_USER}" -d "${DB_NAME}" -Fc | gzip > "${FILENAME}"

# pg_dump의 종료 코드($?)를 확인하여 성공/실패를 판단
if [ ${PIPESTATUS[0]} -eq 0 ]; then
  echo "[$(date)] -> Backup successful:  ${FILENAME}"
  # 3일 이상된 백업 파일 삭제
  find "${BACKUP_DIR}" -type f -name "*.sql.gz" -mtime +3 -delete
  echo "[$(date)] -> Old backups (older than 3 days) cleaned up."
else
  echo "[$(date)] -> ERROR: Backup failed."
  rm -f "${FILENAME}" # 실패 시 빈 파일 삭제
fi