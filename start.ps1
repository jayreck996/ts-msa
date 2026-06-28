Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$PSScriptRoot\back'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$PSScriptRoot\front'; npm run dev"
Write-Host "Backend:  http://localhost:5000"
Write-Host "Frontend: http://localhost:5173"
