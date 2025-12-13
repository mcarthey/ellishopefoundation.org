# Secrets Management Guide

## Overview

This guide explains how to manage sensitive configuration data (API keys, connection strings, etc.) for the Ellis Hope Foundation website without committing them to source control.

## ?? Current Secrets in the Application

1. **Unsplash API Credentials**
   - `Unsplash:AccessKey`
   - `Unsplash:SecretKey`
   - `Unsplash:ApplicationName`

2. **TinyMCE API Key**
   - `TinyMCE:ApiKey`

3. **Database Connection String**
   - `ConnectionStrings:DefaultConnection`

## ?? Development Environment Setup

### Option 1: User Secrets (Recommended for Development)

User Secrets are stored outside your project folder in:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **Linux/Mac**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

#### Initial Setup

1. **Initialize User Secrets** (already done if `UserSecretsId` exists in .csproj)
   ```sh
   cd EllisHope
   dotnet user-secrets init
   ```

2. **Set Your Secrets**
   ```sh
   # Unsplash API Keys
   dotnet user-secrets set "Unsplash:AccessKey" "YOUR_ACCESS_KEY_HERE"
   dotnet user-secrets set "Unsplash:SecretKey" "YOUR_SECRET_KEY_HERE"
   dotnet user-secrets set "Unsplash:ApplicationName" "EllisHopeFoundation"
   
   # TinyMCE API Key
   dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_TINYMCE_API_KEY"
   
   # Database Connection
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   ```

3. **Verify Your Secrets**
   ```sh
   dotnet user-secrets list
   ```

4. **Remove a Secret** (if needed)
   ```sh
   dotnet user-secrets remove "Unsplash:AccessKey"
   ```

5. **Clear All Secrets** (if needed)
   ```sh
   dotnet user-secrets clear
   ```

#### Backup Your User Secrets

Create a backup of your secrets file for safekeeping:

**Windows:**
```powershell
# Find your UserSecretsId from EllisHope.csproj
$userSecretsId = "YOUR_USER_SECRETS_ID"
$secretsPath = "$env:APPDATA\Microsoft\UserSecrets\$userSecretsId\secrets.json"

# Backup to a secure location (NOT in the repo)
Copy-Item $secretsPath "C:\SecureBackup\ellishope-secrets.json"
```

**Linux/Mac:**
```bash
# Find your UserSecretsId from EllisHope.csproj
USER_SECRETS_ID="YOUR_USER_SECRETS_ID"
SECRETS_PATH="~/.microsoft/usersecrets/$USER_SECRETS_ID/secrets.json"

# Backup to a secure location (NOT in the repo)
cp "$SECRETS_PATH" ~/SecureBackup/ellishope-secrets.json
```

### Option 2: appsettings.Development.json (Alternative)

If you prefer a local file approach:

1. **Create** `appsettings.Development.json` (already in `.gitignore`)
2. **Add your secrets:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     },
     "Unsplash": {
       "AccessKey": "YOUR_ACCESS_KEY",
       "SecretKey": "YOUR_SECRET_KEY",
       "ApplicationName": "EllisHopeFoundation"
     },
     "TinyMCE": {
       "ApiKey": "YOUR_TINYMCE_API_KEY"
     }
   }
   ```

3. **Backup the file** to a secure location outside the repo

?? **Important**: Never commit `appsettings.Development.json` if it contains secrets!

## ?? Production Environment Setup

### Option 1: Azure App Service (Recommended)

1. **Navigate to**: Azure Portal ? Your App Service ? Configuration ? Application settings
2. **Add the following settings:**
   ```
   Unsplash__AccessKey = YOUR_ACCESS_KEY
   Unsplash__SecretKey = YOUR_SECRET_KEY
   Unsplash__ApplicationName = EllisHopeFoundation
   TinyMCE__ApiKey = YOUR_TINYMCE_API_KEY
   ConnectionStrings__DefaultConnection = YOUR_PRODUCTION_CONNECTION_STRING
   ```

Note: Use double underscores `__` instead of colons in environment variable names.

### Option 2: Azure Key Vault (Most Secure)

1. **Create Azure Key Vault**
2. **Add secrets to Key Vault**
3. **Configure your app** to use Key Vault:

```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVaultEndpoint"]!);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}
```

### Option 3: Environment Variables (Any Platform)

**Windows:**
```powershell
setx Unsplash__AccessKey "YOUR_ACCESS_KEY"
setx Unsplash__SecretKey "YOUR_SECRET_KEY"
setx TinyMCE__ApiKey "YOUR_TINYMCE_API_KEY"
setx ConnectionStrings__DefaultConnection "YOUR_CONNECTION_STRING"
```

**Linux/Mac:**
```bash
export Unsplash__AccessKey="YOUR_ACCESS_KEY"
export Unsplash__SecretKey="YOUR_SECRET_KEY"
export TinyMCE__ApiKey="YOUR_TINYMCE_API_KEY"
export ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
```

## ?? Cloning to a New Machine

When you clone the repository to a new machine:

### Quick Setup Script

Create a `setup-secrets.ps1` (PowerShell) or `setup-secrets.sh` (Bash) in your **secure backup location**:

**PowerShell:**
```powershell
# setup-secrets.ps1
cd "E:\Documents\Work\dev\repos\EHF\EllisHope"

