# Railway Deployment Guide — QSS Platform

## Overview

Two Railway services in the same environment:

> **Important — Config-as-Code in a monorepo:**
> Railway's `railway.toml` is **single-service scoped**. A single root `railway.toml` does NOT automatically route to the right Dockerfile for each service — it applies to whichever service you assign it to in the dashboard.
>
> This repo uses **two separate config files** that must be assigned explicitly in the Railway dashboard:
>
> | File | Assign to service | Builds |
> |------|-------------------|--------|
> | `railway.api.toml` | `qss-api` | `Dockerfile.api` → runs `QSS.API.dll` |
> | `railway.web.toml` | `qss-web` | `Dockerfile.web` → runs `QSS.Web.dll` |
>
> In each service: **Settings → Config as Code → Config File Path** → enter the path above.
> If this is misconfigured (e.g. `qss-web` points to `railway.api.toml`), the web service will run the API binary and `/Login` will return 404.

Two Railway services in the same environment:

| Service | Source | Internal port | Public URL |
|---------|--------|---------------|------------|
| `qss-api` | `Dockerfile.api` | 8080 | `https://qss-api-production.up.railway.app` |
| `qss-web` | `Dockerfile.web` | 8080 | `https://qss-web-production.up.railway.app` |

Railway terminates HTTPS on port 443 and reverse-proxies to your container on port 8080.
**Never include `:8080` in any public-facing URL.**

---

## Required Railway services & volumes

### Service: `qss-api`

**Volume (required — prevents DB wipe on redeploy):**

| Setting | Value |
|---------|-------|
| Volume name | `qss-api-database` |
| Mount path | `/app/data` |

The SQLite database is stored at `/app/data/qss.db`. If the volume is not mounted at `/app/data`, the DB lives on the ephemeral container filesystem and is wiped on every redeploy, including all seeded users.

**Environment variables:**

```env
ASPNETCORE_ENVIRONMENT=Production
JwtKey=QSS-Platform-Super-Secret-Key-2024-Min32Characters!
JwtIssuer=QSS
JwtAudience=QSS
Cors__AllowedOrigins__0=https://qss-web-production.up.railway.app
```

> **Note:** The connection string defaults to `Data Source=/app/data/qss.db` via `appsettings.json`.
> Override with `ConnectionStrings__DefaultConnection=Data Source=/app/data/qss.db` if needed.
> Do **not** set `ASPNETCORE_URLS` — the app reads Railway's `PORT` env var automatically.

**Networking (Railway dashboard → qss-api → Settings → Networking):**

| Setting | Value |
|---------|-------|
| Target port | `8080` |
| Public domain | `qss-api-production.up.railway.app` |

---

### Service: `qss-web`

**Volume (recommended — persists Data Protection keys so cookie sessions survive redeploys):**

| Setting | Value |
|---------|-------|
| Volume name | `qss-web-keys` |
| Mount path | `/app/keys` |

Without this volume, all users are logged out on every redeploy (their auth cookies become undecryptable). They can re-login normally; no data is lost.

**Environment variables:**

```env
ASPNETCORE_ENVIRONMENT=Production
ApiBaseUrl=https://qss-api-production.up.railway.app
```

> **Important:** `ApiBaseUrl` must **not** end with `:8080`. Railway's public URL is HTTPS on port 443.
> For private networking (faster, no egress): `ApiBaseUrl=http://qss-api.railway.internal:8080`

> Do **not** set `ASPNETCORE_URLS` — the app reads Railway's `PORT` env var automatically.

**Networking (Railway dashboard → qss-web → Settings → Networking):**

| Setting | Value |
|---------|-------|
| Target port | `8080` |
| Public domain | `qss-web-production.up.railway.app` |

---

## Default seeded credentials

These users are created automatically on first startup (idempotent — safe on redeploy):

| Role | Email | Password |
|------|-------|----------|
| Superadmin | `superadmin@qss.com` | `Admin@1234!` |
| Admin | `admin@qss.com` | `Admin@1234!` |
| Dentist | `dentist@qss.com` | `Admin@1234!` |
| DentalAssistant | `assistant@qss.com` | `Admin@1234!` |
| Trainee | `trainee@qss.com` | `Admin@1234!` |

**Note the `@` in the password — it is `Admin@1234!`, not `Admin1234!`.**

---

## Healthcheck paths

| Service | Healthcheck path | Notes |
|---------|-----------------|-------|
| `qss-api` | `/health` | Returns `{"status":"healthy","database":"connected"}` |
| `qss-web` | `/health` | Returns `{"status":"healthy"}` — no auth, no DB dependency |

**Never use `/` as the healthcheck path.** On `qss-web`, `/` redirects to `/Dashboard` which requires authentication, causing every healthcheck probe to receive a 302/401 and marking the service as unavailable.

---

## Verifying a healthy deploy

Run these checks in order after every deploy:

### 1. API health check
```
GET https://qss-api-production.up.railway.app/health
```
Expected response (HTTP 200):
```json
{"status":"healthy","database":"connected"}
```
If `database` is `"unreachable"`, the SQLite volume is not mounted correctly at `/app/data`.

### 2. Web health check
```
GET https://qss-web-production.up.railway.app/health
```
Expected response (HTTP 200):
```json
{"status":"healthy"}
```
This endpoint requires no authentication and has no dependency on the API or database. If it returns anything other than HTTP 200, the web container itself has not started correctly.

