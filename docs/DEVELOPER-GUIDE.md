# Ellis Hope Foundation - Developer Guide

**Version:** 1.0  
**Last Updated:** December 15, 2024  
**Target Framework:** .NET 10  
**For:** Developers & Technical Staff

---

## ?? Table of Contents

1. [System Overview](#system-overview)
2. [Getting Started](#getting-started)
3. [Architecture](#architecture)
4. [Database](#database)
5. [Authentication & Authorization](#authentication--authorization)
6. [Key Features](#key-features)
7. [Testing](#testing)
8. [Deployment](#deployment)
9. [Maintenance](#maintenance)

---

## ??? System Overview

### Technology Stack

- **Framework:** ASP.NET Core 10.0
- **Language:** C# 14.0
- **Database:** SQL Server (Entity Framework Core 10.0)
- **Authentication:** ASP.NET Core Identity
- **UI Framework:** Bootstrap 5
- **Image Processing:** SixLabors.ImageSharp 3.1.12
- **Utilities:** Slugify.Core 5.1.1

### Project Structure

```
EllisHopeFoundation/
??? EllisHope/                    # Main web application
?   ??? Areas/
?   ?   ??? Admin/               # Admin area (authorization required)
?   ?       ??? Controllers/
?   ?       ??? Models/
?   ?       ??? Views/
?   ??? Controllers/             # Public controllers
?   ??? Data/                    # DbContext, migrations, seeding
?   ??? Models/
?   ?   ??? Domain/             # Domain entities
?   ??? Services/               # Business logic services
?   ??? Views/                  # Public views
?   ??? wwwroot/                # Static assets
??? EllisHope.Tests/            # Test project
    ??? Controllers/            # Controller unit tests
    ??? Integration/            # Integration tests
```

---

## ?? Getting Started

### Prerequisites

- Visual Studio 2024 or later
- .NET 10 SDK
- SQL Server 2019 or later (LocalDB for development)
- Git

### Initial Setup

1. **Clone Repository:**
   ```bash
   git clone https://github.com/mcarthey/ellishopefoundation.org
   cd EllisHopeFoundation
   ```

2. **Configure Database:**
   ```bash
   # Update connection string in appsettings.json or user secrets
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YourConnectionString"
   ```

3. **Run Migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Seed Initial Data:**
   ```bash
   # Runs automatically on first startup
   # Creates admin user: admin@ellishope.org / Admin@123456
   ```

5. **Run Application:**
   ```bash
   dotnet run --project EllisHope
   ```

### User Secrets Setup

Store sensitive configuration in user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;..."
dotnet user-secrets set "Unsplash:AccessKey" "your-unsplash-key"
dotnet user-secrets set "Unsplash:SecretKey" "your-unsplash-secret"
```

---

## ??? Architecture

### Design Pattern: MVC

- **Models:** Domain entities in `Models/Domain/`
- **Views:** Razor views in `Views/` and `Areas/Admin/Views/`
- **Controllers:** Handle HTTP requests, return views/data

### Key Architectural Decisions

1. **Area-Based Organization:**
   - `/Admin/*` - Protected admin area
   - Public routes - Open to all

2. **Repository Pattern:**
   - DbContext provides data access
   - Services layer for business logic

3. **View Models:**
   - Separate view models for forms
   - Located in `Areas/Admin/Models/`

---

## ??? Database

### Entity Framework Core

**DbContext:** `ApplicationDbContext` in `Data/ApplicationDbContext.cs`

### Key Entities

```csharp
// Core Domain Models
ApplicationUser           // Extends IdentityUser
BlogPost
BlogCategory
Event
Cause
Page
ContentSection
Media
ImageSize
```

### Relationships

- **ApplicationUser ? ApplicationUser** (Sponsor relationship)
- **BlogPost ? BlogCategory** (Many-to-Many via BlogPostCategory)
- **BlogPost ? Media** (Featured image)
- **Page ? ContentSection** (One-to-Many)
- **Page ? PageImage** (One-to-Many via Media)

### Migrations

**Create Migration:**
```bash
dotnet ef migrations add MigrationName --project EllisHope
```

**Update Database:**
```bash
dotnet ef database update --project EllisHope
```

**Remove Last Migration:**
```bash
dotnet ef migrations remove --project EllisHope
```

### Database Seeding

`DbSeeder.cs` creates:
- Identity roles (Admin, BoardMember, Sponsor, Client, Member, Editor)
- Default admin user
- Blog categories
- Image size definitions

---

## ?? Authentication & Authorization

### ASP.NET Core Identity

**Configuration:** `Program.cs`

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### Authorization

**Admin Area Protection:**
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin,BoardMember,Editor")]
public class SomeController : Controller { }
```

**Role Hierarchy:**
1. **Admin** - Full access
2. **BoardMember** - Content + limited user management
3. **Editor** - Content management only
4. **Sponsor** - Sponsor portal access
5. **Client** - Client portal access
6. **Member** - Basic access

### Custom User Properties

`ApplicationUser` extends `IdentityUser`:
- UserRole enum (Admin, BoardMember, Sponsor, Client, Member)
- Status enum (Active, Pending, Inactive, Expired, Cancelled)
- Profile fields (Name, DOB, Address, etc.)
- Sponsor relationship (SponsorId, SponsoredClients)
- Financial fields (MonthlyFee, MembershipDates)

---

## ?? Key Features

### User Management

**Controller:** `UsersController`
**Views:** `Areas/Admin/Views/Users/`

Features:
- CRUD operations
- Role assignment
- Status management
- Sponsor assignment
- Welcome email on creation
- Filtering and search
- Last login tracking

### Profile Management

**Controller:** `ProfileController`
**Views:** `Areas/Admin/Views/Profile/`

Features:
- View own profile
- Edit personal information
- Change password
- Email change (updates username)
- Account menu (sitewide)

### Content Management

**Controller:** `PagesController`
**Views:** `Areas/Admin/Views/Pages/`

Features:
- Template-based pages (code-managed)
- Managed content pages (admin-managed)
- Rich text editor
- Image integration
- SEO fields (title, meta description)

### Media Library

**Controller:** `MediaController`
**Views:** `Areas/Admin/Views/Media/`

Features:
- File upload (local)
- Unsplash integration
- Automatic thumbnail generation
- Image size management
- Usage tracking
- Categorization and tagging

### Blog Management

**Controller:** `BlogController`
**Views:** `Areas/Admin/Views/Blog/`

Features:
- Rich text content
- Categories and tags
- Featured images
- Draft/Publish workflow
- SEO optimization
- View count tracking

### Event Management

**Controller:** `EventsController`
**Views:** `Areas/Admin/Views/Events/`

Features:
- Date/time scheduling
- Location information
- Registration links
- Organizer details
- Automatic archiving

### Cause Management

**Controller:** `CausesController`
**Views:** `Areas/Admin/Views/Causes/`

Features:
- Fundraising goals
- Progress tracking
- Featured causes
- Category organization
- Time-limited campaigns

---

## ?? Testing

### Test Structure

```
EllisHope.Tests/
??? Controllers/          # Unit tests for controllers
??? Integration/          # Integration tests
?   ??? CustomWebApplicationFactory.cs
?   ??? *IntegrationTests.cs
??? Services/            # Service layer tests (if applicable)
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific tests
dotnet test --filter "FullyQualifiedName~AccountController"

# With verbose output
dotnet test --verbosity detailed
```

### Test Coverage

**Current Coverage:** 342 passing tests

Areas covered:
- ? Account authentication
- ? User management CRUD
- ? Blog management
- ? Event management
- ? Cause management
- ? Media management
- ? Page management

### Writing New Tests

**Controller Test Example:**
```csharp
[Fact]
public async Task Create_ValidModel_RedirectsToIndex()
{
    // Arrange
    var model = new CreateViewModel { /* ... */ };
    
    // Act
    var result = await _controller.Create(model);
    
    // Assert
    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirectResult.ActionName);
}
```

---

## ?? Deployment

### Build Configuration

**Production Build:**
```bash
dotnet publish -c Release -o ./publish
```

### Environment Settings

**appsettings.json** - Default settings
**appsettings.Development.json** - Development overrides
**appsettings.Production.json** - Production overrides
**User Secrets** - Sensitive data (local dev only)
**Environment Variables** - Production secrets

### Database Migration (Production)

1. **Generate SQL Script:**
   ```bash
   dotnet ef migrations script --idempotent --output migrations.sql
   ```

2. **Apply to Production:**
   - Review SQL script
   - Execute via SQL Server Management Studio
   - Or use `dotnet ef database update` on server

### First-Time Deployment Checklist

- [ ] Update connection strings
- [ ] Run database migrations
- [ ] Configure SMTP settings (email)
- [ ] Set up Unsplash API keys (optional)
- [ ] Change default admin password
- [ ] Configure HTTPS
- [ ] Set up backup strategy
- [ ] Configure logging
- [ ] Test email functionality
- [ ] Test file upload limits

---

## ?? Maintenance

### Regular Tasks

**Weekly:**
- Monitor error logs
- Review failed login attempts
- Check database size

**Monthly:**
- Review user accounts
- Archive old events
- Clean unused media files
- Database backup verification

**Quarterly:**
- Update NuGet packages
- Security patch review
- Performance optimization
- User feedback review

### Database Maintenance

**Backup:**
```sql
-- Full backup
BACKUP DATABASE EllisHopeFoundation
TO DISK = 'C:\Backups\EllisHope_Full.bak'
WITH FORMAT, COMPRESSION;

-- Differential backup
BACKUP DATABASE EllisHopeFoundation
TO DISK = 'C:\Backups\EllisHope_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION;
```

**Cleanup Old Data:**
```sql
-- Archive events older than 2 years
UPDATE Events 
SET IsPublished = 0 
WHERE EventDate < DATEADD(YEAR, -2, GETDATE());

-- Delete soft-deleted blog posts older than 1 year
DELETE FROM BlogPosts 
WHERE IsPublished = 0 
AND ModifiedDate < DATEADD(YEAR, -1, GETDATE());
```

### Common Fixes

**Admin User Data Fix:**
If admin user has incorrect role/status:
```sql
-- See scripts/fix-admin-user.sql
UPDATE AspNetUsers
SET UserRole = 4, Status = 1, IsActive = 1
WHERE Email = 'admin@ellishope.org';
```

**Reset User Password:**
```csharp
var user = await _userManager.FindByEmailAsync(email);
var token = await _userManager.GeneratePasswordResetTokenAsync(user);
await _userManager.ResetPasswordAsync(user, token, newPassword);
```

### Monitoring

**Key Metrics:**
- Page load times
- Database query performance
- Failed login attempts
- Error rates
- User activity

**Logging:**
- Application logs in `Logs/` directory
- Configure log retention policy
- Set up alerts for critical errors

---

## ?? Additional Resources

### Useful Commands

```bash
# Clean and rebuild
dotnet clean
dotnet build

# Watch for changes (auto-rebuild)
dotnet watch run

# Entity Framework
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer

# List migrations
dotnet ef migrations list

# Generate migration script
dotnet ef migrations script
```

### Code Style

- Follow Microsoft C# coding conventions
- Use async/await for database operations
- Implement proper error handling
- Add XML comments for public APIs
- Keep controllers thin (business logic in services)

### Security Best Practices

1. Never commit secrets to source control
2. Use user secrets for local development
3. Use environment variables in production
4. Validate all user input
5. Use parameterized SQL queries (EF Core handles this)
6. Implement proper authorization checks
7. Keep dependencies up to date

---

## ?? Troubleshooting

### Common Issues

**Database Connection Errors:**
- Check connection string
- Verify SQL Server is running
- Check firewall settings

**Migration Errors:**
- Delete database and recreate
- Remove problematic migration
- Check for pending migrations

**Build Errors:**
- Clean solution
- Restore NuGet packages
- Check .NET SDK version

**Runtime Errors:**
- Check application logs
- Enable detailed errors in Development
- Review stack traces

---

## ?? Support

**Questions or Issues:**
- Create GitHub issue
- Email: dev@ellishope.org

**Documentation:**
- This guide (technical)
- ADMIN-GUIDE.md (end users)
- README.md (project overview)

---

**Document Version History:**
- v1.0 (Dec 2024): Initial comprehensive developer guide

