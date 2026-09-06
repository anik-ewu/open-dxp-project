FROM node:20-alpine AS dev
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
EXPOSE 4200
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--port", "4200"]

FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npx ng build

# Not yet deployed anywhere - built and smoke-tested locally (`docker build --target final`),
# never pushed to a registry or run against a real API. See infra/README.md.
FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist/admin/browser /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
ENV API_UPSTREAM=http://opendxp-api \
    DNS_RESOLVER=127.0.0.11
EXPOSE 80
