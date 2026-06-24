## ASSET:-jay 2026-06-25 -> NZMSA Phase-2 | Scalar API Docs
- Package: Scalar.AspNetCore in back/QuizApi.csproj
- Wired in back/Program.cs: app.MapOpenApi() + app.MapScalarApiReference()
- Live endpoint: https://quizapi-ts-msa.azurewebsites.net/scalar (available after deployment)
## ASSET:-jay 2026-06-25 -> NZMSA Phase-2 | Badges Page
- File: front/src/pages/Badges.tsx
- Shows all badges from GET /api/badges
- User ID input + lookup calls GET /api/badges/user/{id} to show earned badges
- Unearned badges dimmed at 40% opacity, earned marked with checkmark
- Wired at /badges route in App.tsx
## ASSET:-jay 2026-06-25 -> NZMSA Phase-2 | Low Hanging Fruit Roadmap
- Roadmap agreed 2026-06-25 with Claude (Sonnet 4.6)
- Order: README (done) -> /specs -> UI features (task 6) -> Scalar (task 10) -> UI design (task 7) -> B1 upgrade + back deploy (task 8) -> front deploy (task 9) -> make public + verify (tasks 13-14)
- Rationale: write-only tasks first, then frontend-only (no Azure needed), then deployment tier last
## ASSET:-jay 2026-06-25 -> NZMSA Phase-2 | Docs | Root README.md
- Category: Documentation | Subcategory: Root README
- File: README.md at repo root
- URL: https://github.com/jayreck996/ts-msa/blob/main/README.md
- Sections: stack, advanced features, gamification rules, running locally, tests, live URLs, AI tools note
- Task 12 DONE
## ASSET:-jay 2026-06-24 -> NZMSA Phase-2 | Deployment | Azure Resources Created
- Category: Deployment | Subcategory: Azure Resources

| Resource | Name | Status |
|----------|------|--------|
| Resource Group | rg-ts-msa (Australia East) | Created |
| App Service Plan | plan-ts-msa (Linux) | Created |
| Web App (back) | quizapi-ts-msa.azurewebsites.net | QuotaExceeded - F1 limit hit |
| Azure Static Web Apps (front) | Not created yet | Pending |

- Blocker: F1 free tier 60 min/day CPU quota exhausted during deploy attempts
- Fix: upgrade plan-ts-msa to B1 Basic (~$13/mo, covered by $100 student credit)

## ASSET:-jay 2026-06-24 -> NZMSA Phase-2 | Deployment | Status
- Category: Deployment | Subcategory: Current State

| What | Status |
|------|--------|
| ts-msa GitHub repo | LIVE (private, jayreck996/ts-msa) |
| back/ .NET 10 API | Local only (C:\Users\tnako\Documents\GitHub\ts-msa\back\) |
| front/ React app | Local only |
| Azure App Service (back) | Not created yet |
| Azure Static Web Apps (front) | Not created yet |

## ASSET:-jay 2026-06-24 -> NZMSA Phase-2 | DB | SQLite on Azure App Service
- Category: Database | Subcategory: Hosting Decision
- Engine: SQLite (file-based, quiz.db) - EF Core auto-creates on startup via EnsureCreated()
- Local: runs in back/ folder during development
- Azure: lives on App Service ephemeral filesystem - resets on redeploy, persists between requests
- MSA fit: sufficient - markers access live app, no redeploy during marking window
- Ruled out: Azure SQL (~/mo, overkill) | Cosmos DB (free tier but high setup effort)

## ASSET:-jay 2026-06-24 -> NZMSA Phase-2 | Integration Tests | back.tests/
- Category: Integration Tests | Subcategory: xUnit + EF Core InMemory
- 13 tests, 0 failures - Controllers: Categories, Users, Quizzes, Leaderboard
- Why integration: real EF Core pipeline, in-memory DB per test (no mocking)
- Run: cd back.tests && dotnet test

