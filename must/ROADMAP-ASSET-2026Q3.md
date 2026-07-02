ROADMAP-ASSET

INSTRUCTION FOR AI MODEL:

THIS FILE HOLDS EXACTLY ONE ENTRY: A SINGLE CONNECTED TREE ROOTED AT THE PROJECT NAME. YOU MAY READ AND UPDATE THIS TREE IN PLACE AS THE QUARTER PROGRESSES — EXTEND OR EDIT THE TREE DIRECTLY, KEEPING IT ONE CONNECTED WHOLE. DO NOT CREATE A SECOND DATED ENTRY.

SOURCE: Mirrors ARCHITECTURE-ASSET-2026Q3.MD branch structure. Update ARCHITECTURE-ASSET when each deliverable ships.

FORMAT: ## ASSET:ROADMAP {YYYY-MM-DD} → {CONTENT AS SINGLE CONNECTED TREE GRAPH}

## ASSET:ROADMAP 2026-07-02 →
```text
ts-msa
├── Submission Readiness (Aug 2 2026 deadline)
│   ├── Make repo public, verify all links reachable in incognito
│   └── Update README Live URLs table with real backend/frontend URLs
├── Documentation
│   └── Keep back/specs/README.md current — log AI prompts + design decisions through remaining build
├── Advanced Features (3+ required, list in README)
│   └── Gamification system already built — badges, leaderboard, streak tracking, levels
│       └── Confirm README explicitly lists these as the 3+ advanced features
├── Backend
│   └── Ephemeral SQLite (quiz.db wiped on redeploy)
│       └── Optional: migrate to Azure SQL / Postgres if durability becomes a requirement post-submission
└── Deploy Auth
    └── Kudu basic auth (credentials embedded in scmUri) — workaround for blocked service principal
        └── Revisit RBAC-based auth if Azure for Students subscription restriction lifts
```
