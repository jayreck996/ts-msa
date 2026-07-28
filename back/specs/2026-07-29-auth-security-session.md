# 2026-07-29 — Auth, Security & Advanced Requirements Session

## AI Tool
Claude (Sonnet 5) via Claude Code, working directly in the local repo with read/edit/bash
tool access (build, test, curl verification against the running API).

## Context
This session picked up backend auth work (AuthController, JWT, BCrypt, rate limiting) that
was already started but uncommitted, finished the missing frontend half, and closed the gap
against the official NZMSA 2026 Phase 2 Software Stream requirements.

## Prompts (verbatim, chronological)
1. "gh api to jayreck996/ts-msa"
2. "understand docs"
3. "what github url for 'phase 2'"
4. "no, from msa official github url"
5. "https://github.com/NZMSA/2026-Phase-2, what are requirements? are we ready to submit today?"
6. "arrange to low-to-high hanging fruit tasks, and start"
7. "just focus on backend testing, no frontend concerns, next"
8. "looks fine, use this self-reflection, log this session's prompts for now"

## What the AI did in response
- Fetched and read the official NZMSA 2026 Phase 2 requirements (repo README + the
  "Software Assessment.pdf") via `gh api` rather than assuming content from memory.
- Audited the live repo against those requirements: confirmed basic requirements were met,
  found zero of the official "Advanced Requirements" implemented, and found the README's
  existing "Advanced Features" list didn't actually match the assessment's fixed list (it
  described theme features, not the graded advanced-requirement categories).
- Found already-started, uncommitted backend auth work (`AuthController`, JWT bearer auth,
  BCrypt password hashing, fixed-window rate limiting) and verified it builds cleanly and
  passes all 22 existing backend tests.
- Found a real regression introduced by that in-progress work: `QuizPage.tsx` had a
  hardcoded `CURRENT_USER_ID = 1` (with a `TODO(auth)` comment) that would start failing
  once quiz-attempt submission required `[Authorize]`.
- Wrote the missing frontend half: `front/src/store/authStore.ts` (Zustand),
  `front/src/pages/Auth.tsx` (combined login/register page), and wired the JWT into
  `api.ts` and `QuizPage.tsx`, replacing the hardcoded user id.
- Added light/dark theme switching (`front/src/ThemeToggle.tsx` + `data-theme` CSS variant
  block in `index.css`), reusing the CSS custom properties already used throughout the
  stylesheet.
- Verified the full auth flow against the real running backend via `curl` — register,
  login, a 401 on attempt-submission without a token, a 200 with correct points and badge
  award with a token, and a 429 once the auth rate limit was exceeded — rather than relying
  on unit tests alone.
- Rewrote the root README to match the assessment's required submission sections: theme
  relation, unique/interesting features, a corrected advanced-features checklist (matching
  the official fixed list instead of the theme features previously listed there), and a
  self-reflection section.

## Design decisions made this session
- Zustand was chosen over Redux for the auth store: this app has a single slice of shared
  client state (the signed-in session), so Redux's ceremony wasn't justified.
- Theme switching was implemented with a `data-theme` attribute plus plain CSS custom
  properties rather than pulling in a theming library, since `index.css` already expressed
  every color as a CSS variable.
- The existing fixed-window rate-limiter values (5/min on auth endpoints, 60/min general)
  were left as-is rather than re-tuned, since they were already reasonable and untouched by
  this session's changes — confirmed via a 7-request burst test that 429s kicked in exactly
  at the configured threshold.
