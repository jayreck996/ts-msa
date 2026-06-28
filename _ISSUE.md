## 2026-06-29 - No unified start script

- Repo has no root-level script to start DB + backend + frontend together
- SQLite is embedded (EF Core manages the file on dotnet run)
- Backend: cd back and dotnet run, API at http://localhost:5000
- Frontend: cd front and npm run dev, App at http://localhost:5173
- Resolution: document manual steps; consider adding start.ps1 at root
