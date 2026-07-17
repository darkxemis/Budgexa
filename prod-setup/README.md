# Prod Setup - Primera ejecución

Esta carpeta contiene scripts que **solo se ejecutan UNA VEZ** al configurar el servidor de producción (mini PC Debian).

## Archivos

- `init-letsencrypt.sh` - Obtiene los certificados SSL de Let's Encrypt
- `nginx-init.conf` - Config temporal de nginx usada solo durante la obtención del certificado

## Uso

```sh
cd prod-setup
chmod +x init-letsencrypt.sh
sudo ./init-letsencrypt.sh
```

## Después de la primera vez

Para reinicios o actualizaciones, solo necesitas (desde la raíz del repo):

```sh
sudo docker compose -f docker-compose.prod.yml up -d --build
```
