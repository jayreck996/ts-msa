## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Kudu Deploy Auth — Ownership + Drift Reference
- SCM/FTP Basic Auth policy: Azure-owned, defaults allow=false on new apps (since ~mid-2024); managed tenants may re-disable via policy. Check: az resource show ... basicPublishingCredentialsPolicies/scm --query properties.allow. Recheck first on any future 401
- Kudu publishing creds: Azure auto-generated, never user-set. username = $<appname> (leading $), password random at creation. list-publishing-credentials READS current values; Azure may rotate them -> stored gh secrets go stale
- If a future 401: (1) is SCM basic auth still true? (2) re-copy fresh creds into gh secrets. Workflow's '$'-safe env-var handling is permanent, no need to touch again
- FTP basic auth left at default false (only SCM needed for ZipDeploy) — expected, not a bug

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Backend Deploy — Full 401 Fix + Azure /home Persistence
Three independent things must ALL be right for Kudu ZipDeploy to succeed:

| Requirement | Check | Fix |
|-------------|-------|-----|
| SCM Basic Auth enabled | az resource show ... basicPublishingCredentialsPolicies/scm --query properties.allow | az resource update ... --set properties.allow=true |
| Fresh Kudu creds in secrets | live 401 despite basic auth on | az webapp deployment list-publishing-credentials -> gh secret set AZURE_WEBAPP_USERNAME/PASSWORD --body |
| Username '$'-safe in workflow | username = $quizapi-ts-msa (leading $) | pass via env: KUDU_USER/KUDU_PASS, use -u "$KUDU_USER:$KUDU_PASS" (bash does not re-expand env-var values) |

