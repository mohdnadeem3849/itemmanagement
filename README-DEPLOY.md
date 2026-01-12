=== README-DEPLOY.md ===
# ItemManagement Deployment (EC2 + Docker Compose)

## What this does
Deploys 3 containers on your EC2:
- SQL Server (private, NOT exposed publicly)
- .NET API (host port 8082)
- Nginx Web UI (host port 8088) with reverse proxy `/api` → API (avoids CORS)
SQL persists in Docker named volume: `itemmanagement_sql_data`

## Prereqs
- Your EC2 must already exist with Docker installed
- You must be able to SSH to EC2

## Required GitHub Secrets (this repo)
GitHub → Repo → Settings → Secrets and variables → Actions → **Secrets**
- `EC2_HOST` = your EC2 public IP (example: `54.12.34.56`)
- `EC2_USER` = `ubuntu`
- `EC2_SSH_KEY` = your PRIVATE key text (the one that matches EC2 public key)
- `SA_PASSWORD` = strong SQL password (example: `Str0ng!Passw0rd123!`)
- `SQL_DB` = database name (example: `ItemManagementDb`)

## Deploy
- Push to `main` OR
- Actions → `deploy-to-ec2` → Run workflow

## Verify (in your browser)
- Web: `http://<EC2_PUBLIC_IP>:8088`
- API health: `http://<EC2_PUBLIC_IP>:8082/api/health`

## Troubleshooting
### 1) SSH failing (Permission denied / timeout)
- Ensure security group allows port 22 from your IP
- Ensure `EC2_USER=ubuntu`
- Ensure your private key secret matches the public key used on EC2

### 2) SQL password rejected (container restarting)
- SQL Server requires strong password:
  - 8+ chars, upper, lower, number, symbol
- Update `SA_PASSWORD` secret and redeploy

### 3) Port already in use
SSH to EC2:
- `sudo ss -lntp | grep -E ':8082|:8088'`
- `cd ~/itemmanagement && sudo docker compose down`
- Re-run workflow

### 4) API healthcheck failing
SSH to EC2:
- `cd ~/itemmanagement`
- `sudo docker compose logs -f --tail=200 api`
- If your API does NOT have `/api/health`, change the healthcheck path in `docker-compose.yml` to your real endpoint.

### 5) Web builds but shows blank
- UI build folder can be `build/` (CRA) or `dist/` (Vite). This Dockerfile tries both.
- If your project uses a different output, tell me the build output folder name and I’ll adjust Dockerfile.
