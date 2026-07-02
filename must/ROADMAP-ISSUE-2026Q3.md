ROADMAP-ISSUE

INSTRUCTION FOR AI MODEL:

THIS FILE HOLDS EXACTLY ONE ENTRY: A SINGLE CONNECTED TREE ROOTED AT THE PROJECT NAME. YOU MAY READ AND UPDATE THIS TREE IN PLACE AS THE QUARTER PROGRESSES — EXTEND OR EDIT THE TREE DIRECTLY, KEEPING IT ONE CONNECTED WHOLE. DO NOT CREATE A SECOND DATED ENTRY.

SOURCE: Mirrors ARCHITECTURE-ISSUE-2026Q3.MD branch structure. Update both files together when an issue is resolved.

FORMAT: ## ISSUE:ROADMAP {YYYY-MM-DD} → {CONTENT AS SINGLE CONNECTED TREE GRAPH}

## ISSUE:ROADMAP 2026-07-02 →
```text
ts-msa
├── Ephemeral SQLite storage
│   └── Plan: leave as-is for assessment submission (Aug 2) — durability not in scope
│       └── Future: migrate to Azure SQL / Postgres if project continues post-submission
├── Repo visibility
│   └── Plan: flip repo to public + verify all live links in incognito before Aug 2 deadline
└── Azure for Students subscription restrictions
    ├── Kudu basic-auth workaround
    │   └── Plan: leave as-is — subscription restriction is outside project control
    └── Blob Storage instead of Static Web Apps
        └── Plan: leave as-is — Blob Storage static site already satisfies the live-URL submission requirement
```
