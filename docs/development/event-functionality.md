# Event Functionality - Implementation & Testing

## Overview

This document details the Event management functionality, including admin panel operations, public event listings, and event details pages. It also covers routing configuration and comprehensive test coverage.

## Event Controller Architecture

### Public EventController (`/Controllers/EventController.cs`)

Handles public-facing event pages:

#### Actions

1. **`list(string? search)`**
   - Lists upcoming events (default limit: 100)
   - Supports search functionality
   - Returns events matching search term or all upcoming events

2. **`details(string slug)`**
   - Displays single event details by slug
   - Populates sidebar with:
     - Recent events (limit: 3)
     - Similar events (limit: 4)
   - Returns 404 if event not found

3. **`grid()`**
   - Grid view for events (placeholder)

### Admin EventsController (`/Areas/Admin/Controllers/EventsController.cs`)

Handles admin event management:

#### Actions

1. **`Index(string? searchTerm, bool showUnpublished, bool showUpcomingOnly)`**
   - Lists all events with filtering
   - Supports search, published status filter, and upcoming-only filter

2. **`Create()`** (GET/POST)
   - Creates new events
   - Handles featured image upload
   - Auto-generates slug from title
   - Integrates with TinyMCE editor

3. **`Edit(int id)`** (GET/POST)
   - Edits existing events
   - Manages featured image updates
   - Validates event data

4. **`Delete(int id)`** (POST)
   - Soft deletes events
   - Cleans up associated images

## Routing Configuration

### Explicit Route for Event Details

Added in `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "eventDetails",
    pattern: "event/details/{slug}",
    defaults: new { controller = "Event", action = "details" });
```

**Why explicit routing?**
- Ensures `slug` parameter maps correctly to action parameter
- Prevents 404 errors from default route's `{id?}` parameter mismatch
- Consistent with blog details routing pattern

### URL Examples

| URL | Route | Action |
|-----|-------|--------|
| `/event/list` | Default | `EventController.list()` |
| `/event/list?search=charity` | Default with query | `EventController.list("charity")` |
| `/event/details/charity-run` | Explicit | `EventController.details("charity-run")` |
| `/admin/events` | Admin area | `Admin.EventsController.Index()` |

## Event Model

### Core Properties

```csharp
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Location { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public string? Tags { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }
    public string? OrganizerPhone { get; set; }
    public string? RegistrationUrl { get; set; }
    public int? MaxAttendees { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
```

## Views

### Public Views

#### `Views/Event/list.cshtml`
- Displays grid of upcoming events
- Search functionality
- Responsive card layout
- Event date badge overlay

#### `Views/Event/details.cshtml`
- Full event information
- Event details (date, time, location)
- Organizer information
- Featured image
- Related/similar events carousel
- Recent events sidebar
- Tags for categorization

### Admin Views

#### `Areas/Admin/Views/Events/Index.cshtml`
- Event list with filtering
- Search, publish status, and upcoming filters
- Edit/Delete actions
- Create new event button

#### `Areas/Admin/Views/Events/Create.cshtml`
- Event creation form
- TinyMCE rich text editor for description
- Featured image upload
- Date/time pickers
- Organizer details

#### `Areas/Admin/Views/Events/Edit.cshtml`
- Event editing form
- Pre-populated with existing data
- Image management
- Validation

## Test Coverage

### EventControllerTests (18 tests)

#### List Action Tests (4 tests)
- ? `List_ReturnsUpcomingEvents_WhenNoSearch`
- ? `List_ReturnsSearchResults_WhenSearchTermProvided`
- ? `List_SetsViewBagSearchTerm`
- ? `List_CallsGetUpcomingEventsAsync_WithLimit100`

#### Details Action Tests (11 tests)
- ? `Details_ReturnsNotFound_WhenSlugIsNull`
- ? `Details_ReturnsNotFound_WhenSlugIsEmpty`
- ? `Details_ReturnsNotFound_WhenEventDoesNotExist`
- ? `Details_ReturnsViewWithEvent_WhenEventExists`
- ? `Details_PopulatesViewBagWithSimilarEvents`
- ? `Details_PopulatesViewBagWithRecentEvents`
- ? `Details_CallsGetSimilarEventsAsync_WithCorrectParameters`
- ? `Details_CallsGetUpcomingEventsAsync_WithLimit3`
- ? `Details_CallsGetEventBySlugAsync_WithCorrectSlug`
- ? `Details_HandlesEmptySimilarEvents`
- ? `Details_HandlesEmptyRecentEvents`

#### Grid Action Tests (1 test)
- ? `Grid_ReturnsView`

#### Integration Tests (2 tests)
- ? `Details_IntegrationTest_CompleteWorkflow`
- ? `List_IntegrationTest_SearchWorkflow`

### EventServiceTests (23 tests)

Already existing, covering:
- CRUD operations
- Search functionality
- Slug generation
- Date filtering
- Published/unpublished filtering

