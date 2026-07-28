# ts-msa — Gamified Quiz Platform

NZMSA 2026 Phase 2 project: a gamified quiz platform with points, levels, streaks,
and badges, built with a .NET 10 Web API backend and a React + TypeScript frontend.

## Live URLs

| Service  | URL |
|----------|-----|
| Frontend | https://quizfrontsa.z8.web.core.windows.net/ |
| Backend API | https://quizapi-ts-msa.azurewebsites.net/ |
| API Docs (Scalar) | https://quizapi-ts-msa.azurewebsites.net/scalar |

## Theme: Gamification

QuizQuest applies HCI gamification principles directly to the quiz-taking loop, not just as a
bolt-on: every quiz attempt awards points scaled by difficulty (Easy ×10, Medium ×20, Hard ×30
per correct answer), which roll up into a level (`Level = TotalPoints / 100`), a daily streak
that resets if a day is missed, and badges auto-awarded at milestones (first quiz, point
thresholds, 7-day streak, perfect score). A public leaderboard ranks users by total points,
turning individual progress into social competition — the core mechanism gamification uses to
drive engagement and return visits.

## What Makes This Project Worth Highlighting

- **No boolean flags in the data model** — `Question.CorrectOptionId` is a nullable FK rather
  than an `IsCorrect` bool on `Option`, avoiding an ambiguous multi-row "which one is correct"
  state.
- **Badge awarding is transactional and race-safe** — badges are checked and granted in the same
  `SaveChanges` pass that records the attempt, so the triggering attempt is always visible to the
  badge-requirement check (see commit history for the fix that made this correct).
- **Auth identity never trusts the client** — `POST /api/attempts` reads the user id from the
  validated JWT claim, not the request body, so a caller cannot submit an attempt on someone
  else's behalf even if they tamper with the payload.

## Advanced Features Implemented

> Only the features listed here count toward marking — implemented but unlisted features will
> not be assessed.

- [x] **Security Measures** (2 implemented, both in `back/Controllers/AuthController.cs` and `back/Program.cs`):
  - **Password hashing (BCrypt)** — user passwords are never stored or compared in plaintext;
    `BCrypt.Net.BCrypt.HashPassword` is used on registration and `.Verify` on login. This matters
    because the `Users` table is the one place in this app where a data breach would directly
    expose real user credentials — hashing means a stolen database doesn't hand over passwords.
  - **Rate limiting** — a fixed-window limiter caps `/api/auth/*` at 5 requests/minute and the
    rest of the API at 60/minute (`app.UseRateLimiter()` in `Program.cs`). This blunts credential
    brute-forcing against login specifically, while the looser general limit protects the API
    from casual abuse without affecting normal usage.
  - Additionally, JWT bearer authentication + `[Authorize]` gates quiz-attempt submission so
    points can only be earned by an authenticated user, not spoofed via the request body.
- [x] **State Management Library (Zustand)** — `front/src/store/authStore.ts` holds the signed-in
  user's session (JWT + id + username) with `zustand/middleware persist`, so a refresh doesn't
  log the user out. Chosen over Redux for its minimal boilerplate on a single-slice app this size.
- [x] **Theme Switching** — a light/dark toggle (`front/src/ThemeToggle.tsx`) swaps a `data-theme`
  attribute on `<html>`, driven entirely by CSS custom properties already used throughout
  `index.css`; the choice persists in `localStorage` and defaults to the OS preference on first
  visit.

## Self-Reflection

If I were to start this project again, I'd build authentication in from the first commit instead
of retrofitting it once the CRUD surface already existed — `QuizPage.tsx` originally hard-coded a
demo user id with a `TODO` to replace it later, which is exactly the kind of shortcut that's easy
to forget about under deadline pressure. I'd also reach for a real hosted database (Azure SQL or
Postgres) instead of SQLite on App Service from day one; the ephemeral storage on every redeploy
was a known limitation I accepted early and then had to keep working around.

## Tech Stack

- **Backend**: .NET 10 Web API, EF Core, SQLite, Scalar API docs, JWT auth, BCrypt, rate limiting
- **Frontend**: React, TypeScript, Vite, React Router, Zustand
- **CI/CD**: GitHub Actions (path-filtered) → Azure App Service (backend, Kudu ZipDeploy) and Azure Blob Storage static website (frontend)

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
