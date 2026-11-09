# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

You can also install [eslint-plugin-react-x](https://npmx.dev/package/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://npmx.dev/package/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

---

## Full-stack setup: React + ASP.NET Core (.NET 10)

```
my-static-site/
├── src/                     React frontend (Vite)
│   ├── api/products.ts      fetch wrapper for the backend
│   └── components/Products.tsx
└── backend/
    ├── LearningApp.slnx     solution file
    └── Api/                 ASP.NET Core Web API
        ├── Controllers/     HTTP endpoints (ProductsController, HealthController)
        ├── Dtos/            request/response shapes
        ├── Models/          domain entities
        ├── Services/        business logic (IProductService + in-memory impl)
        ├── Program.cs       DI registration + middleware pipeline
        └── Api.http         ready-made requests to try in VS / VS Code
```

### Run it (two terminals)

```bash
# Terminal 1 – backend on http://localhost:5121
dotnet run --project backend/Api --launch-profile http

# Terminal 2 – frontend on http://localhost:5173
npm run dev
```

The Vite dev server proxies `/api/*` to the backend (see `vite.config.ts`), so React just calls `/api/products`.

### Endpoints

| Method | URL                  | Description          |
|--------|----------------------|----------------------|
| GET    | /api/health          | health check         |
| GET    | /api/products        | list products        |
| GET    | /api/products/{id}   | get one              |
| POST   | /api/products        | create `{name, price}` |
| PUT    | /api/products/{id}   | update               |
| DELETE | /api/products/{id}   | delete               |

OpenAPI document (Development): http://localhost:5121/openapi/v1.json

Data is stored in memory and resets when the API restarts — a good next step is swapping `InMemoryProductService` for an EF Core + SQLite version.
