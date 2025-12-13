# Production Deployment Guide

## Database Migration & Seeding Strategy

Your application now supports **automatic database setup** when deployed to production.

### How It Works

When the application starts (in any environment), it will:

1. ? **Check for pending migrations** - Automatically applies any new migrations
2. ? **Run database seeder** - Creates default data (roles, categories, admin user, image sizes)
3. ? **Safe & Idempotent** - Can run multiple times without creating duplicates

### Deployment Options

You have **two options** for deploying to production:

---

## Option 1: Automatic (Recommended) ?

**What happens**: Publish your app and let it auto-configure the database on first start.

### Steps:

1. **Publish the application**
   ```powershell
   # In Visual Studio: Right-click EllisHope ? Publish ? mcarthey-001-site1 - Web Deploy
   ```

2. **That's it!** On first startup, the application will:
   - Create all database tables (via migrations)
   - Seed default data
   - Create admin account

3. **Monitor the logs** (in SmarterASP.net control panel)
   - Look for: `"Database migrations applied successfully"`
   - Look for: `"Database seeding completed"`

### Advantages:
- ? No manual SQL script execution required
- ? Migrations and seeding happen together
- ? Works even if you add new migrations later
- ? Consistent with local development experience

### Disadvantages:
- ?? First startup might be slower (while migrations run)
- ?? If migrations fail, app won't start (by design - prevents broken state)

---

## Option 2: Manual Migration (Traditional)

**What happens**: Generate SQL script and run it manually before deploying.

### Steps:

1. **Generate migration script**
   ```powershell
   cd EllisHope
   dotnet ef migrations script --output production-migration.sql --idempotent
   ```

2. **Execute script on production database**
   - Log into SmarterASP.net control panel
   - Navigate to SQL Server Management
   - Run `production-migration.sql`

3. **Publish the application**
   ```powershell
   # Application will still run seeder on startup (creates default data)
   ```

### Advantages:
- ? Full control over when migrations happen
- ? Can review SQL before execution
- ? Faster first startup (tables already exist)

### Disadvantages:
- ? Manual process each time you have new migrations
- ? Need to remember to run script before deployment
- ? Requires SQL Server Management tool access

---

## What Gets Seeded Automatically?

The `DbSeeder` creates:

### 1. **Roles**
- Admin
- Editor
- User

### 2. **Default Admin Account**
- Email: `admin@ellishope.org`
- Password: `Admin@123456`
- ?? **CHANGE THIS PASSWORD IMMEDIATELY AFTER FIRST LOGIN!**

### 3. **Blog Categories** (6 categories)
- Children
- Education
- Healthcare
- Community
- Fundraising
- Volunteer

### 4. **Image Sizes** (18 predefined sizes)
- Page Header (1800×540)
- Event sizes (630×450, 1320×743)
- Blog sizes (425×500, 100×84)
- Team photos (400×500, various headshots)
- Hero images (1920×1080, 1800×855)
- Gallery sizes (1200×800, 300×300)

**All seeding is idempotent** - it checks if data exists before creating it.

---

## Recommended Deployment Workflow

### First Deployment (Fresh Database)

1. ? Ensure `appsettings.Production.json` is configured
2. ? Publish via Web Deploy (Visual Studio or CLI)
3. ? Visit your site - migrations and seeding happen automatically
4. ? Log in as admin: `admin@ellishope.org` / `Admin@123456`
5. ? **Immediately change admin password**
6. ? Test creating content (blog post, event)

### Subsequent Deployments (With New Migrations)

1. ? Add migration locally: `dotnet ef migrations add MigrationName`
2. ? Test migration locally: `dotnet ef database update`
3. ? Commit migration files to Git
4. ? Publish to production
5. ? On startup, new migration applies automatically
6. ? No manual intervention needed!

---

## Troubleshooting

### Problem: "Cannot open database" error

**Solution**: Check your connection string in `appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SQL1002.site4now.net;Initial Catalog=db_ab82c4_ellishopedb;User Id=db_ab82c4_ellishopedb_admin;Password=EHFDbAdmin.123;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True"
  }
}
```

### Problem: "Login failed for user" error

**Solution**: 
1. Verify username/password in connection string
2. Check SQL Server firewall allows your hosting provider's IP
3. Verify database user has correct permissions

### Problem: App won't start after deployment

**Check logs in SmarterASP.net**:
- Look for migration errors
- Check if database is accessible
- Verify `appsettings.Production.json` was deployed

**Emergency fix**: Comment out the database initialization block in `Program.cs`:
```csharp
/*
using (var scope = app.Services.CreateScope())
{
    // ... database initialization code
}
*/
```

Then investigate the issue separately.

### Problem: Admin account not created

**Cause**: The `DbSeeder` only creates the admin if it doesn't exist.

**Solution**:
1. Check if user exists: Query `AspNetUsers` table
2. If exists but password forgotten: Use ASP.NET Identity password reset
3. Or manually delete the user and restart the app to recreate it

---

## Production Safety Tips

### ? DO:
- Keep backups of `appsettings.Production.json` (password manager)
- Test migrations locally before deploying
- Monitor application logs after deployment
- Change default admin password immediately
- Use strong database passwords

### ? DON'T:
- Commit `appsettings.Production.json` to Git
- Use weak passwords for production database
- Deploy without testing migrations locally
- Delete migration files from the project
- Run manual database changes without migrations

---

## Monitoring First Deployment

After publishing, monitor these indicators:

### Success Indicators:
- ? Site loads at `http://mcarthey-001-site1.qtempurl.com/`
- ? Admin login page accessible
- ? Can log in with `admin@ellishope.org` / `Admin@123456`
- ? Database has 13+ tables (AspNet + Domain tables)

### Check Logs For:
```
[Information] Applying pending database migrations...
[Information] Database migrations applied successfully.
[Information] Database seeding completed.
[Information] Admin user created: admin@ellishope.org
[Information] Default blog categories created.
[Information] Default image sizes created.
```

---

## Alternative: Disable Auto-Migration in Production

If you prefer **manual control** in production, update `Program.cs`:

```csharp
// Only auto-migrate in Development
if (app.Environment.IsDevelopment())
{
    if (context.Database.GetPendingMigrations().Any())
    {
        await context.Database.MigrateAsync();
    }
}

// Always run seeder (it's idempotent)
await DbSeeder.SeedAsync(services);
```

This gives you:
- ? Auto-migration locally
- ? Manual migration in production
- ? Auto-seeding everywhere

---

## Summary

**Current Setup (Recommended)**:
- ? Automatic migrations on startup (all environments)
- ? Automatic seeding on startup (all environments)
- ? Production-ready error handling
- ? Logging for diagnostics

**Just publish and go!** The application handles the rest. ??
