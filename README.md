# ts-msa - Gamified Quiz Platform

NZMSA 2026 Phase 2 submission.
A full-stack quiz platform with gamification built on .NET 10 + React/TypeScript.

## Stack

| Layer | Tech |
|-------|------|
| Backend | .NET 10 Web API, EF Core, SQLite |
| Frontend | React 18, TypeScript, Vite |
| API Docs | Scalar at /scalar |
| Tests | xUnit (backend), Vitest + RTL (frontend) |
| Deploy | Azure App Service (back) + Azure Static Web Apps (front) |

## Advanced Features

1. Badge system - auto-awarded on attempt submission (first_quiz, points_100, points_500, streak_7, perfect_score)
2. Leaderboard - ranked by total points, supports top-N query param
3. Streak tracking - daily streak increments on attempt; resets if no attempt the previous day

## Gamification Rules

- Points by difficulty: Easy x10 | Medium x20 | Hard x30
- Level = TotalPoints / 100 (level up every 100 points)

## Running Locally

### Backend

    cd back
    dotnet run
    API: http://localhost:5000
    Scalar docs: http://localhost:5000/scalar

### Frontend

    cd front
    cp .env.example .env
    npm install
    npm run dev
    App: http://localhost:5173

## Tests

    Backend (13 tests): cd back.tests && dotnet test
    Frontend (9 tests): cd front && npm test

## Live URLs

| | URL |
|--|-----|
| API | https://quizapi-ts-msa.azurewebsites.net |
| App | Azure Static Web Apps - pending |
| API Docs | https://quizapi-ts-msa.azurewebsites.net/scalar |

## AI Tools

Claude (Sonnet 4.6) and GitHub Copilot were used throughout - prompts and design decisions logged in back/specs/README.md.