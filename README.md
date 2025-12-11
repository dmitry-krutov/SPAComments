# SPAComments

SPAComments is a full-stack comment system with a React + Vite frontend and an ASP.NET Core (.NET 9) backend. It delivers a drop-in comments experience for single-page applications with real-time updates, search, and file attachments.

Key capabilities:
- REST API for creating and managing comments with validation and CAPTCHA protection.
- Optional file attachments stored via a dedicated FileService (MongoDB + MinIO) and linked back to comments.
- Full-text search in Elasticsearch and a "latest" feed with pagination.
- SignalR hub at `/hubs/comments` for realtime comment notifications.
- Infrastructure built around PostgreSQL, Redis caching, RabbitMQ messaging, and Docker-friendly settings.

---

# SPAComments — Deployment Guide

This guide explains how to run the **SPAComments** project from scratch on a fresh server using Docker.  
All required database migrations, services, frontend and backend will run automatically via Docker Compose.

---

## 🔧 Requirements

- Docker
- Docker Compose
- Git

---

## 📥 1. Clone the Repository

```bash
git clone https://github.com/dmitry-krutov/SPAComments.git
cd SPAComments
```

---

## 🔐 2. Create the `.env` File

The backend and FileService require access to GitHub Packages to restore shared NuGet libraries.  
Create a `.env` file in the project root:

```bash
nano .env
```

Paste:

```
GITHUB_OWNER=dmitry-krutov
GITHUB_PACKAGES_TOKEN=<YOUR_GITHUB_TOKEN>
```

> ⚠ IMPORTANT: Do **NOT** commit your real token.  
> Generate a GitHub token with the `read:packages` permission.

Save and exit.

---

## 🐳 3. Build and Start All Services

```bash
docker compose build
docker compose up -d
```

Docker will automatically launch:

- ASP.NET Core Backend  
- React Frontend + Nginx  
- PostgreSQL  
- Redis  
- MongoDB  
- RabbitMQ  
- Elasticsearch  
- MinIO  
- FileService  

Once everything is running, the site will be available at:

```
http://<YOUR_SERVER_IP>
```

---

## 📦 4. Create a MinIO Bucket

Your file uploads require a public bucket named **spa-comments**.

### Install MinIO client (`mc`):

```bash
wget https://dl.min.io/client/mc/release/linux-amd64/mc -O mc
chmod +x mc
sudo mv mc /usr/local/bin/
```

### Add MinIO server:

```bash
mc alias set myminio http://<YOUR_SERVER_IP>:9000 minioadmin minioadmin
```

### Create the bucket:

```bash
mc mb myminio/spa-comments --ignore-existing
```

### Make the bucket publicly readable:

```bash
mc anonymous set download myminio/spa-comments
```

Verify:

```bash
mc anonymous get myminio/spa-comments
```

Expected:

```
Access: read
```

---

## 🚀 5. Done

The project is now fully deployed and operational.

### Frontend URL  
```
http://<YOUR_SERVER_IP>
```

### MinIO Console (optional)  
```
http://<YOUR_SERVER_IP>:9001
```
Credentials:  
```
minioadmin / minioadmin
```

---

## 🛠 Troubleshooting

### Backend cannot restore NuGet packages
Ensure `.env` contains a valid GitHub token with permission:

- `read:packages`

### MinIO file URLs return 403 or signature errors
Ensure the bucket is public:

```bash
mc anonymous set download myminio/spa-comments
```

---

## 📄 License

MIT

---

## Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)
- A GitHub personal access token (fine-grained or classic) with the **read:packages** scope to pull shared NuGet packages from `https://nuget.pkg.github.com/dmitry-krutov/index.json`.

## 1) Configure access to GitHub NuGet packages

Both the SPAComments solution and the FileService depend on the shared packages published to the GitHub feed above. Configure the feed once and restores will work for local runs and container builds.

```bash
# Create/overwrite a repository-local NuGet.Config with the GitHub feed
cat > NuGet.Config <<'XML'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github-dmitry-krutov" value="https://nuget.pkg.github.com/dmitry-krutov/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-dmitry-krutov>
      <add key="Username" value="GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="GITHUB_TOKEN_WITH_READ_PACKAGES" />
    </github-dmitry-krutov>
  </packageSourceCredentials>
</configuration>
XML
```

> Keep this file **locally** (do not commit tokens). NuGet will pick it up automatically for `dotnet restore`. If you prefer a global config instead of a local file, run `dotnet nuget add source https://nuget.pkg.github.com/dmitry-krutov/index.json -n github-dmitry-krutov -u GITHUB_USERNAME -p GITHUB_TOKEN_WITH_READ_PACKAGES --store-password-in-clear-text`.

## 2) Start backing services with Docker Compose

The API and FileService can run locally while databases and queues run in containers. Start the infra stack first (PostgreSQL, Redis, RabbitMQ, Elasticsearch, MongoDB, MinIO):

```bash
docker compose up postgres redis rabbitmq elasticsearch mongo minio -d
```

The compose file also exposes useful ports for local development:

- PostgreSQL: `5431`
- Redis: `6379`
- RabbitMQ: `5672` (management UI: `15672`)
- Elasticsearch: `9200`
- MongoDB: `27017`
- MinIO: `9000` (console: `9001`)

## 3) Restore dependencies

With the GitHub feed configured, restore NuGet packages for both solutions:

```bash
dotnet restore SPAComments.sln
cd src/SPAComments.FileService
dotnet restore FileService.sln
cd ../..
```

## 4) Run the services locally

Run the FileService (serves uploads to MinIO/Mongo) and the main SPAComments API side by side. Default appsettings already point to the compose services.

```bash
# Terminal 1 – FileService (HTTP on http://localhost:8081)
dotnet run --project src/SPAComments.FileService/src/FileService/FileService.csproj

# Terminal 2 – SPAComments API (HTTP on http://localhost:8080)
dotnet run --project src/SPAComments/SPAComments.Web/SPAComments.Web.csproj
```

Once both are running:

- Open Swagger UI for the API at `http://localhost:8080/swagger`.
- Open Swagger UI for FileService at `http://localhost:8081/swagger`.

## 5) Optional: run everything in Docker

If you prefer fully containerized apps, make sure `NuGet.Config` (with your GitHub token) is present in the repository root so the Docker build can reach the packages, then build and start all services:

```bash
docker compose up --build
```

The compose file maps the same ports as in local mode (`8080` for the API, `8081` for FileService).

## Useful defaults and configuration

- API settings for local dev are in `src/SPAComments/SPAComments.Web/appsettings.json` (PostgreSQL/Redis/RabbitMQ/Elasticsearch endpoints, FileService base address, CAPTCHA tuning).
- FileService settings are in `src/SPAComments.FileService/src/FileService/appsettings.json` (MongoDB connection and MinIO credentials/bucket).
- Docker-specific overrides live in `src/SPAComments/SPAComments.Web/appsettings.Docker.json` and environment variables in `compose.yaml`.

## What the API exposes

- `POST /api/comments` – Create a comment (validated + CAPTCHA).
- `POST /api/comments/attachments` – Upload an attachment; stored via FileService and linked to the comment payload.
- `GET /api/comments/search` – Full-text search in Elasticsearch with pagination.
- `GET /api/comments/latest` – Latest comments feed with pagination.
- `GET /api/captcha` – Issue a CAPTCHA challenge (image + id) to be passed to comment creation.
- SignalR hub at `/hubs/comments` ready for realtime notifications.

Feel free to seed your database or indexes as needed; the included `init.sql` provisions the base PostgreSQL schema when the `postgres` service starts.
