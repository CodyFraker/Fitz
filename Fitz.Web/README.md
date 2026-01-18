# Fitz Web Frontend

Mobile-focused Next.js frontend application for the Fitz Bot.

## Tech Stack

- **Framework**: Next.js 14+ (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **Components**: shadcn/ui
- **Testing**: Vitest, React Testing Library, MSW
- **API Client**: Auto-generated from OpenAPI spec

## Getting Started

### Prerequisites

- mise (for tool version management) - Install from https://mise.jdx.dev/
- Running Fitz.Api backend (for API client generation)

### Installation

1. Install mise (if not already installed):
   - Windows: `winget install jdx.mise` or `scoop install mise`
   - Mac: `brew install mise`
   - Linux: `curl https://mise.run | sh`

2. Install tools (Node.js will be automatically installed):
```bash
mise install
```

3. Install dependencies:
```bash
npm install
```

2. Create `.env.local` file:
```env
NEXT_PUBLIC_API_URL=https://localhost:5001
NEXT_PUBLIC_DISCORD_CLIENT_ID=your_discord_client_id
NEXT_PUBLIC_DISCORD_REDIRECT_URI=http://localhost:5173/callback
```

3. Generate API client (requires backend to be running):
```bash
npm run generate-api
```

4. Run development server:
```bash
npm run dev
```

Open [http://localhost:5173](http://localhost:5173) in your browser.

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint
- `npm test` - Run tests in watch mode
- `npm run test:ci` - Run tests once (for CI)
- `npm run test:coverage` - Generate test coverage report
- `npm run test:ui` - Open Vitest UI
- `npm run generate-api` - Generate TypeScript API client from OpenAPI spec

## Project Structure

```
Fitz.Web/
├── src/
│   ├── app/              # Next.js App Router pages
│   ├── components/       # React components
│   ├── lib/             # Utilities and API client
│   ├── hooks/           # Custom React hooks
│   ├── contexts/        # React contexts
│   ├── types/           # TypeScript types
│   ├── styles/          # Global styles
│   ├── mocks/           # MSW mock handlers
│   └── test-utils/      # Testing utilities
├── scripts/             # Build scripts
└── public/              # Static assets
```

## Testing

Tests follow the Arrange, Act, Assert pattern and use MSW for API mocking.

Run tests:
```bash
npm test
```

## API Client Generation

The API client is automatically generated from the OpenAPI spec provided by the backend. Make sure the backend is running, then:

```bash
npm run generate-api
```

This will fetch the OpenAPI spec from `https://localhost:5001/swagger/v1/swagger.json` and generate TypeScript types and API client code.

## Authentication

The app uses Discord OAuth2 for authentication. Users are redirected to Discord for login, then return with an authorization code that is exchanged for an access token.

## Mobile-First Design

All components are designed mobile-first with:
- Touch-friendly targets (min 44x44px)
- Responsive layouts
- Mobile-optimized navigation
- Optimized performance
