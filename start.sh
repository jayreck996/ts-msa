#!/bin/bash
ROOT="$(cd "$(dirname "$0")" && pwd)"
dotnet run --project "$ROOT/back" &
BACK_PID=$!
cd "$ROOT/front" && npm run dev &
FRONT_PID=$!
echo "$BACK_PID" > "$ROOT/.pids"
echo "$FRONT_PID" >> "$ROOT/.pids"
echo "Backend:  http://localhost:5000 (PID $BACK_PID)"
echo "Frontend: http://localhost:5173 (PID $FRONT_PID)"
