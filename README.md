# Ellis Hope Foundation Website

![.NET CI](https://github.com/mcarthey/ellishopefoundation.org/workflows/.NET%20CI/badge.svg)
[![codecov](https://codecov.io/gh/mcarthey/ellishopefoundation.org/branch/main/graph/badge.svg)](https://codecov.io/gh/mcarthey/ellishopefoundation.org)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Official website for the Ellis Hope Foundation - empowering health, fitness, and hope through community-driven initiatives.

## ?? Project Overview

This is a modern ASP.NET Core Razor Pages application built with .NET 10, featuring:

- ?? **Blog Management System** - Create and manage blog posts with categories and tags
- ?? **Event Management** - Organize and promote foundation events
- ?? **Content Management** - Dynamic page content with customizable sections
- ?? **Media Library** - Centralized media asset management
- ?? **Admin Portal** - Secure administrative interface using ASP.NET Core Identity
- ?? **Responsive Design** - Mobile-first design approach
- ? **Comprehensive Testing** - 45+ unit tests with full service coverage

## ??? Architecture

### Technology Stack

- **Framework**: .NET 10 (Preview)
- **Web Framework**: ASP.NET Core Razor Pages
- **Database**: SQL Server with Entity Framework Core 10
- **Authentication**: ASP.NET Core Identity
- **Testing**: xUnit, Moq, EF Core InMemory
- **CI/CD**: GitHub Actions
- **Frontend**: jQuery, Bootstrap 5, TinyMCE

### Project Structure

```
EllisHopeFoundation/
??? .github/
?   ??? workflows/
?       ??? dotnet-ci.yml                # CI/CD pipeline
??? EllisHope/                           # Main web application
?   ??? Areas/
?   ?   ??? Admin/                       # Admin portal
?   ?       ??? Controllers/             # Admin controllers
?   ?       ??? Models/                  # Admin view models
?   ?       ??? Views/                   # Admin views
?   ??? Data/
?   ?   ??? ApplicationDbContext.cs      # EF Core DbContext
?   ?   ??? DbSeeder.cs                  # Database seeding
?   ??? Models/
?   ?   ??? Domain/                      # Domain entities
?   ??? Services/                        # Business logic layer
?   ??? Views/                           # Razor Pages views
?   ??? wwwroot/                         # Static assets
??? EllisHope.Tests/                     # Unit tests
?   ??? Services/                        # Service layer tests
??? EllisHope.sln                        # Solution file
??? CI-CD-SETUP.md                       # CI/CD documentation
??? README.md                            # This file
```

## ??? Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.12+) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/mcarthey/ellishopefoundation.org.git
cd EllisHopeFoundation
```

2. **Restore dependencies**
```bash
dotnet restore EllisHope.sln
```

3. **Update database connection string**

Edit `EllisHope/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**IMPORTANT - Secrets Management**: 
The repository does not contain API keys or sensitive credentials. You must configure secrets before running the application.

**Quick Setup:**
```bash
cd EllisHope
dotnet user-secrets init
dotnet user-secrets set "Unsplash:AccessKey" "YOUR_ACCESS_KEY"
dotnet user-secrets set "Unsplash:SecretKey" "YOUR_SECRET_KEY"
dotnet user-secrets set "Unsplash:ApplicationName" "EllisHopeFoundation"
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_TINYMCE_API_KEY"
```

For detailed instructions, see [Secrets Management Guide](./docs/development/secrets-management.md).

4. **Apply database migrations**
```bash
cd EllisHope
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run --project EllisHope/EllisHope.csproj
```

The application will be available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

### First-Time Setup

1. Navigate to the application in your browser
2. The database seeder will create default categories on first run
3. Register an admin account (configure admin role in `appsettings.json` or database)
4. Access the admin portal at `/Admin`

## ?? Testing

### Run All Tests
```bash
dotnet test EllisHope.sln
```

### Run Tests with Coverage
```bash
dotnet test EllisHope.sln --collect:"XPlat Code Coverage"
```

### Test Coverage
- **BlogService**: 22 tests (CRUD, search, categories, slug generation)
- **EventService**: 23 tests (CRUD, filtering, date handling, search)
- **BlogController**: 16 tests (routing, ViewBag population, error handling)
- **Other Services**: 24 tests (various service layer tests)
- **Total**: **85 tests** with **100% pass rate** ?

## ?? Domain Models

### Core Entities

- **BlogPost** - Blog articles with categories, tags, and rich content
- **BlogCategory** - Categorization for blog posts
- **Event** - Foundation events with dates, locations, and organizer info
- **Page** - Dynamic content pages with sections
- **Media** - Media library assets
- **ContentSection** - Reusable content blocks

### Key Features

- Automatic slug generation from titles
- Published/draft workflow
- Many-to-many category relationships
- Optimistic concurrency with timestamps
- Featured image support

## ?? Admin Portal

Access: `/Admin`

### Features

- **Blog Management**
  - Create/Edit/Delete posts
  - Category management
  - Rich text editor (TinyMCE)
  - Featured image upload
  - SEO metadata

- **Event Management**
  - Create/Edit/Delete events
  - Date and time scheduling
  - Organizer information
  - Registration URLs
  - Attendee tracking

- **User Management** (via ASP.NET Identity)
  - Role-based access control
  - User authentication

## ?? Deployment

### CI/CD Pipeline

GitHub Actions automatically:
- ? Builds the application on every push
- ? Runs all 45 unit tests
- ? Publishes test results
- ? Collects code coverage

See [CI/CD Setup Guide](./docs/development/ci-cd-setup.md) for detailed documentation.

### Production Deployment

See the [Deployment Guide](./docs/deployment/deployment-guide.md) for complete instructions on deploying to production.

**Quick Start:**
1. Configure `appsettings.Production.json`
2. Set up HTTPS (see [HTTPS Setup Guide](./docs/deployment/https-setup-guide.md))
3. Deploy using Web Deploy or your preferred method

## ?? Documentation

Comprehensive documentation is available in the [`/docs`](./docs) folder:

### ?? For Developers
- [Configuration Guide](./docs/development/configuration.md) - Application configuration strategy
- [Secrets Management](./docs/development/secrets-management.md) - Managing sensitive data locally
- [CI/CD Setup](./docs/development/ci-cd-setup.md) - Continuous integration and deployment
- [TinyMCE Setup](./docs/development/tinymce-setup.md) - Rich text editor configuration

### ?? For Deployment
- [Deployment Guide](./docs/deployment/deployment-guide.md) - Complete deployment instructions
- [HTTPS Setup Guide](./docs/deployment/https-setup-guide.md) - Setting up SSL/HTTPS
- [Quick Reference](./docs/deployment/quick-reference.md) - Quick deployment commands
- [Deployment Checklists](./docs/deployment/) - Step-by-step checklists

### ?? For Security
- [HTTPS Configuration](./docs/security/https-configuration.md) - HTTPS security settings
- [Encrypted Configuration](./docs/security/encrypted-configuration.md) - Advanced security options

**See the [Documentation Index](./docs/README.md) for a complete list.**

## ?? Development Guidelines

### Code Style
- Follow C# coding conventions
- Use nullable reference types
- Implement async/await patterns
- Keep controllers thin, services fat

### Database Migrations

Create a new migration:
```bash
cd EllisHope
dotnet ef migrations add MigrationName
```

Update database:
```bash
dotnet ef database update
```

### Adding New Features

1. Create domain models in `Models/Domain/`
2. Update `ApplicationDbContext`
3. Create service interface and implementation
4. Add controllers and views
5. Write unit tests
6. Update database migrations

## ?? Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Commit Message Convention
```
feat: Add new feature
fix: Fix bug
docs: Update documentation
test: Add or update tests
refactor: Code refactoring
style: Code style changes
chore: Maintenance tasks
```

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Acknowledgments

- Ellis Hope Foundation team
- Open source community
- All contributors

## ?? Contact

- **Website**: [ellishopefoundation.org](https://ellishopefoundation.org)
- **GitHub**: [@mcarthey](https://github.com/mcarthey)
- **Issues**: [GitHub Issues](https://github.com/mcarthey/ellishopefoundation.org/issues)

## ?? Links

- [Complete Documentation](./docs/README.md)
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

---

**Built with ?? for the Ellis Hope Foundation**
