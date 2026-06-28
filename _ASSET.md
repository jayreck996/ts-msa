## 2026-06-29 - ts-msa: Gamified Quiz Platform

- Full-stack quiz app, NZMSA 2026 Phase 2 submission
- Backend: .NET 10 Web API, EF Core, SQLite, Scalar docs at /scalar
- Frontend: React 19 + TypeScript + Vite, 4 pages (Home, Quizzes, Badges, Leaderboard)
- Gamification: points by difficulty (Easy x10 / Medium x20 / Hard x30), levels every 100 pts, streaks, 5 badge types
- Tests: 13 xUnit (backend) + 9 Vitest/RTL (frontend)
- CI/CD: GitHub Actions to Azure App Service (back) + Azure Static Web Apps (front)
- API live: https://quizapi-ts-msa.azurewebsites.net
