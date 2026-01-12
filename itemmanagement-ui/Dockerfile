=== itemmanagement-ui/Dockerfile ===
# Build React (or Vite) UI
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Serve with nginx + reverse proxy /api -> api service
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Works for most React builds:
# - CRA outputs build/
# - Vite outputs dist/
COPY --from=build /app/build /usr/share/nginx/html 2>/dev/null || true
COPY --from=build /app/dist  /usr/share/nginx/html 2>/dev/null || true

