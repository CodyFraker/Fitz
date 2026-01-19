FROM --platform=$TARGETPLATFORM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM --platform=$TARGETPLATFORM node:20-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=build --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=build --chown=nextjs:nodejs /app/.next/static ./.next/static
RUN mkdir -p ./public && chown -R nextjs:nodejs ./public

USER nextjs

EXPOSE 5173

ENV PORT=5173
ENV HOSTNAME="0.0.0.0"

CMD ["node", "server.js"]
