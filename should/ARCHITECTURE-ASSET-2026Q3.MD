ARCHITECTURE-ASSET

INSTRUCTION FOR AI MODEL:

THIS FILE HOLDS EXACTLY ONE ENTRY: A SINGLE CONNECTED TREE ROOTED AT THE PROJECT NAME. YOU MAY READ AND UPDATE THIS TREE IN PLACE AS THE SYSTEM EVOLVES — EXTEND OR EDIT THE TREE DIRECTLY, KEEPING IT ONE CONNECTED WHOLE. DO NOT CREATE A SECOND DATED ENTRY.

FORMAT: ## ASSET:ARCHITECTURE {YYYY-MM-DD HH:MM} → {CONTENT AS SINGLE CONNECTED TREE GRAPH}

## ASSET:ARCHITECTURE 2026-07-02 10:10 →
ts-msa
├── Backend
│   ├── .NET 10 Web API + EF Core + SQLite
│   ├── Controllers: Users, Categories, Quizzes, Questions, Options, Attempts, Badges, Leaderboard
│   ├── Scalar API docs at /scalar
│   └── Deployed: Azure App Service F1 (quizapi-ts-msa, rg-ts-msa) via Kudu ZipDeploy REST API
├── Frontend
│   ├── React + TypeScript + Vite
│   ├── React Router pages: Home, Quizzes, Leaderboard
│   ├── Typed fetch client: api.ts
│   └── Deployed: Azure Blob Storage static website (quizfrontsa, $web container)
│       ├── Not Azure Static Web Apps (blocked by Azure for Students subscription policy)
│       └── Not Vercel (considered, not the final path)
├── Data Model
│   └── User, Category, Quiz, Question, Option, QuizAttempt, Badge, UserBadge
├── Gamification
│   ├── Points by difficulty: Easy=10, Medium=20, Hard=30
│   ├── Levels: 1 per 100pts
│   ├── Daily streak tracking
│   └── Badges on milestones
└── CI/CD (GitHub Actions, path-filtered)
    ├── backend.yml (back/**): dotnet test → dotnet publish → curl POST Kudu ZipDeploy
    └── frontend.yml (front/**): npm test → npm run build → az storage blob upload-batch
