# stop.ps1 — stop the local dev environment (backend on 5289, frontend on 5173)

# --- Backend (port 5289) ---
$back = Get-NetTCPConnection -LocalPort 5289 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
if ($back) { Stop-Process -Id $back -Force; Write-Host "Backend stopped (port 5289)" } else { Write-Host "Backend not running" }

# Fallback: kill any stray QuizApi processes by name (port lookup can miss detached/elevated strays)
$stray = Get-Process QuizApi -ErrorAction SilentlyContinue
if ($stray) { $stray | Stop-Process -Force -ErrorAction SilentlyContinue; Write-Host "Cleaned up stray QuizApi process(es)" }

# --- Frontend (port 5173) ---
$front = Get-NetTCPConnection -LocalPort 5173 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
if ($front) { Stop-Process -Id $front -Force; Write-Host "Frontend stopped (port 5173)" } else { Write-Host "Frontend not running" }
