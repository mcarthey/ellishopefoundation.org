# Causes Management System Implementation

## Summary

Implemented comprehensive Causes management functionality similar to Blog and Events, including domain models, services, controllers (public and admin), and full CRUD operations.

## Files Created

### Domain Models
- ? `EllisHope/Models/Domain/Cause.cs` - Core domain model with properties for title, description, goal/raised amounts, dates, etc.

### Services
- ? `EllisHope/Services/ICauseService.cs` - Service interface
- ? `EllisHope/Services/CauseService.cs` - Service implementation with full CRUD, search, filtering

### View Models
- ? `EllisHope/Areas/Admin/Models/CauseViewModel.cs` - Admin form model
- ? `EllisHope/Areas/Admin/Models/CauseListViewModel.cs` - Admin list view model

### Controllers
- ? `EllisHope/Controllers/CausesController.cs` - Public controller (updated)
- ? `EllisHope/Areas/Admin/Controllers/CausesController.cs` - Admin controller

## Files Modified

### Configuration
- ? `EllisHope/Data/ApplicationDbContext.cs` - Added Causes DbSet and indexes
- ? `EllisHope/Program.cs` - Registered ICauseService and added cause details route

## Features Implemented

### Domain Model (Cause.cs)
- Title, Slug, Description, ShortDescription
- Category, Tags
- Goal Amount, Raised Amount, Progress Percentage (calculated)
- Start Date, End Date
- Featured Image URL
- Donation URL
- IsPublished, IsFeatured flags
- View Count tracking
- Created/Updated dates

### Service Layer (CauseService.cs)
- `GetAllCausesAsync()` - Get all causes with optional unpublished filter
- `GetFeaturedCausesAsync()` - Get featured causes
- `GetActiveCausesAsync()` - Get active (non-expired) causes
- `GetCauseByIdAsync()` - Get by ID
- `GetCauseBySlugAsync()` - Get by slug (published only)
- `SearchCausesAsync()` - Search by title, description, category
- `GetCausesByCategoryAsync()` - Filter by category
- `GetSimilarCausesAsync()` - Get similar causes for recommendations
- `CreateCauseAsync()` - Create with auto-slug generation
- `UpdateCauseAsync()` - Update existing cause
- `DeleteCauseAsync()` - Delete cause
- `SlugExistsAsync()` - Check slug uniqueness
- `GenerateSlug()` - Generate URL-friendly slug
- `EnsureUniqueSlugAsync()` - Ensure unique slug with counter

### Public Controllers (CausesController.cs)
**Actions:**
1. `list(string? search)` - List active causes with optional search
2. `details(string slug)` - Show cause details with similar/featured causes in sidebar
3. `grid()` - Grid view of active causes

**Features:**
- Search functionality
- Similar causes recommendations
- Featured causes sidebar
- 404 handling for missing causes

### Admin Controller (Admin/CausesController.cs)
**Actions:**
1. `Index()` - List all causes with filters (search, unpublished, active, category)
2. `Create()` GET/POST - Create new causes with image upload
3. `Edit(int id)` GET/POST - Edit existing causes
4. `Delete(int id)` POST - Delete causes with image cleanup

**Features:**
- Authorization: `[Authorize(Roles = "Admin,Editor")]`
- TinyMCE rich text editor integration
- Featured image upload/management
- Auto-slug generation from title
- Search and filtering
- Category filtering
- Active-only filter
- Success/Error messages via TempData

### Database
**DbSet:** `Causes`

**Indexes:**
- Slug (unique)
- Category
- IsPublished

### Routing
Added explicit route in Program.cs:
```csharp
app.MapControllerRoute(
    name: "causeDetails",
    pattern: "causes/details/{slug}",
    defaults: new { controller = "Causes", action = "details" });
```

## URL Structure

### Public
- `/causes/list` - List all active causes
- `/causes/list?search=education` - Search causes
- `/causes/details/{slug}` - Cause details page
- `/causes/grid` - Grid view

### Admin
- `/admin/causes` - List all causes
- `/admin/causes?searchTerm=water` - Search
- `/admin/causes?categoryFilter=children` - Filter by category
- `/admin/causes?showActiveOnly=true` - Show active only
- `/admin/causes/create` - Create new cause
- `/admin/causes/edit/5` - Edit cause
- `/admin/causes/delete/5` - Delete cause (POST)

## TODO: Tests & Views

### Tests (To be created)
1. **CauseServiceTests.cs** - Unit tests for service layer (~25 tests)
2. **CausesControllerTests.cs** - Unit tests for public controller (~10 tests)
3. **Admin.CausesControllerTests.cs** - Unit tests for admin controller (~15 tests)
4. **CausesControllerIntegrationTests.cs** - Integration tests (~20 tests)

### Admin Views (To be created)
1. **Areas/Admin/Views/Causes/Index.cshtml** - List view
2. **Areas/Admin/Views/Causes/Create.cshtml** - Create form
3. **Areas/Admin/Views/Causes/Edit.cshtml** - Edit form

### Public Views (Existing - to be updated)
1. **Views/Causes/list.cshtml** - Update to use dynamic data from database
2. **Views/Causes/details.cshtml** - Update to use dynamic data from database
3. **Views/Causes/grid.cshtml** - Create grid view

## Migration Required

Run the following command to create and apply migration:
```bash
dotnet ef migrations add AddCausesTable
dotnet ef database update
```

## Integration with Existing Code

- ? Follows same pattern as Blog and Events
- ? Uses existing SlugHelper for URL generation
- ? Uses existing image upload infrastructure
- ? Uses existing TinyMCE configuration
- ? Uses existing authorization roles
- ? Uses existing DbContext
- ? Registered in DI container

## Next Steps

1. Create database migration
2. Create unit tests for CauseService
3. Create unit tests for controllers
4. Create integration tests
5. Create/update admin views
6. Update public views to use database data
7. Add to admin navigation menu
8. Create documentation

## Benefits

- ? Complete CRUD operations
- ? Search and filtering
- ? Featured/active cause support
- ? Progress tracking (goal vs raised)
- ? SEO-friendly slugs
- ? Image management
- ? Category organization
- ? Similar cause recommendations
- ? Admin role-based security
- ? Follows established patterns
- ? Production-ready code structure

## Status

**Backend**: ? Complete  
**Database**: ? Pending migration  
**Tests**: ? Pending  
**Admin Views**: ? Pending  
**Public Views**: ? Pending  
**Documentation**: ? Complete
