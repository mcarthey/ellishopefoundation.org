# Configuration Strategy

This project uses ASP.NET Core's built-in configuration system for managing settings across different environments.

## Configuration Files

### `appsettings.json` (Committed to Git)
- Base configuration file with default values
- Contains non-sensitive settings and structure
- **Safe to commit** to version control

### `appsettings.Development.json` (Git Ignored)
- Local development overrides (optional)
- Can be used for local database connections
- **Not committed** to version control

### `appsettings.Production.json` (Git Ignored, Manually Deployed)
- Production environment settings
- Contains production database connection strings and API keys
- **Not committed** to version control
- **Must be manually maintained** on the production server

### User Secrets (Local Only)
- Used during local development
- Managed via Visual Studio or `dotnet user-secrets` CLI
- Never leaves your local machine

## Configuration Priority

ASP.NET Core applies configuration in this order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., Development, Production)
3. User Secrets (Development environment only)
4. Environment Variables
5. Command-line arguments

## Setting Up Environments

### Local Development

1. **Using User Secrets (Recommended):**
   ```bash
   cd EllisHope
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   dotnet user-secrets set "Unsplash:AccessKey" "your-local-key"
   dotnet user-secrets set "Unsplash:SecretKey" "your-local-secret"
   dotnet user-secrets set "TinyMCE:ApiKey" "your-local-key"
   ```

2. **Or using appsettings.Development.json:**
   - Create this file locally if you prefer
   - Add your local settings
   - It won't be committed to git

### Production (SmarterASP.net)

1. **Create `appsettings.Production.json`** in the `EllisHope` folder with your production values:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-server;Database=your-db;User Id=your-user;Password=your-password;TrustServerCertificate=True;MultipleActiveResultSets=true"
     },
     "Unsplash": {
       "AccessKey": "your-production-key",
       "SecretKey": "your-production-secret",
       "ApplicationName": "EllisHopeFoundation"
     },
     "TinyMCE": {
       "ApiKey": "your-production-key"
     }
   }
   ```

2. **Publish via Web Deploy:**
   - The publish profile is configured to use the Production environment
   - `appsettings.Production.json` will be included in the deployment
   - Development settings will be excluded

3. **Security Notes:**
   - Keep a secure backup of `appsettings.Production.json` (password manager, secure note, etc.)
   - Never commit this file to version control
   - Use strong, unique passwords for production database connections

## Why This Approach?

? **Consistent** - Same pattern for local (User Secrets) and remote (appsettings.Production.json)  
? **Secure** - Secrets never committed to git  
? **Simple** - No need to configure environment variables on the hosting provider  
? **Portable** - Easy to deploy and update via Web Deploy  
? **ASP.NET Native** - Leverages framework's built-in configuration system  

## Updating Production Settings

When you need to update production settings:

1. Edit your local copy of `appsettings.Production.json`
2. Publish via Visual Studio using the Web Deploy profile
3. The updated configuration will be deployed automatically

## Important Files Location

- User Secrets are stored in: `%APPDATA%\Microsoft\UserSecrets\0ec83aa3-d463-491c-acc4-b3c9fa37241f\secrets.json`
- Production settings (keep secure backup): `EllisHope\appsettings.Production.json` (not in git)