### 3. Swagger UI
```
GET https://qss-api-production.up.railway.app/swagger
```
Expected: Swagger UI loads in browser (HTTP 200).

### 4. Web login
```
https://qss-web-production.up.railway.app/Login
```
Login with `superadmin@qss.com` / `Admin@1234!` → expect redirect to `/Dashboard`.

### 5. Redeploy regression check
After redeploying both services, repeat steps 1–4. Users must still exist and `/health` on both services must return HTTP 200.

---

## Troubleshooting

### "Container failed healthcheck" on qss-web
Railway probes the healthcheck path during deploy. If it receives anything other than HTTP 200, the deploy is marked failed.

1. **Wrong healthcheck path** — Go to Railway → `qss-web` → Settings → Config as Code → confirm Config File Path is `railway.web.toml`. That file sets `healthcheckPath = "/health"`. Never use `/` — it redirects to `/Dashboard` which requires authentication.
2. **App not yet listening** — Increase `healthcheckTimeout` in `railway.web.toml` if the container is slow to start.
3. **Keys volume warning in logs** — This is a warning, not a fatal error. The app still starts; Data Protection keys are just ephemeral. To silence the warning and persist auth cookies across redeploys, mount the `qss-web-keys` volume at exactly `/app/keys` in Railway → `qss-web` → Volumes.

### "Container failed to start" on qss-api
This usually means one of:

1. **Wrong start command** — Railway → `qss-api` → Settings → Start Command must be **empty** (so the Dockerfile `ENTRYPOINT ["dotnet", "QSS.API.dll"]` is used).
2. **Volume path wrong** — Verify `qss-api-database` is mounted at exactly `/app/data` (no trailing slash, not `/data`).
3. **Health check returning 503** — Fixed in code: the `/health` endpoint now always returns HTTP 200 with a `status` field of `"healthy"` or `"degraded"`. If you deployed before this fix, redeploy from the latest commit.
4. **Do not set `ASPNETCORE_URLS`** — The app reads Railway's `PORT` env var automatically.

Check Railway deploy logs (not build logs) for the exact startup exception.

### "Application failed to respond" on API URL
- Check Railway logs for startup errors.
- Verify the volume `qss-api-database` is mounted at `/app/data` (not `/data` or another path).
- Confirm Railway target port is `8080`.
- Do not set `ASPNETCORE_URLS` in Railway env vars.

### "Invalid credentials" on login
1. Confirm you are using `Admin@1234!` (with `@`), not `Admin1234!`.
2. Confirm the API is healthy: `GET /health` → `{"status":"healthy","database":"connected"}`.
3. If the DB was recreated (volume not mounted), seeding runs automatically on next restart — wait for it to complete.
4. Check `CORS` — ensure `Cors__AllowedOrigins__0` matches the exact Web public URL (no trailing slash, no `:8080`).

### Users wiped after redeploy
The `qss-api-database` volume is not mounted at `/app/data`. Go to Railway → `qss-api` → Volumes and verify the mount path is exactly `/app/data`.

### Auth cookies invalid after redeploy (users logged out)
Add and mount the `qss-web-keys` volume at `/app/keys` so Data Protection keys are persisted. Without it, users must re-login after every web service redeploy (no data is lost).

### qss-web returns 404 on /Login
The correct login URL is `https://qss-web-production.up.railway.app/Login` (capital L, matches `Pages/Login.cshtml`).
ASP.NET Core routing is case-insensitive so `/login` also works.

If you are seeing a 404, first check the Railway config assignment:

1. **Wrong Railway config file assigned (most common root cause)** — If `qss-web` has `railway.api.toml` (or the old root `railway.toml`) assigned, the service builds the API image and runs `QSS.API.dll`. The API has no `/Login` page, so every Razor Pages route returns 404.
   - Go to Railway → `qss-web` → Settings → Config as Code
   - Set Config File Path to `railway.web.toml`
   - Redeploy
   - After the fix, confirm the web container entrypoint is `dotnet QSS.Web.dll` in the build logs.

2. Confirm the web container is actually running (Railway → `qss-web` → Deployments shows green).
3. Confirm `ASPNETCORE_ENVIRONMENT=Production` is set and no other conflicting env vars are present.
4. Do **not** set `ASPNETCORE_URLS` — the app reads `PORT` automatically.
5. Try the root URL `https://qss-web-production.up.railway.app/` — it should redirect to `/Dashboard` which in turn redirects to `/Login` if unauthenticated.

### Login page says "Unable to connect to the API server"
- `ApiBaseUrl` in `qss-web` env vars is wrong. Correct value: `https://qss-api-production.up.railway.app` (no `:8080`).
- Alternatively use private networking: `http://qss-api.railway.internal:8080`.

---

## Architecture notes

- Both containers bind to `http://0.0.0.0:$PORT` where `PORT` is injected by Railway (defaults to 8080).
- Railway's reverse proxy handles HTTPS termination; containers only speak HTTP internally.
- SQLite is used for production. When migrating to SQL Server or PostgreSQL, set `ConnectionStrings__DefaultConnection` and update the EF provider in `QSS.Infrastructure`.
- Data Protection keys are stored in `/app/keys` on the web container. Mount a volume there to survive redeploys.
