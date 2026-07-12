# start.ps1 — launch the local dev environment in separate windows

# --- Backend (.NET API, port 5289) ---
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$PSScriptRoot\back'; dotnet run"

# --- Frontend (Vite dev server, port 5173) ---
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$PSScriptRoot\front'; npm run dev"

Write-Host "Backend:  http://localhost:5289"
Write-Host "Frontend: http://localhost:5173"
