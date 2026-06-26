# /specs — AI Prompts & Design Decisions
<!-- CI test push -->

## AI Tools Used
- Claude (Sonnet 4.6) — primary assistant for planning, scaffolding, and coding
- Used for: data model design, backend scaffold, controller logic, EF Core setup

## Design Decisions

### No boolean fields
Replaced `isCorrect` on `Option` with `correctOptionId` (nullable FK) on `Question`.
This keeps the schema clean and avoids boolean flags that can cause ambiguity.

### Points by difficulty
Easy = score × 10 | Medium = score × 20 | Hard = score × 30
Score represents correct answers out of total questions.

### Level calculation
`Level = TotalPoints / 100` — simple integer division, level up every 100 points.

### Streak logic
Tracked on quiz attempt submission. Streak resets if no attempt the previous day.

### Badge system
Badges defined by string `Requirement` key checked at attempt submission:
- `first_quiz` — completed at least 1 quiz
- `points_100` / `points_500` — total points milestones
- `streak_7` — 7-day streak
- `perfect_score` — score of 10/10 on any quiz

## Prompts Log
- "design a data model for a gamified quiz platform with no boolean fields"
- "scaffold .NET 10 Web API with EF Core SQLite, Scalar docs, CORS, all controllers for quiz app"
- "implement badge auto-award logic and streak tracking in AttemptsController"