## ASSET:-jay 2026-06-24 -> NZMSA Phase-2 | Unit Tests | front/__tests__/
- Category: Unit Tests | Subcategory: Vitest + React Testing Library
- 9 tests, 0 failures - Pages: Home, Quizzes, Leaderboard
- Why unit: API layer mocked via vi.mock - no network, no backend
- Config: jsdom env, globals true, setup file imports jest-dom matchers
- Run: cd front && npm test (CI) | npm run test:watch (dev)

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: Test Setup References
- Backend test project: ts-msa/back.tests/QuizApi.Tests.csproj
  - Packages: xunit, Microsoft.EntityFrameworkCore.InMemory, project ref to back/QuizApi.csproj
  - Helper: back.tests/Helpers/DbHelper.cs - creates isolated in-memory DB per test (Guid name)
  - Test files: Controllers/CategoriesControllerTests.cs, UsersControllerTests.cs, QuizzesControllerTests.cs, LeaderboardControllerTests.cs
- Frontend test setup: ts-msa/front/
  - Packages: vitest, @vitest/coverage-v8, jsdom, @testing-library/react, @testing-library/jest-dom, @testing-library/user-event
  - Config: vite.config.ts test block - environment jsdom, globals true, setupFiles src/test/setup.ts
  - Setup file: src/test/setup.ts - imports @testing-library/jest-dom
  - Test files: src/pages/__tests__/Home.test.tsx, Quizzes.test.tsx, Leaderboard.test.tsx
  - Scripts: npm test (vitest run) | npm run test:watch (vitest)
- Run backend tests: cd back.tests && dotnet test
- Run frontend tests: cd front && npm test

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: Monorepo File Structure
- ts-msa/back/ - .NET 10 Web API
  - Controllers/: AttemptsController, BadgesController, CategoriesController, LeaderboardController, OptionsController, QuestionsController, QuizzesController, UsersController
  - Data/AppDbContext.cs - EF Core DbContext, SQLite, cascade rules
  - Models/: Badge, Category, Option, Question, Quiz, QuizAttempt, User, UserBadge
  - Program.cs - CORS, Scalar, EF Core, DB auto-create
  - specs/README.md - AI prompts and design decisions log
  - back/QuizApi.csproj - packages: EF Core Sqlite, EF Core Design, Scalar.AspNetCore, OpenApi
- ts-msa/front/ - React + TypeScript + Vite
  - src/api.ts - typed fetch client, all endpoints, shared interfaces
  - src/App.tsx - BrowserRouter, NavLink, 3 routes
  - src/pages/: Home.tsx, Quizzes.tsx, Leaderboard.tsx
  - staticwebapp.config.json - Azure Static Web Apps SPA fallback
  - .env.example - VITE_API_URL template
- ts-msa/could/ - ISSUE-2026Q2.md, ASSET-2026Q2.md

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: Azure Deployment Stack
- Frontend: Azure Static Web Apps - free tier | https://portal.azure.com
- Backend: Azure App Service - free F1 tier | .NET 10 Web API
- Local repo: C:\Users\tnako\Documents\GitHub\ts-msa
- Monorepo layout: ts-msa/back/ (API) | ts-msa/front/ (React/TS/Vite)
- CI/CD: GitHub Actions auto-generated by Azure Static Web Apps on repo link
- SPA config: staticwebapp.config.json at front root - routes all 404s to index.html
- Env var: VITE_API_URL in front/.env (local) and Azure app settings (prod)

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: ts-msa Repo + AI Policy
- Repo: https://github.com/jayreck996/ts-msa (private)
- Stack: .NET 10 Web API + EF Core + SQLite | React + TypeScript + Vite
- AI tools allowed: Claude, GitHub Copilot, ChatGPT - explicitly encouraged in Phase 2 README
- AI usage must be logged in /specs folder (prompts + design decisions) - marked as part of assessment
- GitHub naming rules: letters/numbers/hyphens/underscores/dots | no spaces | no leading dot or hyphen | max 100 chars

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: Submission Requirements Summary
- Repo must be public with all files needed to run the code
- Backend: .NET 10 Web API | CRUD endpoints | EF Core | SQLite | Scalar docs at /scalar
- Frontend: React + TypeScript + Vite | proper UI (MUI/Mantine/Tailwind) | mobile-friendly
- Tests: unit tests for both frontend and backend
- Deployment: live URL required (Render / Railway / Azure / Vercel / Netlify)
- Advanced features: minimum 3, listed in README
- /specs folder: AI prompts used + design decisions documented throughout build
- AI tools explicitly encouraged: Claude, GitHub Copilot, ChatGPT
- Academic integrity: must understand and own your own work - copying flagged and disqualified

