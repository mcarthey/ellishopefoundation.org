# Ellis Hope Foundation Website

**Version:** 1.0  
**Framework:** .NET 10  
**Last Updated:** December 15, 2024

---

## ?? About

The Ellis Hope Foundation website is a comprehensive platform for managing the foundation's online presence, including:
- Public-facing website with information about programs and services
- Admin portal for content management
- User management system with role-based access
- Blog and event management
- Donation tracking and cause management
- Media library for images and assets

---

## ?? Quick Start

### For Administrators

**Admin Portal:** `https://ellishopefoundation.org/Admin`

**Default Login:**
- Email: `admin@ellishope.org`
- Password: `Admin@123456`
- ?? Change this password immediately!

**Documentation:** See [Admin Guide](docs/ADMIN-GUIDE.md)

### For Developers

**Prerequisites:**
- .NET 10 SDK
- SQL Server
- Visual Studio 2024 (recommended)

**Setup:**
```bash
git clone https://github.com/mcarthey/ellishopefoundation.org
cd EllisHopeFoundation
dotnet restore
dotnet ef database update
dotnet run --project EllisHope
```

**Documentation:** See [Developer Guide](docs/DEVELOPER-GUIDE.md)

---

## ?? Documentation

| Document | Audience | Purpose |
|----------|----------|---------|
| [Admin Guide](docs/ADMIN-GUIDE.md) | Site Admins, Board Members | How to use the admin portal |
| [Developer Guide](docs/DEVELOPER-GUIDE.md) | Developers | Technical implementation details |
| [README.md](README.md) | Everyone | This file - overview and quick start |

---

## ?? Key Features

### Admin Portal
- **User Management:** Create, edit, manage users and roles
- **Content Management:** Edit pages, blog posts, events
- **Media Library:** Upload and organize images
- **Cause Management:** Track fundraising campaigns
- **Analytics:** View site statistics and user activity

### Public Site
- **Responsive Design:** Mobile-friendly, modern UI
- **Blog:** Share news and updates
- **Events:** Promote upcoming events
- **Causes:** Showcase fundraising campaigns
- **Contact Forms:** Connect with visitors

### Security
- **Role-Based Access:** Admin, Board Member, Editor, Sponsor, Client, Member
- **Secure Authentication:** ASP.NET Core Identity
- **Password Requirements:** Strong password enforcement
- **Account Management:** Self-service profile and password changes

---

## ?? Testing

**Run Tests:**
```bash
dotnet test
```

**Test Coverage:**
- 342 passing tests
- Controllers, services, integration tests
- All critical paths covered

---

## ??? Technology Stack

- **Framework:** ASP.NET Core 10.0
- **Language:** C# 14.0
- **Database:** SQL Server / Entity Framework Core 10.0
- **Authentication:** ASP.NET Core Identity
- **UI:** Bootstrap 5, jQuery
- **Image Processing:** SixLabors.ImageSharp
- **Testing:** xUnit

---

## ?? Project Structure

```
EllisHopeFoundation/
??? EllisHope/              # Main web application
?   ??? Areas/Admin/       # Admin portal
?   ??? Controllers/       # Public controllers
?   ??? Models/            # Domain models
?   ??? Views/             # Razor views
?   ??? wwwroot/           # Static assets
??? EllisHope.Tests/       # Test project
??? docs/                  # Documentation
??? scripts/               # Utility scripts
```

---

## ?? Deployment

**Build for Production:**
```bash
dotnet publish -c Release -o ./publish
```

**First Deployment:**
1. Configure database connection
2. Run migrations
3. Update admin password
4. Configure SMTP (email)
5. Set up HTTPS
6. Test functionality

See [Developer Guide](docs/DEVELOPER-GUIDE.md) for detailed deployment steps.

---

## ?? Support

**For Administrators:**
- Email: support@ellishope.org
- See: [Admin Guide](docs/ADMIN-GUIDE.md)

**For Developers:**
- GitHub Issues: Create an issue
- Email: dev@ellishope.org
- See: [Developer Guide](docs/DEVELOPER-GUIDE.md)

---

## ?? License

© 2024 Ellis Hope Foundation. All rights reserved.

---

## ?? Acknowledgments

Built with ?? for the Ellis Hope Foundation to support their mission of empowering health, fitness, and hope in the community.
