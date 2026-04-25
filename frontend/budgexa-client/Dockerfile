# Build de Angular
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --output-path=dist
RUN ls -lR /app/dist

# Imagen final: solo archivos estáticos, listos para servir por Traefik
FROM node:20-alpine AS prod
WORKDIR /app
COPY --from=build /app/dist/browser .
EXPOSE 80
CMD ["npx", "serve", "-s", ".", "-l", "80"]
