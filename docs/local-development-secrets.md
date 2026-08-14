# Local development secrets

The backend project uses .NET user secrets in Development. They are stored outside the repository and are also read by EF design-time commands.

Set the required values from the `fitspire-backend` directory:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=fitspireapp;Username=YOUR_USER;Password=YOUR_PASSWORD"
dotnet user-secrets set "JWT:SigningKey" "YOUR_32_BYTE_OR_LONGER_RANDOM_SIGNING_KEY"
```

Set these only when the related integration is enabled:

```powershell
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"
dotnet user-secrets set "Resend:ApiKey" "YOUR_RESEND_API_KEY"
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY"
```

The tracked `appsettings.json` intentionally contains no database password, JWT signing key, or third-party API credential. `MediaStorage:ConnectionString=UseDevelopmentStorage=true` remains in tracked development defaults because it configures local Azurite, not a production account credential.

For Azure App Service, provide the same settings as environment variables using double underscores:

- `ConnectionStrings__DefaultConnection`
- `JWT__SigningKey`
- `Authentication__Google__ClientSecret` when Google sign-in is enabled
- `Resend__ApiKey` when Resend email is enabled
- `OpenAI__ApiKey` when AI Coach generation is enabled
- `Administration__InitialAdminEmails__0` and further indexed values to bootstrap existing administrator accounts
- `Startup__ApplyMigrationsOnStartup` to explicitly control startup migrations
- `DataProtection__ServiceUri` and `DataProtection__ContainerName` in Production

`appsettings.Production.json` disables mock email and clears local Azurite configuration. In Production, `DataProtection__ServiceUri` must be the HTTPS Blob service URI and `DataProtection__ContainerName` a dedicated private container. The application accepts no data-protection connection string or SAS setting; it uses the App Service managed identity.

`OpenAI:Enabled` and `OpenAI:Model` are tracked non-secret defaults. The empty `OpenAI:ApiKey` placeholder means the API starts without a credential, while AI Coach generation returns `503` until this user secret or production App Service setting is supplied. `AiCoachInteraction` settings are non-secret code-owned operational limits and should not be set in browser configuration.

`Administration:InitialAdminEmails` is a non-secret, initially empty list. Each configured existing account receives the `Admin` role idempotently on startup; no account is created and removing an address later does not demote an existing administrator. An assigned user must sign out and sign in again before their JWT/session carries the new role.
