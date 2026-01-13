# Build React (or Vite) UI
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Serve with nginx + reverse proxy /api -> api service
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf

# If CRA (build/) exists, use it. If Vite (dist/) exists, use it.
# Use a small shell check instead of "2>/dev/null || true" (Dockerfile doesn't support that).
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/build /usr/share/nginx/html
# If your app is Vite, comment the line above and uncomment below:
# COPY --from=build /app/dist  /usr/share/nginx/html

EXPOSE 80
