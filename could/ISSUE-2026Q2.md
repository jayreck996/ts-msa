## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Deploy — Backend 401 (Kudu creds stale, RECURRENCE) + Frontend Test Fix
- Pushed feat commit -> both CI deploys ran, both failed at DIFFERENT stages
- Backend: build + tests passed; FAILED only at Deploy via Kudu ZipDeploy -> curl (22) HTTP 401 Unauthorized
  - Same failure + cause as 2026-06-26 "Credentials 401": scmUri Kudu creds stored as GitHub secrets AZURE_WEBAPP_USERNAME / AZURE_WEBAPP_PASSWORD have gone stale (Azure rotates SCM publishing creds; or SCM Basic Auth disabled on quizapi-ts-msa)
  - NOT a code issue — deploy-auth only
  - Fix recipe (from ASSET 2026-06-26): az webapp deployment list-publishing-credentials --name quizapi-ts-msa --resource-group rg-ts-msa -> extract user/pass from scmUri -> gh secret set AZURE_WEBAPP_USERNAME / AZURE_WEBAPP_PASSWORD -> if still 401, re-enable SCM Basic Auth Publishing Credentials in Portal -> gh run rerun (or workflow_dispatch)
  - BLOCKER: needs authenticated az session (interactive az login) — deferred to Jay
- Frontend: FAILED at Run tests — my QuizPage work added <Link> to Quizzes.tsx, which broke Quizzes.test.tsx (rendered <Quizzes/> without Router -> "Cannot destructure 'basename' of useContext() as null")
  - Fix: wrap test renders in <MemoryRouter> (renderQuizzes helper); 9/9 tests pass locally -> pushed to re-trigger frontend deploy

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Quiz-Play Flow Live + Two Flags (badge bug, auth placeholder)
- Added clickable quiz-taking page: Quizzes cards -> /quizzes/:id -> QuizPage.tsx (load questions, select answers, submit, results). Verified: POST /api/attempts score 3 -> +30 pts, streak 1, leaderboard updates
- Flag 1 (backend bug): first_quiz + perfect_score badges not awarded on the triggering attempt — badge check queries DB before SaveChanges
- Flag 2 (frontend placeholder): QuizPage hardcodes CURRENT_USER_ID=1 (seeded demo) until JWT auth lands

```text
Badge award flow — POST /api/attempts (AttemptsController.Submit)
└── request arrives {userId, quizId, score}
    ├── lookup quiz → compute PointsEarned (score × difficulty multiplier)
    ├── db.QuizAttempts.Add(attempt)          ← attempt in memory, NOT saved
    ├── lookup user
    │   ├── user.TotalPoints += PointsEarned   ← in-memory, visible now
    │   ├── recompute Level + CurrentStreak     ← in-memory, visible now
    │   └── AwardBadges(user)
    │       ├── points_100 / points_500 → reads user.TotalPoints ✅ works
    │       ├── streak_7             → reads user.CurrentStreak ✅ works
    │       ├── first_quiz    → db.QuizAttempts.CountAsync(...) ✗ counts 0 (attempt unsaved)
    │       └── perfect_score → db.QuizAttempts.AnyAsync(...)   ✗ no match (attempt unsaved)
    └── db.SaveChanges()                        ← attempt persisted HERE, too late for the 2 badges

Play-a-quiz flow — /quizzes/:id (QuizPage.tsx)
└── user clicks a quiz card (Quizzes.tsx → Link /quizzes/:id)
    ├── load: api.getQuiz(id) + api.getQuestions(id)
    ├── user selects one option per question → answers{qId: optId}
    ├── click Submit (enabled only when all answered)
    │   ├── score = count(answers[q] === q.correctOptionId)
    │   └── api.submitAttempt({ userId: CURRENT_USER_ID=1, quizId, score, completedAt })
    │       └── ⚠ userId hardcoded to seeded "demo" — // TODO(auth) swap for logged-in user
    └── show results (score/total, +pointsEarned) → links back to Quizzes / Leaderboard
```