- gh secret set: use --body <value>, NOT stdin pipe (PowerShell pipe can append CR/newline into the secret)
- Verify live: Invoke-WebRequest https://quizapi-ts-msa.azurewebsites.net/api/quizzes -> 200
- AZURE PERSISTENCE (correction): App Service /home is PERSISTENT storage; only /tmp is ephemeral. quiz.db under content root survives redeploys, so DbSeeder no-ops when prod DB already has rows. Earlier "SQLite ephemeral on Azure" note only holds if the db lives in /tmp (it doesn't here)

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Deploy Auth — Kudu Creds Refresh Recipe (recurrence)
- backend.yml auths Kudu ZipDeploy with GitHub secrets AZURE_WEBAPP_USERNAME + AZURE_WEBAPP_PASSWORD (from scmUri, NOT publish-profile XML)
- Symptom of stale creds: curl (22) HTTP 401 at Deploy step, build/tests green
- Refresh: az webapp deployment list-publishing-credentials --name quizapi-ts-msa --resource-group rg-ts-msa
  -> scmUri = https://USER:PASS@quizapi-ts-msa.scm.azurewebsites.net
  -> $uri=[Uri]$creds.scmUri; user=$uri.UserInfo.Split(':')[0]; pass=$uri.UserInfo.Split(':')[1]
  -> gh secret set AZURE_WEBAPP_USERNAME / AZURE_WEBAPP_PASSWORD --repo jayreck996/ts-msa
- If still 401: SCM Basic Auth Publishing Credentials disabled -> re-enable (Portal > quizapi-ts-msa > Configuration > General settings) or az resource update .../basicPublishingCredentialsPolicies/scm allow=true
- Re-run without new commit: gh run rerun <id> --repo jayreck996/ts-msa, or workflow_dispatch (both workflows have it)
- Frontend test note: components using react-router <Link>/<NavLink> must have their tests wrapped in <MemoryRouter> or they throw on null router context

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Quiz-Play Feature — Files + Flow Reference
- New route /quizzes/:id -> front/src/pages/QuizPage.tsx (added to App.tsx)
- Quizzes.tsx cards wrapped in Link -> /quizzes/:id (now clickable)
- Score computed client-side (correctOptionId is exposed by GET /api/questions), submitted via POST /api/attempts
- Current player: hardcoded CURRENT_USER_ID=1 (seeded demo) with // TODO(auth) — swap when JWT lands

```text
Play-a-quiz flow — /quizzes/:id (QuizPage.tsx)
└── click quiz card (Quizzes.tsx → Link /quizzes/:id)
    ├── load: api.getQuiz(id) + api.getQuestions(id)
    ├── select one option per question → answers{qId: optId}
    ├── Submit (enabled only when all answered)
    │   ├── score = count(answers[q] === q.correctOptionId)
    │   └── api.submitAttempt({ userId:1(demo), quizId, score, completedAt })
    └── results (score/total, +pointsEarned) → links back to Quizzes / Leaderboard

Badge award gap — AttemptsController.AwardBadges
├── points_100 / points_500 / streak_7 → in-memory user fields ✅
└── first_quiz / perfect_score → DB query before SaveChanges ✗ (attempt not yet persisted)
```

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: DbSeeder.cs — Code Seeder Implementation
- File: back/Data/DbSeeder.cs, static DbSeeder.Seed(AppDbContext db); called from Program.cs in startup scope (replaced bare EnsureCreated)
- Idempotent: calls EnsureCreated() then returns early if db.Quizzes.Any() — safe on every boot, survives ephemeral-SQLite redeploys
- Seed set: 3 categories (General Knowledge, Science, Programming); 3 quizzes (World Capitals/Easy, Basic Science/Medium, Programming Fundamentals/Hard); 9 questions x 4 options; 5 badges (first_quiz, points_100, points_500, streak_7, perfect_score); 3 users (demo, alice 230pts Lv2, bob 280pts Lv2)
- Circular FK handling: Question.CorrectOptionId <-> Option.QuestionId — imperative order (add question -> SaveChanges -> add options -> SaveChanges -> set CorrectOptionId -> SaveChanges); avoids EF HasData hardcoded-id pain
- User PasswordHash seeded as "" for now (auth not built yet — set when JWT lands)
- Verification pending: blocked by elevated stray QuizApi.exe holding bin lock + port 5289

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: DB Seeding — EnsureCreated Seeds Nothing
- GET /api/quizzes and /api/categories both return [] on this quiz.db — DB is schema-only, no rows
- Program.cs calls Database.EnsureCreated() = creates schema, seeds ZERO data
- Earlier Bob/Charlie/Alice + 3-quiz data lived in a different quiz.db populated by hand via API POSTs; not reproducible on a fresh DB
- SQLite is ephemeral on Azure App Service: every redeploy/restart wipes the DB → live demo shows "no quizzes"; manual POST-seeding does NOT survive
- Correct fix: seed in code (EF HasData or a startup seeder) — fixes both local and deployed demo on any fresh DB
- Minor cleanup pending: front/src/api.ts hardcodes fallback 'http://localhost:5000' (stale; harmless while .env sets 5289)

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Auth Approach Decision — Homegrown JWT

Infra context: .NET API on Azure App Service (Linux, ephemeral SQLite) + React on Azure Blob static site; Azure for Students (service principals already blocked). User model already has passwordHash.

| Option | Fit | Verdict |
|--------|-----|---------|
| JWT in .NET API (register/login, BCrypt hash, issue token, [Authorize]) | Uses existing User.passwordHash; zero new cloud services; no subscription limits | ✅ chosen — simplest |
| ASP.NET Core Identity | Adds tables/machinery not needed for a demo | overkill |
| Entra External ID / Azure AD B2C | Managed but heavy setup + likely Students-subscription wall | avoid |

- Pieces: /api/auth/register + /api/auth/login (BCrypt), JWT issuance, [Authorize] on protected controllers, frontend token store + Authorization header
- Seed a demo login in the code seeder (ephemeral SQLite wipes users on redeploy) so graders can always log in

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Cross-Project Port Map — ts-msa vs ts-recruitment-dev

| Project | Backend | Frontend (Vite) |
|---------|---------|-----------------|
| ts-recruitment-dev (Node/Express) | 5000 (backend/.env PORT=5000) | 5173 |
| ts-msa (.NET) | 5289 | 5173 |

- Backends do not conflict: 5000 vs 5289
- Frontends share Vite default 5173 — only a conflict if run simultaneously; projects run sequentially so no real clash
- Discipline: stop.ps1 / close dev windows before switching projects (avoid stray process holding a port)

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Local Env Port Config — Aligned to 5289

| Component | File | Port / URL | Status |
|-----------|------|-----------|--------|
| Backend (http profile) | back/Properties/launchSettings.json | http://localhost:5289 | source of truth |
| Backend (https profile) | back/Properties/launchSettings.json | https://localhost:7048;http://localhost:5289 | source of truth |
| Frontend API target | front/.env | http://localhost:5289 | ✅ fixed (was 5000) |
| Launcher echo | start.ps1 | http://localhost:5289 | ✅ fixed (was 5000) |
| Stop script target | stop.ps1 | port 5289 + QuizApi name fallback | ✅ fixed (was 5000) |
| Frontend dev server | front (Vite) | http://localhost:5173 | ✅ unchanged |
| Prod API placeholder | front/.env.example | https://your-api.azurewebsites.net | left as-is (correct) |

- start.ps1 / stop.ps1 rewritten with section comments and clearer per-port log lines
- stop.ps1 now has a by-name QuizApi fallback kill for detached/elevated strays the port lookup misses

## ASSET:-jay 2026-07-13 -> NZMSA 2026-Phase-2: Local Env Port Config — Backend/Frontend Mismatch

| Component | File | Port / URL configured | Matches backend? |
|-----------|------|----------------------|------------------|
| Backend (http profile) | back/Properties/launchSettings.json | http://localhost:5289 | — (source of truth) |
| Backend (https profile) | back/Properties/launchSettings.json | https://localhost:7048;http://localhost:5289 | — (source of truth) |
| Frontend API target | front/.env | http://localhost:5000 | ❌ mismatch |
| Launcher echo | start.ps1 | http://localhost:5000 | ❌ stale text |
| Frontend dev server | front (Vite) | http://localhost:5173 | ✅ runs fine |
| Stray process | QuizApi.exe (PID 42816) | listening on 5289 | ⚠️ blocks fresh start |

- Backend actually serves on 5289; frontend + launcher scripts all point to 5000 → API calls fail even when everything "starts"
- No fix applied yet

## ASSET:-jay 2026-07-02 -> NZMSA 2026-Phase-2: .gitignore Reverted
- Removed `could/` line from .gitignore (added earlier same day, now reversed)
- could/ folder (ISSUE-2026Q2.md, ASSET-2026Q2.md) now untracked-but-includable — will be git add'ed and pushed

## ASSET:-jay 2026-07-02 -> NZMSA 2026-Phase-2: .gitignore Updated
- Added `could/` line to .gitignore (repo root)
- Existing entries: bin/, obj/, *.db, *.user, .vs/, node_modules/, front/.env, dist/
- could/ now formally excluded, not just conventionally untracked

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend Deploy — Flat Zip for Linux App Service
- Linux App Service ZipDeploy requires flat zip (files at root, not inside a subdirectory)
- Wrong: `zip -r publish.zip ./publish` → creates `publish/QuizApi.dll` inside zip
- Right: `cd publish && zip -r ../publish.zip .` → creates `QuizApi.dll` at zip root
- Debug tip: add `--verbose` to curl and remove `--fail --silent` to see HTTP response body in CI logs

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend Deploy — Kudu ZipDeploy via curl
- Endpoint: `https://quizapi-ts-msa.scm.azurewebsites.net/api/zipdeploy?isAsync=false`
- Method: POST with `Content-Type: application/zip` and `--data-binary @publish.zip`
- Auth: HTTP basic auth with site-level credentials from scmUri
- Full command: `curl -X POST <endpoint> -u "$USER:$PASS" -H "Content-Type: application/zip" --data-binary @publish.zip --max-time 300 --fail --silent --show-error`
- Required for Linux App Service — `azure/webapps-deploy@v3` is Windows-only (uses MSDeploy)

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Backend Deploy — Real Kudu Credentials Source
- Wrong source: publish profile XML userName/userPWD → 401 Unauthorized on Kudu REST API
- Right source: `az webapp deployment list-publishing-credentials --name quizapi-ts-msa --resource-group rg-ts-msa` → returns `scmUri` with credentials embedded
- scmUri format: `https://USERNAME:PASSWORD@quizapi-ts-msa.scm.azurewebsites.net`
- Extract: `$uri = [Uri]$creds.scmUri; $user = $uri.UserInfo.Split(':')[0]; $pass = $uri.UserInfo.Split(':')[1]`
- Test locally: `Invoke-WebRequest -Uri $creds.scmUri -Method GET` → 200 confirms credentials work

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: App Service OS Check
- Command: `az webapp show --name quizapi-ts-msa --resource-group rg-ts-msa --query "{kind:kind, linuxFxVersion:siteConfig.linuxFxVersion}"`
- Result: `kind: app,linux` | `linuxFxVersion: DOTNETCORE|10.0`
- Signal in CI logs: `warning: Failed to get app runtime OS` — azure/webapps-deploy@v3 cannot identify Linux runtime
- Service principal creation blocked on Azure for Students: `az ad sp create-for-rbac` → `Insufficient privileges`

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Frontend Test — MemoryRouter Package Mismatch
- Home.tsx imports `Link` from `react-router-dom`; test must import `MemoryRouter` from `react-router-dom` (not `react-router`)
- Using `react-router` MemoryRouter with `react-router-dom` Link causes context null — `basename` destructure error
- Fix: `import { MemoryRouter } from 'react-router-dom'` in Home.test.tsx
- Rule: import test router wrapper from same package the component uses

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: GitHub Actions Workflow IDs + Secrets
- Backend workflow ID: 302436807 | Frontend workflow ID: 302436808
- AZURE_WEBAPP_PUBLISH_PROFILE: App Service publish profile XML (downloaded via az webapp deployment list-publishing-profiles --xml)
- AZURE_STORAGE_KEY: quizfrontsa storage account key (downloaded via az storage account keys list)
- Both secrets set via `az ... | gh secret set <name> --repo jayreck996/ts-msa`
- Trigger: path-filtered — back/** fires backend only, front/** fires frontend only; workflow_dispatch available for manual trigger

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Actual Deployment Stack (Corrected)
- Backend: Azure App Service F1 | name: quizapi-ts-msa | rg: rg-ts-msa | URL: https://quizapi-ts-msa.azurewebsites.net
- Frontend: Azure Blob Storage static website | account: quizfrontsa | rg: rg-ts-msa | URL: https://quizfrontsa.z8.web.core.windows.net
- GitHub Actions secrets: AZURE_WEBAPP_PUBLISH_PROFILE (App Service) | AZURE_STORAGE_KEY (Blob Storage)
- frontend.yml deploy step: `az storage blob upload-batch --account-name quizfrontsa --source front/dist --destination '$web' --overwrite`
- Previous ASSET (2026-06-24) listed "Azure Static Web Apps" — was the plan, not what was actually deployed; now corrected

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Frontend Deploy — Vercel
- SWA blocked: RequestDisallowedByAzure on Azure for Students — both CLI and Portal confirmed
- Vercel CLI deploy: `cd front && npm run build && npx vercel --prod` (one-time link prompt, then live URL)
- Vercel dashboard deploy: vercel.com → Import jayreck996/ts-msa → Root Directory: front → Env: VITE_API_URL=https://quizapi-ts-msa.azurewebsites.net → Deploy
- frontend.yml: tests-only (no deploy step) — runs npm ci → npm test on push to front/**
- backend.yml: full pipeline — dotnet test → dotnet publish → azure/webapps-deploy@v3 on push to back/**

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: GitHub Actions Workflows
- .github/workflows/backend.yml: ubuntu-latest | dotnet 10.x | path filter back/** | steps: checkout → setup-dotnet → dotnet test → dotnet publish -c Release -o ./publish → azure/webapps-deploy@v3 (app-name: quizapi-ts-msa)
- .github/workflows/frontend.yml: ubuntu-latest | node 20 | path filter front/** | steps: checkout → setup-node (npm cache) → npm ci → npm test → npm run build (VITE_API_URL=https://quizapi-ts-msa.azurewebsites.net) → Azure/static-web-apps-deploy@v1 (skip_app_build: true, app_location: front/dist)
- GitHub secrets required: AZURE_WEBAPP_PUBLISH_PROFILE (set) | AZURE_STATIC_WEB_APPS_API_TOKEN (pending SWA creation)
- AZURE_WEBAPP_PUBLISH_PROFILE: downloaded via `az webapp deployment list-publishing-profiles --xml` and piped to `gh secret set`
- SWA token source: Azure Portal → Static Web App resource → Manage deployment token → pipe to `gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --repo jayreck996/ts-msa`
- Both workflows: path-filtered (backend changes don't trigger frontend deploy and vice versa) + workflow_dispatch for manual trigger

## ASSET:-jay 2026-06-26 -> NZMSA 2026-Phase-2: Azure Deployment Commands
- Backend (App Service): `cd back && dotnet publish -c Release -o ./publish` → `az webapp deploy --resource-group <rg> --name quizapi-ts-msa --src-path ./publish --type zip`
- Backend shorthand: `az webapp up --name quizapi-ts-msa --runtime "DOTNET|10.0"` (builds + deploys in one step)
- SQLite note: quiz.db on ephemeral App Service filesystem — wiped on each redeploy
- Frontend build: set `VITE_API_URL=https://quizapi-ts-msa.azurewebsites.net` in front/.env → `cd front && npm run build` → outputs to front/dist/
- Frontend deploy via SWA CLI: `npm i -g @azure/static-web-apps-cli` → `swa deploy ./dist --deployment-token <token>`
- SWA token location: Azure Portal → Static Web App resource → Manage deployment token
- Frontend deploy via Portal: link GitHub repo in SWA resource → Azure auto-commits .github/workflows/azure-static-web-apps-*.yml → every push to main triggers deploy
- No CI/CD workflow in repo yet (no .github/workflows/) — Portal-link method is easiest path

## ASSET:-jay 2026-06-25 -> NZMSA 2026-Phase-2: Live Seed Data
- Non-nullable EF nav props → implicit [Required] → fixed to nullable (`Type?`) in 5 models; redeployed
- `ReferenceHandler.IgnoreCycles` added to Program.cs AddJsonOptions → fixes 500 circular ref on attempt POST
- Deployed twice to Azure App Service F1 (linux-x64, 3.91 MB zip each)
- Seed data live: categories 1–4, badges 1–5, users 1–3 (alice/bob/charlie), quizzes 1–3, questions 1–9, options 1–36, attempts 1–9
- Leaderboard populated; badge auto-award logic verified (streak/points triggers in AttemptsController.AwardBadges)
- could/ docs removed from GitHub — now local-only; no gitignore needed (direct API-push workflow)

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