# ?? Secrets Management - Quick Reference Card

## ?? One-Time Setup (Current Machine)

```powershell
# Navigate to project
cd EllisHope

# Initialize user secrets
dotnet user-secrets init

# Set all secrets
dotnet user-secrets set "Unsplash:AccessKey" "9IEyanBmuyo8qB-v37hdi8PcPpqVV9R1voLgdhhn9hQ"
dotnet user-secrets set "Unsplash:SecretKey" "VMH2nCK7eAwvOZTOv1Czfkf1C813P6hazOSRIrlj0Yk"
dotnet user-secrets set "Unsplash:ApplicationName" "EllisHopeFoundation"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"

# Verify
dotnet user-secrets list
```

## ?? Backup Your Secrets

```powershell
# Windows - Find secrets file
$id = (Select-Xml -Path .\EllisHope.csproj -XPath "//UserSecretsId").Node.InnerText
$path = "$env:APPDATA\Microsoft\UserSecrets\$id\secrets.json"
echo $path

# Copy to secure backup
Copy-Item $path "C:\SecureBackup\ellishope-secrets.json"
```

## ?? New Machine Setup

**Option 1: Use Setup Script**
```powershell
# Copy your backed-up setup script to new machine
.\ellishope-setup-secrets.ps1
```

**Option 2: Manual Setup**
```powershell
# Run the commands from "One-Time Setup" above
```

## ? Verify Before Committing

```powershell
# Check appsettings.json has NO secrets
Get-Content EllisHope\appsettings.json | Select-String -Pattern "9IEyan|VMH2nC"
# Should return NOTHING

# Check git status
git status
# Should NOT show: appsettings.Development.json or any *secrets* files
```

## ?? Common Commands

```powershell
# List all secrets
dotnet user-secrets list

# Update a secret
dotnet user-secrets set "Unsplash:AccessKey" "NEW_VALUE"

# Remove a secret
dotnet user-secrets remove "Unsplash:AccessKey"

# Clear all secrets
dotnet user-secrets clear

# Get secrets file location
dotnet user-secrets list --project EllisHope
```

## ?? Where Are Secrets Stored?

**Windows**: `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`
**Mac**: `~/.microsoft/usersecrets/<id>/secrets.json`
**Linux**: `~/.microsoft/usersecrets/<id>/secrets.json`

## ?? Full Documentation

See `SECRETS-MANAGEMENT.md` for complete guide.

## ?? Security Rules

? DO:
- Use User Secrets for development
- Backup secrets to secure location
- Store in password manager
- Use environment variables in production

? DON'T:
- Commit secrets to Git
- Share via email/chat
- Store in repo files
- Use prod credentials in dev