dotnet user-secrets set "Unsplash:AccessKey" "9IEyanBmuyo8qB-v37hdi8PcPpqVV9R1voLgdhhn9hQ"
dotnet user-secrets set "Unsplash:SecretKey" "VMH2nCK7eAwvOZTOv1Czfkf1C813P6hazOSRIrlj0Yk"
dotnet user-secrets set "Unsplash:ApplicationName" "EllisHopeFoundation"
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_TINYMCE_API_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"

Write-Host "Secrets configured successfully!" -ForegroundColor Green
dotnet user-secrets list
```

**Bash:**
```bash
#!/bin/bash
# setup-secrets.sh
cd ~/repos/EllisHopeFoundation/EllisHope

dotnet user-secrets set "Unsplash:AccessKey" "9IEyanBmuyo8qB-v37hdi8PcPpqVV9R1voLgdhhn9hQ"
dotnet user-secrets set "Unsplash:SecretKey" "VMH2nCK7eAwvOZTOv1Czfkf1C813P6hazOSRIrlj0Yk"
dotnet user-secrets set "Unsplash:ApplicationName" "EllisHopeFoundation"
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_TINYMCE_API_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDb;Trusted_Connection=True;MultipleActiveResultSets=true"

echo "Secrets configured successfully!"
dotnet user-secrets list
```

### Manual Steps

1. **Clone the repository**
   ```sh
   git clone https://github.com/mcarthey/ellishopefoundation.org.git
   cd EllisHopeFoundation/EllisHope
   ```

2. **Restore User Secrets**
   
   Option A: Run your setup script
   ```sh
   # PowerShell
   .\path\to\setup-secrets.ps1
   
   # Bash
   bash ./path/to/setup-secrets.sh
   ```
   
   Option B: Copy backed-up secrets.json
   ```sh
   # Find UserSecretsId from EllisHope.csproj
   # Copy your backed-up secrets.json to the correct location
   ```

3. **Verify**
   ```sh
   dotnet user-secrets list
   ```

## ?? Security Best Practices

### ? DO
- ? Use User Secrets for development
- ? Use Azure Key Vault for production
- ? Keep backup of secrets in encrypted password manager (1Password, LastPass, etc.)
- ? Use different credentials for dev/staging/production
- ? Rotate API keys regularly
- ? Add `appsettings.Development.json` to `.gitignore`
- ? Document which secrets are needed in README

### ? DON'T
- ? Commit secrets to Git
- ? Share secrets via email or chat
- ? Store secrets in plaintext files in the repository
- ? Use production credentials in development
- ? Hardcode secrets in source code

## ?? .gitignore Configuration

Ensure your `.gitignore` includes:

```gitignore
# User-specific files
*.suo
*.user
*.userosscache
*.sln.docstates

# User secrets
appsettings.Development.json
appsettings.*.json
!appsettings.json
!appsettings.Production.json.template

# Environment files
.env
.env.local
.env.*.local

# Secret backup files
*secrets*.json
*secrets*.txt
setup-secrets.*
```

## ?? Troubleshooting

### Secrets Not Loading

1. **Check UserSecretsId exists** in `EllisHope.csproj`
2. **Verify secrets are set**: `dotnet user-secrets list`
3. **Check environment**: User Secrets only work in Development mode
4. **Restart IDE/terminal** after setting secrets

### Lost Secrets

1. **Check backup location**
2. **Check password manager** (if you stored them there)
3. **Regenerate API keys** (Unsplash dashboard)
4. **Recreate database** (LocalDB can be easily recreated)

## ?? Additional Resources

- [ASP.NET Core User Secrets Documentation](https://docs.microsoft.com/aspnet/core/security/app-secrets)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [Configuration in ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)

## ?? Quick Reference

### View Current Secrets
```sh
cd EllisHope
dotnet user-secrets list
```

### Update a Secret
```sh
dotnet user-secrets set "Unsplash:AccessKey" "NEW_VALUE"
```

### Remove All Secrets
```sh
dotnet user-secrets clear
```

### Find Secrets File Location
```sh
# PowerShell
$userSecretsId = (Select-Xml -Path .\EllisHope.csproj -XPath "//UserSecretsId").Node.InnerText
echo "$env:APPDATA\Microsoft\UserSecrets\$userSecretsId\secrets.json"

# Bash/Mac
USER_SECRETS_ID=$(grep -oP '(?<=<UserSecretsId>)[^<]+' EllisHope.csproj)
echo "~/.microsoft/usersecrets/$USER_SECRETS_ID/secrets.json"
