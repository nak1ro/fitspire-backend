# Azure App Service Docker deployment

This backend is deployed as one Linux container. The API and its AI, gamification, and media-cleanup hosted workers run in that same process. This guide intentionally does not create Azure resources or CI/CD automation.

## Build and run locally

From `fitspire-backend`:

```powershell
docker build -t fitspire-backend:local .
docker run --rm -p 8080:8080 --env-file .env.production-safe fitspire-backend:local
```

Do not commit the env file. It must provide a reachable PostgreSQL instance, a valid JWT key, Production media storage settings, and the dedicated data-protection storage settings. The container serves HTTP on port `8080`.

## Container registry and App Service

1. Build and push the image to an Azure Container Registry (or another registry supported by App Service):

   ```powershell
   docker tag fitspire-backend:local <registry>.azurecr.io/fitspire-backend:<tag>
   docker push <registry>.azurecr.io/fitspire-backend:<tag>
   ```

2. Configure one Linux App Service for the image and set `WEBSITES_PORT=8080`.
3. Enable the App Service system-assigned managed identity.
4. Configure App Service Health Check to `https://<app-host>/health/ready`.

`/health/live` checks only whether the ASP.NET process is running and returns `200 Healthy` while the process is alive. `/health/ready` additionally checks PostgreSQL with a five-second timeout; it returns `200 Healthy` when PostgreSQL is reachable and `503 Unhealthy` otherwise. Neither endpoint checks OpenAI, email, media storage, or data-protection storage.

## Storage permissions

Create a dedicated, private container in a separate storage account for data-protection keys. Do not reuse the media account or media container.

Grant the App Service managed identity the minimum Blob data role required to read/write keys in the dedicated data-protection container (normally **Storage Blob Data Contributor** at the narrowest practical scope). Grant media-storage access separately to the media account/container; the current media initializer and upload workflow require Blob data access there as well.

The application does not create storage accounts, containers, role assignments, or managed identities.

## Required Production settings

Set App Service application settings using double underscores:

- `ConnectionStrings__DefaultConnection`
- `JWT__SigningKey`
- `Cors__AllowedOrigins__0` and additional indexed origins as needed
- `Frontend__BaseUrl`
- `Email__UseMockEmail=false`
- `Resend__ApiKey` and `Resend__SenderEmail` when real email is enabled
- `Authentication__Google__ClientId` and `Authentication__Google__ClientSecret` when Google sign-in is enabled
- `OpenAI__Enabled=true` and `OpenAI__ApiKey` when AI coaching is enabled
- `MediaStorage__ContainerName` and `MediaStorage__ServiceUrl`
- `DataProtection__ServiceUri` and `DataProtection__ContainerName`
- `Startup__ApplyMigrationsOnStartup=true`

Production media storage uses its service URI and managed identity. Data-protection storage accepts only an HTTPS Blob service URI plus container name; it does not accept a storage connection string or SAS token. Keep all credential values out of Dockerfiles, images, source control, and deployment logs.

The backend honors `X-Forwarded-For` and `X-Forwarded-Proto` only in Production, where it is intended to run behind the App Service ingress proxy. Do not expose the same container directly to untrusted clients while relying on forwarded headers.

## Startup, rollback, and credential safety

At startup, this single container validates Production configuration, applies pending EF migrations when enabled, runs idempotent seeders, then starts the API and hosted workers. A migration failure intentionally prevents the container from serving traffic.

Because schema migration happens during container startup, rolling back the image does not roll back the database. Review each migration and keep a database backup/rollback plan before deployment.

Rotate the database password and JWT signing key that were previously stored in source control. If the repository was shared, rotation is mandatory even if the old values have been removed from the current files.
