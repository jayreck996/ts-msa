# ts-msa — Gamified Quiz Platform

NZMSA 2026 Phase 2 project: a gamified quiz platform with points, levels, streaks,
and badges, built with a .NET 10 Web API backend and a React + TypeScript frontend.

## Live URLs

| Service  | URL |
|----------|-----|
| Frontend | https://quizfrontsa.z8.web.core.windows.net/ |
| Backend API | https://quizapi-ts-msa.azurewebsites.net/ |
| API Docs (Scalar) | https://quizapi-ts-msa.azurewebsites.net/scalar |

## Tech Stack

- **Backend**: .NET 10 Web API, EF Core, SQLite, Scalar API docs
- **Frontend**: React, TypeScript, Vite, React Router
- **CI/CD**: GitHub Actions (path-filtered) → Azure App Service (backend, Kudu ZipDeploy) and Azure Blob Storage static website (frontend)

## Advanced Features

This project implements the following advanced features beyond core CRUD:

1. **Gamification system** — points awarded by quiz difficulty (Easy ×10, Medium ×20, Hard ×30 per correct answer)
2. **Leveling** — user level increases every 100 total points earned
3. **Daily streak tracking** — tracks consecutive days of quiz attempts, resets if a day is missed
4. **Badge system** — automatic badge awards on milestones (first quiz completed, point thresholds, 7-day streak, perfect score)

## Project Structure

```
ts-msa/
├── back/           # .NET 10 Web API (Controllers, Data, Models, specs/)
├── back.tests/     # Backend unit tests
├── front/          # React + TypeScript + Vite frontend
└── .github/workflows/  # CI/CD pipelines (backend.yml, frontend.yml)
```

## AI Usage

Claude, GitHub Copilot, and ChatGPT were used during development, per NZMSA policy.
Prompts and design decisions are logged in [`back/specs/README.md`](back/specs/README.md).

## Known Limitations

- SQLite storage is ephemeral on Azure App Service — data resets on every redeploy.
- Backend deploy auth uses Kudu basic auth (not RBAC) as a workaround for Azure for Students subscription restrictions blocking service-principal creation.
