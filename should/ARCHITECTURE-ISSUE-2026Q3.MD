ARCHITECTURE-ISSUE

INSTRUCTION FOR AI MODEL:

THIS FILE HOLDS EXACTLY ONE ENTRY: A SINGLE CONNECTED TREE ROOTED AT THE PROJECT NAME. YOU MAY READ AND UPDATE THIS TREE IN PLACE AS THE SYSTEM EVOLVES — EXTEND OR EDIT THE TREE DIRECTLY, KEEPING IT ONE CONNECTED WHOLE. DO NOT CREATE A SECOND DATED ENTRY.

FORMAT: ## ISSUE:ARCHITECTURE {YYYY-MM-DD HH:MM} → {CONTENT AS SINGLE CONNECTED TREE GRAPH}

## ISSUE:ARCHITECTURE 2026-07-02 10:10 →
ts-msa
├── Ephemeral SQLite storage
│   ├── quiz.db lives on Azure App Service's ephemeral filesystem
│   ├── All data wiped on every redeploy: users, quizzes, attempts, leaderboard
│   └── Acceptable for current assessment scope, not durable
│       └── Would need managed DB (Azure SQL / Postgres) or persisted volume for real use
└── Repo visibility
    ├── Still private
    └── Must go public before Aug 2 2026 deadline
        └── All live links (backend App Service URL, frontend Blob Storage URL) verified reachable in incognito window