## ASSET:-jay 2026-06-24 -> NZMSA 2026-Phase-2: Software Stream References
- Repo: https://github.com/NZMSA/2026-Phase-2
- Assessment PDF: https://github.com/NZMSA/2026-Phase-2/blob/main/software/2026%20Phase%202%20-%20Software%20Assessment.pdf
- Demo scaffold: https://github.com/NZMSA/2026-Phase-2/tree/main/software/demo
- Submission form (opens July 13): https://forms.office.com/r/tRVKyQZVZ7
- Tech stack: .NET 10 Web API + EF Core + SQLite | React + TypeScript + Vite
- Demo runs on: backend localhost:5000 (Scalar docs at /scalar) | frontend localhost:5173
- Office Hours (Fridays 7:30-8 PM NZST): July 3, July 17, July 31 - Discord #help-software voice channel

# ASSET Log - MSA (migrated from -jay)

## ASSET:-jay 2026-06-15 09:38 -> AUT Official Transcript (Eligibility Proof)
- Document: Transcript of Official Academic Record - Auckland University of Technology
- Student: Jay Reck | ID: 20120687 | NSI: 130305394
- Covers: 2023, 2024, 2025, 2026 academic years
- 2025 courses confirmed: COMP702, COMP703, COMP707, COMP705, COMP716, COMP718, COMP721 + others (S1+S2)
- Google Drive: https://drive.google.com/file/d/1xTH3ADpmICWiFCjTkehEyAxDy5XrBbit/view?usp=sharing
- Used for: NZMSA 2026 Phase 2/3 eligibility proof submission

## ASSET:-jay 2026-06-15 09:38 -> NZMSA 2026-Phase-1: Programme Reference + Badge Map
- Programme: MSA NZ 2026 Phase 1 - Software Development Stream
- Ref: https://github.com/NZMSA/2026-Phase-1
- Submission form: https://forms.office.com/r/6nssmi68X6
- SW Dev collection: https://learn.microsoft.com/en-nz/collections/n2kyajtpq8my3q
- Deadline: 29 May 2026 11:59pm (missed - attempt late submission)
- Week 1 COMPLETE: Introduction to GitHub | Write your first C# code | Introduction to .NET | Create a new .NET project and work with dependencies | Introduction to .NET web development with ASP.NET Core | Build your first ASP.NET Core web app | Customize ASP.NET Core behavior with middleware
- Week 5 COMPLETE: Introduction to Transact-SQL | Sort and filter results in T-SQL | Combine multiple tables with JOINs in T-SQL | Modify data with T-SQL
- Week 6 (4/5): Use a database with minimal API EF Core ASP.NET Core | Build CI workflows by GitHub Actions | Build a containerized web application with Docker | Host a web application with Azure App Service
- Pending: React Tic-Tac-Toe (W2) | TypeScript tutorials W3-W4 (no badge) | Introduction to software testing concepts (W6)

## ASSET:-jay 2026-06-15 09:38 -> NZMSA 2024-Phase-1: 16 Microsoft Learn Badges
- Stream: Software Development
- Introduction to GitHub (4/17/2026)
- Write your first C# code (4/17/2026)
- Introduction to .NET (4/19/2026)
- Create a new .NET project and work with dependencies (4/19/2026)
- Get started with web development using Visual Studio Code (4/19/2026)
- Introduction to .NET web development with ASP.NET Core (4/19/2026)
- Build your first ASP.NET Core web app (4/19/2026)
- Customize ASP.NET Core behavior with middleware (4/19/2026)
- Introduction to Transact-SQL (4/19/2026)
- Sort and filter results in T-SQL (4/19/2026)
- Combine multiple tables with JOINs in T-SQL (4/19/2026)
- Modify data with T-SQL (4/19/2026)
- Use a database with minimal API, Entity Framework Core, and ASP.NET Core (4/19/2026)
- Build continuous integration workflows by using GitHub Actions (4/19/2026)
- Build a containerized web application with Docker (4/19/2026)
- Host a web application with Azure App Service (4/19/2026)