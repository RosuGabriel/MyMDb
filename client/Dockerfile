# Stage 1: Build the code
FROM node:22-alpine AS build
WORKDIR /app
ARG VITE_API_URL=http://api:5000/
COPY package*.json ./
RUN npm ci --prefer-offline --no-audit
COPY . .
RUN VITE_API_URL=$VITE_API_URL npm run build && npm cache clean --force

# Target: Production (Nginx static)
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]