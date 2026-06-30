#!/bin/sh
mkdir -p ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/nginx.key -out ssl/nginx.crt \
  -subj "/C=ES/ST=State/L=City/O=Organization/CN=localhost"
