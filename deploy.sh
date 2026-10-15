#!/usr/bin/env bash
# ==============================================================================
# Sitemenggaris Excel-to-XML Converter - Production Deployment Script
# Target: JOB PERTAMINA - MEDCO E&P SIMENGGARIS Environment
# ==============================================================================

set -e

# Warna untuk output terminal
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}==================================================================${NC}"
echo -e "${BLUE}   SITEMENGGARIS EXCEL-TO-XML CONVERTER - PRODUCTION DEPLOYMENT   ${NC}"
echo -e "${BLUE}==================================================================${NC}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# 1. Periksa ketersediaan Docker & Docker Compose
echo -e "\n${YELLOW}[1/5] Memeriksa dependensi Docker...${NC}"
if ! command -v docker &> /dev/null; then
    echo -e "${RED}[ERROR] Docker tidak ditemukan! Silakan install Docker terlebih dahulu.${NC}"
    exit 1
fi

if docker compose version &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker compose"
elif command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
else
    echo -e "${RED}[ERROR] Docker Compose tidak ditemukan!${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker & Docker Compose tersedia.${NC}"

# 2. Periksa / inisialisasi file .env
echo -e "\n${YELLOW}[2/5] Menyiapkan environment configuration...${NC}"
if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        echo -e "${YELLOW}File .env tidak ditemukan, membuat dari .env.example...${NC}"
        cp .env.example .env
    else
        cat << 'EOF' > .env
FRONTEND_PORT=3050
BACKEND_PORT=5050
SQLSERVER_PORT=1433
MSSQL_SA_PASSWORD=Simenggaris2026!
MSSQL_PID=Developer
DB_NAME=SitemenggarisDb
EOF
    fi
fi

# Load variabel dari .env
export $(grep -v '^#' .env | xargs)

FRONTEND_PORT=${FRONTEND_PORT:-3050}
BACKEND_PORT=${BACKEND_PORT:-5050}
SQLSERVER_PORT=${SQLSERVER_PORT:-1433}

echo -e "Port yang akan digunakan:"
echo -e "  - Frontend Web UI : ${GREEN}${FRONTEND_PORT}${NC} (tidak bentrok dengan m2prime_web di 3000)"
echo -e "  - Backend REST API: ${GREEN}${BACKEND_PORT}${NC} (internal/localhost)"
echo -e "  - SQL Server 2022 : ${GREEN}${SQLSERVER_PORT}${NC} (internal/localhost)"

# 3. Validasi potensi konflik port dengan container lain
echo -e "\n${YELLOW}[3/5] Memeriksa ketersediaan port di server...${NC}"
CONFLICT_CONTAINERS=$(docker ps --format "{{.Names}}: {{.Ports}}" | grep -E ":${FRONTEND_PORT}->|:${BACKEND_PORT}->|:${SQLSERVER_PORT}->" || true)
if [ -n "$CONFLICT_CONTAINERS" ]; then
    echo -e "${YELLOW}[PERINGATAN] Port terdeteksi digunakan oleh container lain:${NC}"
    echo -e "$CONFLICT_CONTAINERS"
    echo -e "${YELLOW}Silakan ubah port pada file .env jika diperlukan.${NC}"
else
    echo -e "${GREEN}✓ Port aman, tidak ada konflik terdeteksi.${NC}"
fi

# 4. Build dan jalankan container
echo -e "\n${YELLOW}[4/5] Membangun dan menjalankan container (docker compose up -d --build)...${NC}"
$DOCKER_COMPOSE_CMD up -d --build

# 5. Verifikasi status container dan healthcheck
echo -e "\n${YELLOW}[5/5] Menunggu inisialisasi database dan backend...${NC}"
for i in {1..20}; do
    STATUS_SQL=$(docker inspect --format='{{json .State.Health.Status}}' excel-to-xml-sqlserver 2>/dev/null || echo '"unknown"')
    if [ "$STATUS_SQL" == "\"healthy\"" ]; then
        echo -e "${GREEN}✓ SQL Server 2022 telah aktif dan berstatus HEALTHY.${NC}"
        break
    fi
    echo -n "."
    sleep 2
done

sleep 3

# Tampilkan status container
echo -e "\n${BLUE}==================================================================${NC}"
echo -e "${GREEN}              DEPLOYMENT BERHASIL DISELESAIKAN!                  ${NC}"
echo -e "${BLUE}==================================================================${NC}"
$DOCKER_COMPOSE_CMD ps

# Ambil IP server
SERVER_IP=$(hostname -I 2>/dev/null | awk '{print $1}')
if [ -z "$SERVER_IP" ]; then
    SERVER_IP=$(ip route get 1 2>/dev/null | awk '{print $7; exit}' || ipconfig getifaddr en0 2>/dev/null || echo "localhost")
fi
[ -z "$SERVER_IP" ] && SERVER_IP="localhost"

echo -e "\n${GREEN}URL Akses Aplikasi:${NC}"
echo -e "  ▶ Web Interface : ${BLUE}http://${SERVER_IP}:${FRONTEND_PORT}${NC}"
echo -e "  ▶ Backend Swagger: ${BLUE}http://localhost:${BACKEND_PORT}/swagger${NC}"

echo -e "\n${YELLOW}Perintah Operasional & Monitoring:${NC}"
echo -e "  - Cek Log Realtime : ${GREEN}$DOCKER_COMPOSE_CMD logs -f${NC}"
echo -e "  - Cek Log Backend  : ${GREEN}docker logs -f excel-to-xml-backend${NC}"
echo -e "  - Cek Log SQL Server: ${GREEN}docker logs -f excel-to-xml-sqlserver${NC}"
echo -e "  - Restart Service  : ${GREEN}$DOCKER_COMPOSE_CMD restart${NC}"
echo -e "  - Stop Service     : ${GREEN}$DOCKER_COMPOSE_CMD down${NC}"
echo -e "${BLUE}==================================================================${NC}\n"
