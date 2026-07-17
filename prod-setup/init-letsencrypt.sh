#!/bin/bash
# Script para obtener el certificado SSL inicial con Let's Encrypt
# Ejecutar UNA VEZ en tu mini PC Debian antes de levantar todo en producción
#
# USO: cd prod-setup && chmod +x init-letsencrypt.sh && ./init-letsencrypt.sh

DOMAIN="budgexaclient.duckdns.org"
EMAIL="ps3josemiguel@gmail.com"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

set -e

echo ">>> Parando contenedores existentes..."
docker compose -f "$ROOT_DIR/docker-compose.prod.yml" down 2>/dev/null || true

echo ">>> Creando volúmenes y levantando nginx con config temporal..."
# Usamos un contenedor nginx temporal con la config sin SSL
docker run -d --name nginx-certbot-temp \
  -p 80:80 \
  -v "$SCRIPT_DIR/nginx-init.conf:/etc/nginx/conf.d/default.conf:ro" \
  -v budgexa_certbot-www:/var/www/certbot \
  nginx:alpine

echo ">>> Esperando 3 segundos..."
sleep 3

echo ">>> Solicitando certificado SSL a Let's Encrypt..."
docker run --rm \
  -v budgexa_certbot-etc:/etc/letsencrypt \
  -v budgexa_certbot-var:/var/lib/letsencrypt \
  -v budgexa_certbot-www:/var/www/certbot \
  certbot/certbot certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email \
  -d "$DOMAIN"

echo ">>> Parando nginx temporal..."
docker stop nginx-certbot-temp && docker rm nginx-certbot-temp

echo ">>> Levantando todos los servicios de producción..."
docker compose -f "$ROOT_DIR/docker-compose.prod.yml" up -d --build

echo ""
echo "=========================================="
echo "¡LISTO! Tu app está en https://$DOMAIN"
echo "=========================================="
