## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Remaining Tasks

| # | Task | Status |
|---|------|--------|
| 1 | Gamification idea (Quiz platform) | Done |
| 2 | Data models (no booleans) | Done |
| 3 | .NET 10 backend - CRUD, EF Core, SQLite | Done |
| 4 | React + TypeScript frontend (basic) | Done |
| 5 | Unit + integration tests (22 passing) | Done |
| 6 | Surface advanced features in UI (badges, streak, leaderboard) | TODO |
| 7 | Proper UI design (MUI/Mantine/Tailwind, mobile-friendly) | TODO |
| 8 | Deploy back/ (upgrade B1, redeploy) | Blocked |
| 9 | Deploy front/ (Azure Static Web Apps) | TODO |
| 10 | Scalar API docs live at /scalar | TODO |
| 11 | /specs folder - log more AI prompts + decisions | TODO |
| 12 | Root README.md documenting the project | TODO |
| 13 | Make repo public before submission | Required |
| 14 | Verify all links work in Incognito | Required |
| 15 | Submit via form (opens July 13) | Deadline Aug 2 |

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Step 7: Azure Deployment Blocker
- Azure CLI 2.87.0 installed, logged in as ync5389@autuni.ac.nz (AUT University tenant)
- Subscription: Azure for Students | ID: a266860f-628b-4bde-9a84-df8ca2e0ac4e | $100 credit
- Resource group created: rg-ts-msa (Australia East)
- App Service plan created: plan-ts-msa (F1 Free, Linux)
- Web app created: quizapi-ts-msa.azurewebsites.net (.NET 10 Linux)
- Deployment failed: F1 free tier hit QuotaExceeded (60 min CPU/day limit consumed by deploy attempts)
- Decision pending: upgrade to B1 Basic (~$13/mo from $100 credit) for reliable deployment

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Step 7: Azure Deployment (Starting)

| What | Status |
|------|--------|
| ts-msa GitHub repo | LIVE (private, jayreck996/ts-msa) |
| back/ .NET 10 API | Local only (C:\Users\tnako\Documents\GitHub\ts-msa\back\) |
| front/ React app | Local only |
| Azure App Service (back) | Not created yet |
| Azure Static Web Apps (front) | Not created yet |

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - DB Hosting Decision
- DB: SQLite file (quiz.db) hosted on Azure App Service local filesystem
- Resets on redeploy - acceptable for MSA assessment (markers won't redeploy)
- SQLite survives between requests (all reads/writes work normally during marking)
- Decision: keep SQLite, no Azure SQL needed - zero cost, zero extra setup
- Next: Step 7 - deploy back/ to Azure App Service + front/ to Azure Static Web Apps

## ISSUE:-jay 2026-06-24 -> NZMSA 2026: Phase 2 - Step 5 DONE: Unit Tests (22 passing)
- Backend back.tests/ (xUnit + EF Core InMemory) - 13 tests PASSED:
  - CategoriesControllerTests: GetAll empty, Create, GetById not found, Delete, Update
  - UsersControllerTests: Create, GetById, Delete
  - QuizzesControllerTests: Create, GetAll filter by difficulty, Delete
  - LeaderboardControllerTests: ordered by points, top N param
- Frontend front/src/pages/__tests__/ (Vitest + React Testing Library) - 9 tests PASSED:
  - Home.test.tsx: heading renders, tagline renders
  - Quizzes.test.tsx: loading state, list render, empty message, error message
  - Leaderboard.test.tsx: loading state, entries render, empty message
- Test setup: Vitest globals + jsdom + @testing-library/jest-dom setup.ts
- Next: Step 6 - confirm 3 advanced features (badge system, leaderboard, streak tracking already wired in back/)

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