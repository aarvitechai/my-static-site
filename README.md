# Learning App: React + ASP.NET Core

Two independent projects in one repository:

| Folder                     | What                                  | Run                                                      |
|----------------------------|---------------------------------------|----------------------------------------------------------|
| [`frontend/`](frontend/)   | React + TypeScript + Vite             | `cd frontend && npm install && npm run dev` → :5173       |
| [`backend/`](backend/)     | ASP.NET Core Web API (.NET 10), EF Core | `cd backend && dotnet run --project Api --launch-profile http` → :5121 |

Each folder has its own README, dependencies and `.gitignore`, and can be opened on its own
(VS Code for `frontend/`, Visual Studio for `backend/LearningApp.slnx`).

They talk over HTTP only: in development Vite proxies `/api/*` to the backend.

`.github/workflows/deploy.yml` builds `frontend/` and deploys it to GitHub Pages.
