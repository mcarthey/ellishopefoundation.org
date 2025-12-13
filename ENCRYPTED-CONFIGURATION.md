# Encrypted Configuration (Advanced Security Option)

If you need enhanced security for production secrets, ASP.NET Core supports encrypted configuration sections using Data Protection API.

## Option 1: Azure Key Vault (Cloud-based)

If you move to Azure in the future:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Option 2: Encrypted appsettings.Production.json

For your current SmarterASP.net hosting, you can encrypt specific values:

### 1. Install Package
```bash
dotnet add package Microsoft.AspNetCore.DataProtection
```

### 2. Create Encrypted Values Script

Create a PowerShell script to encrypt your secrets:

```powershell
# encrypt-secrets.ps1
$plainText = "YourDatabasePassword"
$key = "YourEncryptionKey32CharsLong!!"  # Store this separately/securely

# Encrypt (simplified - use proper Data Protection API in production)
$bytes = [System.Text.Encoding]::UTF8.GetBytes($plainText)
$encrypted = [Convert]::ToBase64String($bytes)
Write-Output $encrypted
```

### 3. Update appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Password=ENCRYPTED:dGVzdA==;..."
  }
}
```

### 4. Add Decryption in Program.cs

```csharp
// After builder is created
builder.Services.AddDataProtection()
    .SetApplicationName("EllisHopeFoundation");

// Create custom configuration source that decrypts values
var encryptedConfig = builder.Configuration.AsEnumerable()
    .Where(kvp => kvp.Value?.StartsWith("ENCRYPTED:") == true)
    .ToList();

foreach (var kvp in encryptedConfig)
{
    var encryptedValue = kvp.Value.Substring("ENCRYPTED:".Length);
    var decryptedValue = DecryptValue(encryptedValue);
    builder.Configuration[kvp.Key] = decryptedValue;
}
```

## Option 3: Recommended for SmarterASP.net - Connection String Builder

Store only the password encrypted, build the connection string at runtime:

### appsettings.Production.json
```json
{
  "DatabaseConfig": {
    "Server": "your-server",
    "Database": "your-database",
    "UserId": "your-user",
    "PasswordEncrypted": "BASE64_ENCRYPTED_PASSWORD"
  }
}
```

### Program.cs
```csharp
var dbConfig = builder.Configuration.GetSection("DatabaseConfig");
var password = DecryptPassword(dbConfig["PasswordEncrypted"]);

var connectionString = new SqlConnectionStringBuilder
{
    DataSource = dbConfig["Server"],
    InitialCatalog = dbConfig["Database"],
    UserID = dbConfig["UserId"],
    Password = password,
    TrustServerCertificate = true,
    MultipleActiveResultSets = true
}.ConnectionString;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

## Security Assessment

| Method | Security Level | Complexity | Best For |
|--------|---------------|------------|----------|
| Plain appsettings.Production.json | ??? | Low | Small sites, trusted hosting |
| Encrypted sections | ???? | Medium | Shared hosting with extra security |
| Azure Key Vault | ????? | Medium | Cloud-hosted applications |
| Environment Variables | ??? | Low | Platforms with easy env var management |

## Current Recommendation

For SmarterASP.net with Web Deploy:
1. **Start with plain appsettings.Production.json** (current setup) ?
2. Ensure strong hosting account password
3. Use FTPS when possible
4. Monitor access logs
5. Implement encryption if compliance requires it

The risk is relatively low because:
- Your hosting provider (SmarterASP.net) is reputable
- Files are protected by IIS
- Your FTP access is password-protected
- The database is already behind a firewall

Most importantly: **Keep appsettings.Production.json out of Git** (already configured in .gitignore).
