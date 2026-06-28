#!/bin/bash
ROOT="$(cd "$(dirname "$0")" && pwd)"
if [ -f "$ROOT/.pids" ]; then
  while read pid; do
    kill "$pid" 2>/dev/null && echo "Killed PID $pid"
  done < "$ROOT/.pids"
  rm "$ROOT/.pids"
else
  echo "No .pids file — nothing to stop"
fi