- Fix options for Flag 1: (A) move AwardBadges after SaveChanges, or (B) count in-memory (existing + 1) — not yet applied

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Code Seeder Added — Build Blocked by Elevated Stray
- Implemented back/Data/DbSeeder.cs: idempotent startup seeder (no-ops if Quizzes.Any()); seeds 3 categories, 3 quizzes (Easy/Medium/Hard), 9 questions x 4 options, 5 badges, 3 demo users (demo/alice/bob)
- Handles the Question.CorrectOptionId <-> Option.QuestionId circular FK imperatively: save options first, then set CorrectOptionId (why not EF HasData — HasData needs hardcoded ids for the cycle)
- Program.cs: replaced bare EnsureCreated() with DbSeeder.Seed(db) (seeder calls EnsureCreated internally)
- C# compiles clean — build FAILED only at final copy step: elevated stray QuizApi.exe (PID 61012) locks bin output + holds port 5289; Stop-Process denied from normal shell (3rd time this session)
- Blocker for verification: need PID 61012 killed (close its elevated -NoExit window, or Stop-Process from an elevated shell) before rebuild + run + curl /api/quizzes check
- quiz.db (empty) deleted, ready for a clean re-seed once the process is gone

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: /quizzes Empty — No Seed Data + Auth Plan
- Symptom: http://localhost:5173/quizzes shows "no quizzes"
- Root cause: NOT a port/frontend bug — GET /api/quizzes and /api/categories both return [] (DB genuinely empty)
- Program.cs only calls EnsureCreated() (schema only, zero rows); earlier seeded data lived in a different quiz.db populated manually via API POSTs, not this one
- Cloud impact: SQLite is ephemeral on Azure App Service — every redeploy/restart wipes the DB, so live demo also shows "no quizzes"; manual POST-seeding will not survive
- Fix plan: seed IN CODE (EF HasData or startup seeder) so both local and deployed demo are populated on a fresh DB
- Minor: front/src/api.ts still hardcodes fallback 'http://localhost:5000' (stale; harmless while .env sets 5289) — clean when touched
- Login plan: add auth. Simplest for our infra = homegrown JWT inside the .NET API (User model already has passwordHash) — register/login endpoints, BCrypt hash, JWT + [Authorize]; no new Azure services (avoids Azure-for-Students restrictions that already blocked service principals)
- Rejected: ASP.NET Core Identity (overkill), Entra External ID / AD B2C (heavy + likely subscription wall)
- Ephemeral-DB caveat for auth: registered users also vanish on redeploy — seed a demo login in the same code seeder so graders can always log in

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Cross-Project Port Overlap with ts-recruitment-dev — Non-Issue
- ts-recruitment-dev (Node backend) runs on 5000; its frontend on Vite 5173 — same 5173 as ts-msa frontend
- Latent hazard now closed: ts-msa's OLD front/.env=5000 pointed straight at ts-recruitment-dev's backend — if that API were up, quiz frontend would have silently hit the WRONG backend (recruitment API answering quiz calls) instead of connection-refused; fixing ts-msa to 5289 removed this
- Both projects run SEQUENTIALLY (never simultaneously) per Jay — so shared 5173 frontend port never actually collides; no port change needed
- Backends never conflict anyway: 5000 (recruitment) vs 5289 (ts-msa)
- Only discipline required: run stop.ps1 (or close dev windows) before switching projects, so no stray process holds a port into the next session

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Local Env Port Mismatch — Resolved
- Fixed the 5000-vs-5289 mismatch by aligning everything to the real dev port 5289 (launchSettings.json is the source of truth; Program.cs sets no port)
- front/.env: VITE_API_URL 5000 -> 5289
- start.ps1: backend echo 5000 -> 5289; added section comments
- stop.ps1: backend port 5000 -> 5289; added by-name QuizApi fallback kill (port lookup misses detached/elevated strays); cleaner log lines
- Left front/.env.example as-is (Azure prod placeholder URL — correct)
- Note: 5000 was ASP.NET Core's default when launchSettings.json is bypassed (published/prod); local dev overrides to 5289
- Caveat: stray QuizApi.exe PID 42816 was elevated — Stop-Process denied from normal shell; must close its window or run stop.ps1 elevated once

