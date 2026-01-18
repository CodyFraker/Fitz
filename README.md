# Fitz

An anti-bot Discord bot with a beer-based economy system, inspired by the character from 12oz Mouse. Fitz provides a comprehensive set of features for Discord servers including account management, banking, games, polls, and more—when it chooses to. It's really just for my personal discord guilds but I suppose if you really wanted to run it yourself, I recommend forking the repo and changing some of the hardcoded guild IDs.

## About

Fitz is a multi-purpose Discord bot built with .NET 8.0 and DSharpPlus. Unlike typical bots that obediently follow commands, Fitz is an **anti-bot** that performs commands if it chooses to do so. The bot centers around a beer-based economy where users can earn, spend, and interact with various features. Feeding Fitz beer helps encourage cooperation, making the beer economy central to the bot's functionality and personality.

### Inspiration

Fitz is inspired by the character **Fitz** from the animated series **12oz Mouse**. Just like the character, this bot embodies an anti-authoritarian personality—it's not a typical obedient bot, but rather one that operates on its own terms. The beer-based economy reflects the character's relationship with alcohol in the show, where beer serves as both currency and motivation. The architecture follows a feature-based system that allows for easy enablement and disablement of functionality, giving Fitz the flexibility to "choose" which features to engage with.

## Features

### 🍺 Economy System

- **Bank**: Complete banking system with beer as the primary currency
- **Accounts**: User account management with balance tracking
- **Transactions**: Full transaction history and logging
- **Happy Hour**: Double beer rewards during specific hours (7PM-11PM EST)
- **Favorability**: Dynamic favorability system based on beer balance ratios

### 🎰 Games & Entertainment

- **Lottery**: Regular lottery drawings with ticket purchasing system
- **Blackjack**: Interactive blackjack games with betting
- **Beer Hunt**: Random beer reactions on messages for surprise rewards

### 📊 Community Features

- **Polls**: Create and manage polls with approval workflow
  - User-submitted polls require approval from moderators
  - Poll creators earn beer when users vote
  - Support for multiple poll types and voting options
- **Rename**: Users can spend beer to rename other users in the guild
  - Configurable base cost and duration
  - Automatic expiration and nickname restoration

### 🎵 Media

- **Music**: Play music in voice channels using Lavalink
  - Queue management
  - Playback controls

### ⚙️ Administration

- **Settings**: Comprehensive settings management per guild
  - Configurable lottery pool, ticket costs, and durations
  - Account creation bonuses
  - Happy hour base amounts
  - Rename costs and limits
- **Feature Toggle**: Enable/disable features per guild
- **Admin Commands**: Specialized commands for administrators

## Architecture

Fitz is built as a modular system with three main components:

### Bot (`Fitz`)
- Core Discord bot implementation
- Feature-based architecture
- CQRS pattern for commands and queries
- Hangfire for background job processing
- OpenTelemetry metrics with Prometheus export

### API (`Fitz.Api`)
- RESTful API for bot data access
- Discord OAuth2 authentication
- OpenAPI/Swagger documentation
- Entity Framework Core with MySQL
- CORS support for web frontend

### Web (`Fitz.Web`)
- Next.js 14+ frontend application
- Mobile-first design
- TypeScript with auto-generated API client
- Discord OAuth integration

## Tech Stack

### Backend
- **.NET 8.0** - Core framework
- **DSharpPlus 5.0** - Discord API wrapper
- **Entity Framework Core** - ORM
- **MySQL** - Database (via Pomelo.EntityFrameworkCore.MySql)
- **Hangfire** - Background job processing
- **Lavalink4NET** - Music playback
- **Serilog** - Logging
- **OpenTelemetry** - Observability and metrics

### Frontend
- **Next.js 14+** - React framework with App Router
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **shadcn/ui** - UI components
- **Vitest** - Testing framework

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD

## Project Structure

```
Fitz/
├── Fitz/                    # Main bot application
│   ├── Core/               # Core bot functionality
│   ├── Features/           # Feature modules
│   ├── Events/             # Discord event handlers
│   └── Variables/          # Configuration constants
├── Fitz.Api/               # REST API
│   ├── Controllers/        # API endpoints
│   ├── Models/             # Request/Response models
│   └── Services/           # Business logic
├── Fitz.Database/          # Database layer
│   ├── Entities/           # Entity models
│   └── Migrations/         # EF Core migrations
├── Fitz.Web/               # Web frontend
│   └── src/
│       ├── app/            # Next.js pages
│       ├── components/     # React components
│       └── lib/            # Utilities
└── docker-compose.yml      # Container orchestration
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js (managed via mise)
- MySQL database
- Discord Bot Token
- Discord OAuth2 credentials (for API/Web)

### Environment Setup

1. Clone the repository
2. Copy `.env.example` to `.env` and configure:
   - Database connection strings
   - Discord bot token
   - Discord OAuth credentials
   - Other required environment variables

### Running with Docker Compose

The easiest way to run Fitz is using Docker Compose:

```bash
docker-compose up -d
```

This will start:
- **Bot**: The Discord bot service
- **API**: REST API on ports 5000 (HTTP) and 5001 (HTTPS)
- **Web**: Web frontend on port 3000

### Development Setup

#### Bot
```bash
cd Fitz
dotnet restore
dotnet run
```

#### API
```bash
cd Fitz.Api
dotnet restore
dotnet run
```

#### Web
```bash
cd Fitz.Web
mise install
npm install
npm run generate-api  # Requires API to be running
npm run dev
```

## Features Overview

### Account System
Users can create accounts to participate in the economy. Accounts track:
- Beer balance
- Username
- Account status (active/deactivated)
- Lottery subscription preferences

### Banking
The bank handles all beer transactions:
- Deposits and withdrawals
- Transfers between users
- Transaction logging
- Top balance leaderboards

### Lottery
Regular lottery drawings where users can:
- Purchase tickets with beer
- Subscribe to automatic ticket purchases
- Win from a growing pool
- View current lottery status and tickets

### Blackjack
Interactive blackjack games:
- Multiple game types
- Betting system
- Multi-player support
- Dealer logic

### Polls
Community-driven polling system:
- User-submitted polls
- Moderation workflow (approval/denial)
- Reaction-based voting
- Beer rewards for poll creators

### Rename
Users can spend beer to rename others:
- Configurable costs
- Temporary renames with expiration
- Automatic restoration

### Happy Hour
Scheduled bonus periods:
- Double beer rewards during Happy Hour
- Configurable time windows
- Automatic activation/deactivation

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns and structure
- All tests pass
- Features follow the CQRS pattern
- Controllers have associated test classes
- Endpoints are documented
