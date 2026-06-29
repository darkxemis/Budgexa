<div align="center">

# 💼 Budgexa

**The AI‑powered budgeting & quoting platform built for freelancers and the self‑employed.**

Generate professional budgets from a single sentence. Let the AI do the boring part.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Ollama](https://img.shields.io/badge/Ollama-Local%20LLM-000000?logo=ollama&logoColor=white)](https://ollama.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](./LICENSE)

</div>

---

## ✨ Overview

**Budgexa** is a full‑stack web application designed to help **freelancers and small businesses (autónomos)** spend less time on paperwork and more time on what they do best. Its flagship feature is an **integrated AI assistant** that turns a plain‑text description like *"3 hours of consulting and 2 logo designs"* into a fully structured budget with products and quantities — ready to review, edit and send.

The AI runs **100% locally** via [Ollama](https://ollama.com/), so your business data **never leaves your infrastructure**.

## 🚀 Key Features

- 🤖 **AI‑powered budget generation** — Extract products and quantities from free‑form text using local LLMs (default: `llama3.2`).
- 🔐 **Robust authentication** — JWT access tokens + refresh token rotation, HTTP‑only cookies, BCrypt password hashing.
- 🛡️ **Account protection** — Built‑in login lockout after repeated failed attempts.
- 👥 **Role‑based authorization** — Granular policies for `Standard`, `Administrator` and `SuperAdministrator` users.
- 🌍 **Internationalization (i18n)** — Multi‑language support with translations persisted in the database (no rebuilds to add a language).
- 📊 **Server‑side grids** — Sorting, filtering and pagination powered by [Gridify](https://gridify.alirezanet.com/).
- 📑 **Modern API docs** — Interactive [Scalar](https://scalar.com/) UI for OpenAPI exploration.
- 🐳 **Container‑first** — Production and development `docker-compose` stacks, including optional GPU acceleration for the AI service.
- 📦 **Auto‑migrations** — EF Core migrations are applied automatically on startup with retry logic.

## 🏗️ Architecture

The backend follows **Clean Architecture** with a strict dependency rule (outer layers depend on inner ones, never the other way around) and the **CQRS** pattern via [MediatR](https://github.com/jbogard/MediatR).

```
┌──────────────────────────────────────────────────────┐
│  Budgexa.API           ← Minimal APIs, middleware    │
│   ├─ Budgexa.Application  ← CQRS, validators, DTOs   │
│   │   └─ Budgexa.Domain   ← Entities, business rules │
│   └─ Budgexa.Infrastructure ← EF Core, JWT, Ollama   │
└──────────────────────────────────────────────────────┘
```

| Concern               | Implementation                                                       |
| --------------------- | -------------------------------------------------------------------- |
| API style             | ASP.NET Core **Minimal APIs** with endpoint groups                   |
| Use cases             | **CQRS** (Commands & Queries) via MediatR pipeline behaviors         |
| Validation            | **FluentValidation** auto‑wired through a `ValidationBehavior`       |
| Persistence           | **EF Core 10** + SQL Server, model snapshot migrations               |
| AI integration        | **OllamaSharp** client targeting a local/Docker Ollama instance      |
| Logging               | **Serilog** with console + rolling file sinks                        |
| Error handling        | Global `IExceptionHandler` returning RFC‑compliant problem responses |
| API documentation     | **Scalar** UI on top of native `Microsoft.AspNetCore.OpenApi`        |

## 🧰 Tech Stack

### Backend
- **.NET 10** (C# 14, nullable + implicit usings enabled)
- ASP.NET Core Minimal APIs
- Entity Framework Core 10 (SQL Server)
- MediatR 14 · FluentValidation 12 · Gridify
- OllamaSharp · BCrypt.Net‑Next · Serilog · Scalar
- **Testing**: xUnit · FluentAssertions · NSubstitute · EF Core InMemory

### Frontend
- **Angular 22** (standalone components, signals)
- TypeScript 6
- `@ngx-translate` for i18n
- Vitest for unit testing · Prettier for formatting

### Infrastructure
- SQL Server 2022 (Docker image)
- Ollama (local LLM runtime, optional NVIDIA GPU)
- Nginx (production frontend reverse proxy)
- Docker Compose (dev + prod stacks)

## 📁 Repository Structure

```
Budgexa/
├── backend/BudgexaApi/                  # .NET 10 solution
│   ├── src/
│   │   ├── Budgexa.API/                 # Endpoints, middleware, Program.cs
│   │   ├── Budgexa.Application/         # CQRS handlers, DTOs, validators
│   │   ├── Budgexa.Domain/              # Entities, value objects, contracts
│   │   └── Budgexa.Infrastructure/      # EF Core, JWT, AI, persistence
│   └── tests/
│       ├── Budgexa.Domain.Tests/        # Domain entities, exceptions, constants
│       ├── Budgexa.Application.Tests/   # CQRS handlers, validators, behaviors
│       ├── Budgexa.Infrastructure.Tests/# Auth, JWT, current-user services
│       └── Budgexa.API.Tests/           # Middleware, error handling
├── frontend/budgexa-client/             # Angular 22 SPA
├── .ai/                                 # AI assistant rules & conventions
├── docker-compose.dev.yml               # Local development stack
├── docker-compose.prod.yml              # Production stack
├── generate-dev-cert.ps1                # ASP.NET dev HTTPS certificate
└── LICENSE
```

## 🚦 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm 11+
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- (Optional) NVIDIA GPU + drivers for accelerated AI inference

### 1. Clone the repository

```bash
git clone https://github.com/darkxemis/Budgexa.git
cd Budgexa
```

### 2. Configure environment variables

Create a `.env` file in the repository root with at least:

```env
SA_PASSWORD=<your-strong-sqlserver-password>
CONNECTION_STRING=Server=sqlserver;Database=BudgexaDb;User=sa;Password=<same-password>;TrustServerCertificate=True;
JWT_SECRET=<your-base64-jwt-secret>
```

> 💡 Generate a strong JWT secret with `openssl rand -base64 64` (or any equivalent tool).

### 3. Generate the local HTTPS certificate (dev only)

```powershell
./generate-dev-cert.ps1
```

### 4. Start the full stack

```bash
docker compose -f docker-compose.dev.yml up --build
```

That's it. The first build pulls SQL Server, Ollama and builds both apps.

| Service        | URL                          |
| -------------- | ---------------------------- |
| Frontend (SPA) | http://localhost:4300        |
| API            | https://localhost:5443       |
| API docs       | https://localhost:5443/scalar |
| Ollama         | http://localhost:11434       |

### 5. Pull an AI model

Once the `ollama` container is running, pull the default model:

```bash
docker exec -it budgexa-ollama ollama pull llama3.2
```

You can switch to any other model supported by Ollama via the `Ollama:DefaultModel` setting.

## ⚙️ Configuration Highlights

Configuration is read from `appsettings.json`, environment variables and user secrets (in that order). Key sections:

| Section                | Purpose                                                          |
| ---------------------- | ---------------------------------------------------------------- |
| `ConnectionStrings`    | SQL Server connection string                                     |
| `JwtSettings`          | Token issuer, audience, lifetimes and signing secret             |
| `LoginLockout`         | Max failed attempts and lockout duration                         |
| `Cors:AllowedOrigins`  | Whitelist of frontend origins                                    |
| `Ollama`               | Base URL and default model for the local LLM                     |
| `Serilog`              | Log levels, output template and rolling file retention           |

> 🔒 **Never commit real secrets.** Use `dotnet user-secrets`, Docker secrets or your platform's secret manager for production credentials.

## 🤖 The AI Endpoint

The flagship feature is a single, focused endpoint:

```
POST /api/v1/budgets/ai-generate
Authorization: Bearer <token>

{
  "userRequest": "2 web pages and 5 hours of SEO consulting"
}
```

The handler builds a deterministic prompt, calls the local Ollama model, parses the JSON response and returns a structured list of products and quantities — ready to be turned into a real budget.

## 🧪 Development Workflow

- **Branching** — work happens on `develop`; `main` tracks releases.
- **Commits** — [Conventional Commits](https://www.conventionalcommits.org/) (`feat`, `fix`, `refactor`, `chore`, `docs`, `style`, `test`).
- **Code style** — see [`.ai/README.md`](./.ai/README.md) for the full set of conventions used by both humans and AI assistants in this repo.
- **Testing** — backend covered by **229+ unit tests** across `Budgexa.Domain.Tests`, `Budgexa.Application.Tests`, `Budgexa.Infrastructure.Tests` and `Budgexa.API.Tests` (xUnit + FluentAssertions + NSubstitute + EF Core InMemory).

### Running the backend test suite

```bash
cd backend/BudgexaApi
dotnet test
```

Or target a single layer:

```bash
dotnet test tests/Budgexa.Application.Tests/Budgexa.Application.Tests.csproj
```

In Visual Studio you can also use **Test Explorer** to discover and run them interactively.

### Run the backend without Docker

```bash
cd backend/BudgexaApi
dotnet run --project src/Budgexa.API
```

### Run the frontend without Docker

```bash
cd frontend/budgexa-client
npm install
npm start
```

## 📜 License

This project is **proprietary software**. All rights reserved by the copyright holder. See [LICENSE](./LICENSE) for details.

No part of this software may be reproduced, distributed, modified, or used in any form without prior written permission.

---

<div align="center">

Made with ❤️, ☕ and a lot of <code>dotnet watch</code>.

</div>