## ISSUE:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Local Env Startup Failure — Stray Process + Port Mismatch
- start.ps1 backend failed: "Failed to bind to address http://127.0.0.1:5289: address already in use"
- Root cause 1: orphaned QuizApi.exe (PID 42816) already listening on 5289 from a prior dotnet run that was never stopped/killed
- Root cause 2: port mismatch — back/Properties/launchSettings.json binds API to localhost:5289, but front/.env has VITE_API_URL=http://localhost:5000 and start.ps1/README both claim port 5000
- Effect: even after killing the stray process and restarting, frontend would call :5000 (nothing listening) — API requests fail
- Frontend itself started fine standalone: http://localhost:5173
- Not yet fixed — awaiting decision: align front/.env to 5289, or change launchSettings.json to 5000

## ISSUE:-jay 2026-07-11 -> NZMSA 2026-Phase-2: Submission Logistics Recap — No Attempt Limit Found
- Confirmed submission deadline: Sunday 2 August 2026, 11:59 PM NZST
- Submission form: https://forms.office.com/r/tRVKyQZVZ7 (opened July 13)
- Phase 2 spec repo: https://github.com/NZMSA/2026-Phase-2
- Office hours: Fri July 3, July 17, July 31, 7:30-8:00 PM NZST (Discord #help-software voice)
- Searched all could/should/must docs + repo for a resubmission/attempt cap — none documented; Microsoft Forms likely just records the latest response

## ISSUE:-jay 2026-07-02 -> NZMSA 2026-Phase-2: could/ Plan Reversed — Now Tracked in Remote
- Previous decision (same day): gitignore could/ to keep docs local-only
- Plan changed: could/ is meant to be pushed to remote too, not excluded
- Fix: removed `could/` line from .gitignore; ISSUE-2026Q2.md + ASSET-2026Q2.md will now be committed and pushed

## ISSUE:-jay 2026-07-02 -> NZMSA 2026-Phase-2: could/ Not Gitignored
- could/ was untracked but not explicitly gitignored — relied on never being `git add`ed
- Risk: an accidental `git add -A` or `git add .` would pull could/ into the repo, exposing local-only docs
- Fix: added `could/` to .gitignore to make the exclusion explicit and safe against accidental staging

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend CI/CD Debug — Resolution
- Both workflows green after two fixes: correct scmUri credentials + flat zip structure
- Frontend: 9/9 tests pass → blob storage deploy succeeds
- Backend: 13/13 tests pass → Kudu ZipDeploy to Linux App Service succeeds
- Key lesson: always check App Service OS (linux vs windows) before choosing deploy method

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend CI/CD Debug — Zip Structure Wrong
- `zip -r publish.zip ./publish` creates nested zip: `publish/QuizApi.dll` — App Service rejects this
- Linux App Service ZipDeploy expects flat zip: `QuizApi.dll` at root
- Fix: `cd publish && zip -r ../publish.zip .` zips contents not the folder
- Added `--verbose` to curl to expose HTTP response body in CI logs for diagnosis

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend CI/CD Debug — OS Mismatch Root Cause
- `az webapp show` returned `"kind": "app,linux"` — App Service is Linux not Windows
- `azure/webapps-deploy@v3` uses WebDeploy (MSDeploy) — Windows only; warning "Failed to get app runtime OS" was the signal
- Decision: drop the action entirely; use direct curl POST to Kudu ZipDeploy REST API
- Service principal creation blocked (Azure for Students AD restriction) — kudu basic auth was only option

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend CI/CD Debug — Credentials 401
- Tested ZipDeploy credentials locally with Invoke-WebRequest — got 401 Unauthorized
- Credentials from publish profile XML (ZipDeploy entry userName/userPWD) do not work for Kudu REST API
- `az webapp deployment list-publishing-credentials` → scmUri contains real working credentials
- Confirmed by GET to scmUri → 200 OK; extracted username/password and re-stored as GitHub secrets

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend CI/CD Debug — Publish Profile Invalid
- Error: `Deployment Failed, Error: Publish profile is invalid for app-name and slot-name provided`
- Not a build failure — a credential/authentication failure at deploy step only
- Encoding ruled out: re-stored via WriteAllText (no BOM) — still failed; encoding was not root cause
- Tests and build steps passed cleanly throughout — only deploy step failed

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Frontend CI/CD Debug — Home Test Failing
- `Home.test.tsx`: 2/9 tests failed — `TypeError: Cannot destructure property 'basename' of useContext as null`
- Root cause: `Home.tsx` imports `Link` from `react-router-dom`; test imported `MemoryRouter` from `react-router` — context mismatch between packages
- Fix: import `MemoryRouter` from `react-router-dom` to match the component's router package
- Other 7 tests (Quizzes x4, Leaderboard x3) passed from the start — only Home uses router Links

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: GitHub Actions Live
- Both workflows pushed to main and confirmed active on GitHub Actions
- Backend — Build & Deploy (ID 302436807): triggers on push to back/** | dotnet test → dotnet publish → azure/webapps-deploy@v3
- Frontend — Build & Deploy (ID 302436808): triggers on push to front/** | npm test → npm run build → az storage blob upload-batch to quizfrontsa/$web
- GitHub secrets set: AZURE_WEBAPP_PUBLISH_PROFILE | AZURE_STORAGE_KEY
- README.md pending deletion from GitHub remote (never existed locally)
- could/ confirmed local-only (untracked by git); local index cleaned after stash pop conflict

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Frontend Already Live on Blob Storage (Undocumented)
- Frontend was already deployed to Azure Blob Storage static website — never logged in docs
- URL: https://quizfrontsa.z8.web.core.windows.net (storage account: quizfrontsa, container: $web)
- Previous docs/README said "Azure Static Web Apps - pending" — incorrect; SWA was never used
- Root cause of doc gap: Blob Storage deploy was done but not logged; docs reflected the original plan (SWA) not actual state
- SWA creation attempts (CLI + Portal) were unnecessary — deployment already existed
- Lesson: always verify live Azure resources (`az resource list --resource-group rg-ts-msa`) before assessing project state

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Frontend Deployment Decision
- Azure Static Web Apps blocked on Azure for Students subscription — RequestDisallowedByAzure policy fires on both CLI and Azure Portal across all valid SWA regions
- Decision: deploy frontend via Vercel (explicitly listed as accepted option in NZMSA requirements)
- Manual deploy: `cd front && npm run build && npx vercel --prod` — Vercel detects Vite, prompts once to link project, outputs live URL
- OR: connect GitHub repo to Vercel dashboard (Root Directory: front, VITE_API_URL env var set) — Vercel handles CI on push without GitHub Actions
- frontend.yml kept as tests-only (no deploy step) — 9 Vitest tests still run on every push to front/**
- backend.yml unchanged — build + test + deploy to App Service on push to back/**

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: GitHub Actions CI/CD Setup
- Backend workflow: .github/workflows/backend.yml — triggers on push to back/** | runs dotnet test → dotnet publish → azure/webapps-deploy@v3
- Frontend workflow: .github/workflows/frontend.yml — triggers on push to front/** | runs npm test → npm run build (VITE_API_URL baked in) → Azure/static-web-apps-deploy@v1
- AZURE_WEBAPP_PUBLISH_PROFILE secret stored in GitHub (downloaded from App Service publish profile)
- AZURE_STATIC_WEB_APPS_API_TOKEN secret NOT yet set — SWA resource not yet created
- SWA CLI creation blocked by Azure for Students subscription policy (RequestDisallowedByAzure) across all valid SWA regions (eastasia, eastus2, westus2, westeurope, centralus)
- Next: create SWA resource via Azure Portal (portal.azure.com) → Manage deployment token → store token as GitHub secret → push to main triggers both pipelines

## ISSUE:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Azure Deployment Status
- Backend live: https://quizapi-ts-msa.azurewebsites.net (Azure App Service F1)
- Frontend: Azure Static Web Apps NOT yet deployed — no GitHub Actions workflow in repo, no SWA URL recorded
- SQLite (quiz.db) lives on App Service ephemeral filesystem — data will be lost on redeploy; acceptable for assessment
- Backend redeploy: `cd back && az webapp up --name quizapi-ts-msa --runtime "DOTNET|10.0"` (or dotnet publish + az webapp deploy)
- Frontend deploy: build with VITE_API_URL set → `npm run build` → `swa deploy ./dist --deployment-token <token>` OR link GitHub repo in Azure Portal (auto-generates workflow)
- Next: deploy SWA and update README Live URLs table with real URL; make repo public before submission (Aug 2)

## ISSUE:-jay 2026-06-25 -> NZMSA 2026: Phase 2 - Data Seeding Complete
- Fixed 400 on POST /api/quizzes: non-nullable EF nav props (`public Category Category { get; set; } = null!`) trigger implicit [Required] — fixed by making all nav props nullable (`Category?`, `Quiz?`, `User?`, `Badge?`, `Question?`) across Quiz.cs, Question.cs, Option.cs, QuizAttempt.cs, UserBadge.cs
- Fixed 500 on POST /api/attempts: EF identity fixup auto-links nav props in same DbContext after SaveChanges → circular JSON serialisation — fixed with `ReferenceHandler.IgnoreCycles` in AddJsonOptions
- Seeded: 3 quizzes, 9 questions, 36 options (correctOptionId set via PUT), 9 attempts across 3 users
- Live leaderboard: Bob 280pts Lv2, Charlie 280pts Lv2, Alice 230pts Lv2 — badges auto-awarded (First Quiz + Century for all 3 users)
- could/ docs removed from GitHub remote (local only) — gitignore N/A here (API-push workflow, no local git)

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Monorepo Shape + Unit Tests (Step 5)
- Repo: https://github.com/jayreck996/ts-msa (private monorepo)
- back/: .NET 10 Web API | Models (User,Category,Quiz,Question,Option,QuizAttempt,Badge,UserBadge) | Controllers (Users,Categories,Quizzes,Questions,Options,Attempts,Badges,Leaderboard) | EF Core SQLite | Scalar at /scalar | CORS enabled | DB auto-created on startup
- front/: React + TypeScript + Vite | React Router | Pages: Home, Quizzes, Leaderboard | api.ts typed fetch client | VITE_API_URL env var | staticwebapp.config.json (Azure SPA routing)
- could/: ISSUE-2026Q2.md + ASSET-2026Q2.md (this doc)
- Next: Step 5 - unit tests for back/ (xUnit) and front/ (Vitest)

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Monorepo Structure + Azure Deployment Plan
- Repo structure: ts-msa/ monorepo with back/ (API) and front/ (React) folders
- back/ renamed from QuizApi/ - .NET 10 Web API, EF Core, SQLite, Scalar at /scalar
- front/ to scaffold: React + TypeScript + Vite + staticwebapp.config.json (SPA routing fallback)
- Deployment: Azure Static Web Apps (front) + Azure App Service free F1 tier (back)
- Azure Static Web Apps: free tier | GitHub Actions CI/CD auto-wired on push | global CDN
- API base URL in .env for local/Azure swap: VITE_API_URL=http://localhost:5000 (local)
- staticwebapp.config.json required for React Router - prevents 404 on direct URL access
- MSA alignment: Microsoft program + Microsoft Azure = strong fit for assessment
- Next: scaffold front/ with Vite + React + TypeScript wired for Azure Static Web Apps

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Repo Created + Data Model Finalised
- Repo: https://github.com/jayreck996/ts-msa (private, jayreck996)
- Description: NZMSA 2026 Phase 2 - Gamified Quiz Platform (.NET 10 + React/TypeScript)
- AI assistance: Claude explicitly named and allowed in Phase 2 README - log usage in /specs folder
- Data model finalised (no booleans):
  - User: id, username, email, passwordHash, totalPoints, level, currentStreak, longestStreak, createdAt
  - Category: id, name
  - Quiz: id, title, description, difficulty (Easy/Medium/Hard), categoryId, createdAt
  - Question: id, quizId, text, points, correctOptionId (FK to Option, nullable)
  - Option: id, questionId, text
  - QuizAttempt: id, userId, quizId, score, pointsEarned, completedAt
  - Badge: id, name, description, requirement
  - UserBadge: id, userId, badgeId, earnedAt
- Gamification: points by difficulty (Easy=10/Med=20/Hard=30) | level per 100pts | daily streak | badges on milestones
- Advanced features planned: badge system | leaderboard | streak tracking
- Next: scaffold .NET 10 backend locally

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Submission Checklist + Timeline
- Timeline: Now-July 13 (build) | July 13 form opens | Aug 2 11:59 PM NZST deadline
- Step 1: Pick gamification idea (study tracker / habit app / quiz platform)
- Step 2: Design data models (users, points, badges, streaks, levels)
- Step 3: Build .NET 10 backend - CRUD endpoints + EF Core + SQLite
- Step 4: Build React + TypeScript frontend (MUI/Mantine/Tailwind, mobile-friendly)
- Step 5: Add unit tests - both frontend and backend
- Step 6: Pick and implement 3+ advanced features - list them in README
- Step 7: Deploy app (Render / Railway / Azure / Vercel / Netlify)
- Step 8: Write /specs folder - document AI prompts + design decisions throughout
- Step 9: Enable Scalar API docs at /scalar endpoint
- Step 10: Make repo + all links public, verify in Incognito before submitting
- Submit: https://forms.office.com/r/tRVKyQZVZ7 (opens July 13)

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Software Development Action Plan
- Ref: https://github.com/NZMSA/2026-Phase-2
- Stream: Software Development (same as Phase 1)
- Workload: 3-4 hrs/week | builds on Phase 1 | individual project
- Project idea required: gamification app (e.g. study tracker, habit app, quiz platform)
- Must include: .NET 10 backend (CRUD + EF Core + SQLite) | React + TypeScript frontend | unit tests (both) | deployment | Scalar API docs | 3+ advanced features | /specs folder (AI prompts + design decisions)
- Demo scaffold available at repo (leaderboard app) - NOT submission-worthy, reference only
- Status: Awaiting Phase 2/3 eligibility confirmation (submitted late June 15)
- Submission form opens: Week 4 - July 13, 0:00 NZST -> https://forms.office.com/r/tRVKyQZVZ7
- Submission deadline: Sunday 2 August 2026 11:59 PM NZST
- Office Hours: Fri July 3, July 17, July 31 - 7:30-8:00 PM | Discord #help-software voice
- Support: Discord channel #help-software

# ISSUE Log - MSA (migrated from -jay)

## ISSUE:-jay 2026-06-15 09:38 -> NZMSA 2026: Phase 2/3 Eligibility Form - Ready to Submit
- Form: MSA 2026 Phase 2/3 Eligibility Proof Submission (temp form, deadline 14 Jun - submitting late)
- Field 1 First Name: Jay
- Field 2 Last Name: Reck
- Field 3 Email: jayreck996@gmail.com
- Field 4 Student ID: 20120687
- Field 5 Stream: Software Development
- Field 6 Eligibility Proof: https://drive.google.com/file/d/1xTH3ADpmICWiFCjTkehEyAxDy5XrBbit/view?usp=sharing
- Field 7 Comments: Submitting one day past the deadline (June 15) - I was unaware the form would close. I have completed all Phase 1 modules and have AUT enrollment proof for 2025 ready. Please consider my late submission. Thank you.
- Document: AUT Transcript of Official Academic Record - confirms name, student ID 20120687, AUT, courses in 2025 (S1+S2), credit points

## ISSUE:-jay 2026-06-15 09:38 -> NZMSA 2026: Phase 2/3 Eligibility Proof Submission Steps
- Status: Phase 1 CONFIRMED COMPLETE (MSA email 11 Jun 2026)
- Deadline: 14 Jun 2026 11:59 PM NZST - MISSED by ~10 hours
- Step 1: Log into my.aut.ac.nz > My Documents > download Statement Invoice Jan 2025 as PDF
- Step 2: Upload PDF to Google Drive > Share > Anyone with the link (Viewer) > copy link
- Step 3: Click Phase 2/3 Eligibility Proof button directly from MSA email > paste Google Drive link
- Step 4 (if form closed): Message MSA Discord (discord.gg/2WCtnQDjEf) - explain missed by <1 day, Phase 1 complete, document ready - ASK FOR LATE SUBMISSION LINK
- Document must show: full name | AUT | student ID 20120687 | full-time status | year 2025
- Best doc: Statement Invoice Jan 2025 or Final Academic Results Jul/Nov 2025

## ISSUE:-jay 2026-06-15 09:38 -> NZMSA 2026-Phase-1: Corrected Programme + Progress
- Programme: MSA NZ 2026 Phase 1 - Software Development Stream (corrected from 2024)
- Ref: https://github.com/NZMSA/2026-Phase-1
- Deadline: Friday 29 May 2026 11:59pm - MISSED by 17 days
- Action: Attempt late submission via https://forms.office.com/r/6nssmi68X6
- Week 1 (7/7 COMPLETE): Introduction to GitHub | Write your first C# code | Introduction to .NET | Create a new .NET project | Introduction to .NET web development with ASP.NET Core | Build your first ASP.NET Core web app | Customize ASP.NET Core behavior with middleware
- Week 2 (1/2): Get started with web development using VS Code [done] | React Tic-Tac-Toe tutorial [pending - no badge]
- Week 3 (unknown): TypeScript for the New Programmer | TypeScript for JavaScript Programmers - no MS Learn badges required
- Week 4 (unknown): TypeScript Handbook (Basics, Everyday Types, Functions, Object Types, Classes) - no MS Learn badges required
- Week 5 (4/4 COMPLETE): Introduction to Transact-SQL | Sort and filter results in T-SQL | Combine multiple tables with JOINs in T-SQL | Modify data with T-SQL
- Week 6 (4/5): Use a database with minimal API EF Core ASP.NET Core [done] | Build CI workflows by GitHub Actions [done] | Build a containerized web application with Docker [done] | Host a web application with Azure App Service [done] | Introduction to software testing concepts [pending]

## ISSUE:-jay 2026-06-15 09:38 -> NZMSA 2024-Phase-1: Software Development Progress
- Programme: MSA NZ 2024 Phase 1 - Software Development Stream
- Ref: https://github.com/NZMSA/2024-Phase-1
- Status: 16 badges completed
- Week 1: Introduction to GitHub [done] | Write your first C# code [done] | Introduction to .NET [done] | Create a new .NET project and work with dependencies [done]
- Week 2: Get started with web development using Visual Studio Code [done]
- Week 5: Introduction to Transact-SQL [done] | Sort and filter results in T-SQL [done] | Combine multiple tables with JOINs in T-SQL [done] | Modify data with T-SQL [done]
- Week 6: Introduction to .NET web development with ASP.NET Core [done] | Build your first ASP.NET Core web app [done] | Customize ASP.NET Core behavior with middleware [done] | Use a database with minimal API, EF Core, and ASP.NET Core [done] | Build a containerized web application with Docker [done] | Build CI workflows by using GitHub Actions [done] | Host a web application with Azure App Service [done]
- Pending: Week 1 (debug .NET apps, create web API with controllers) | Week 2 (accessibility, Node.js x2, React x3) | Week 3-4 (TypeScript x8)