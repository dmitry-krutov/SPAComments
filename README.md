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
GITHUB_PACKAGES_TOKEN=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
```

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