## Usage Examples

### Creating an Event (Admin)

1. Navigate to `/admin/events`
2. Click "Create New Event"
3. Fill in event details:
   - Title: "Charity Run 2025"
   - Date: Select future date
   - Time: Start and end times
   - Location: Event venue
   - Description: Rich text content
   - Organizer info
   - Upload featured image
4. Click "Create"
5. Event slug auto-generated: `charity-run-2025`

### Viewing Event Details (Public)

1. Navigate to `/event/list`
2. Click on any event card
3. Redirects to `/event/details/charity-run-2025`
4. View full event information:
   - Event banner with date badge
   - Time and location details
   - Organizer contact information
   - Registration link
   - Full description
   - Similar events
   - Recent events sidebar

### Searching Events

Public:
```
GET /event/list?search=charity
```

Admin:
```
GET /admin/events?searchTerm=charity
```

## Service Layer

### IEventService Interface

```csharp
public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync(bool includeUnpublished = false);
    Task<Event?> GetEventByIdAsync(int id);
    Task<Event?> GetEventBySlugAsync(string slug);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    Task<IEnumerable<Event>> GetSimilarEventsAsync(int eventId, int count);
    Task<Event> CreateEventAsync(Event eventItem);
    Task<Event> UpdateEventAsync(Event eventItem);
    Task DeleteEventAsync(int id);
    string GenerateSlug(string title);
}
```

### Key Methods

- **`GetUpcomingEventsAsync(int count)`**: Returns future events, sorted by date
- **`GetSimilarEventsAsync(int eventId, int count)`**: Returns events with similar tags or nearby dates
- **`GetEventBySlugAsync(string slug)`**: Retrieves published event by URL-friendly slug

## Security

### Authorization

- **Admin Actions**: Require `[Authorize(Roles = "Admin,Editor")]`
- **Public Actions**: Anonymous access allowed

### Input Validation

- Required fields validated via model attributes
- Slug uniqueness enforced
- Date validation (events must be in future for creation)
- HTML sanitization via TinyMCE configuration

### File Upload Security

- Image uploads restricted to specific formats
- File size limits enforced
- Uploaded files stored in isolated directory
- Unique GUID-based filenames prevent conflicts

## Performance Considerations

### Caching Opportunities

Consider implementing caching for:
- Upcoming events list (1-hour cache)
- Event details (1-hour cache)
- Similar events (30-minute cache)

### Database Optimization

- Indexed columns: `Slug`, `EventDate`, `IsPublished`
- Efficient queries with `Include()` for related data
- Pagination for large event lists (currently limited to 100)

## Future Enhancements

### Potential Features

1. **Event Registration System**
   - Track attendees
   - Manage capacity
   - Send confirmation emails

2. **Calendar Integration**
   - iCal export
   - Google Calendar sync

3. **Categories**
   - Event types/categories
   - Filter by category

4. **Recurring Events**
   - Daily/weekly/monthly patterns
   - Series management

5. **Event Maps**
   - Google Maps integration
   - Directions link

6. **Social Sharing**
   - Share to Facebook, Twitter, LinkedIn
   - Open Graph metadata

## Testing Guidelines

### Writing New Tests

When adding new event functionality:

1. **Controller Tests**: Mock service layer, verify:
   - Correct service method called
   - ViewBag populated correctly
   - Proper HTTP status codes

2. **Service Tests**: Use in-memory database, verify:
   - Data persistence
   - Business logic
   - Edge cases

3. **Integration Tests**: Test complete workflows:
   - User creates event ? appears in list
   - Search returns correct results
   - Slug uniqueness enforcement

### Running Tests

```bash
# All event tests
dotnet test --filter "FullyQualifiedName~Event"

# Event controller only
dotnet test --filter "FullyQualifiedName~EventControllerTests"

# Event service only
dotnet test --filter "FullyQualifiedName~EventServiceTests"
```

## Troubleshooting

### Common Issues

**Event not found (404)**
- Check slug matches exactly (case-sensitive)
- Verify event is published (`IsPublished = true`)
- Ensure explicit route is configured in `Program.cs`

**Images not displaying**
- Verify file uploaded to `/wwwroot/uploads/events/`
- Check `FeaturedImageUrl` starts with `/`
- Confirm image file exists on server

**Search not working**
- Check search term length (min 1 character)
- Verify `SearchEventsAsync` implementation
- Test with simpler search terms

## Summary

The Event functionality provides:
- ? Complete CRUD operations
- ? Public event listing and details
- ? Admin panel for event management
- ? Search and filtering
- ? Featured image support
- ? Rich text descriptions
- ? Organizer information
- ? Similar/recent events
- ? **18 controller tests** covering all scenarios
- ? **23 service tests** ensuring business logic
- ? **100% pass rate**

Total: **41 event-related tests** ensuring production quality.
