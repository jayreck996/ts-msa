$back = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
if ($back) { Stop-Process -Id $back -Force; Write-Host "Backend stopped" } else { Write-Host "Backend not running" }

$front = Get-NetTCPConnection -LocalPort 5173 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
if ($front) { Stop-Process -Id $front -Force; Write-Host "Frontend stopped" } else { Write-Host "Frontend not running" }
