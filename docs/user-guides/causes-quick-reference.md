# Causes Management - Quick Reference Guide

## Admin Panel Access

**URL**: `/admin/causes`  
**Authentication**: Required (Admin or Editor role)

### Create a New Cause
1. Navigate to `/admin/causes`
2. Click "Create New Cause"
3. Fill in required fields:
   - Title (required)
   - Description (required)
   - Goal Amount (required)
4. Optional fields:
   - Short Description (for cards/listings)
   - Category (Water, Education, Healthcare, Children, Other)
   - Start/End dates
   - Featured image
   - Donation URL
   - Tags
5. Check "Is Published" to make visible
6. Check "Is Featured" to highlight
7. Click "Create Cause"

### Edit a Cause
1. Navigate to `/admin/causes`
2. Click "Edit" on the cause
3. Modify fields as needed
4. Upload new image (optional)
5. Update Raised Amount as donations come in
6. Click "Save Changes"

### Delete a Cause
1. Navigate to `/admin/causes`
2. Click "Delete" on the cause
3. Confirm deletion in modal
4. Cause and associated image will be removed

### Search & Filter
- **Search**: Enter keywords in search box
- **Category**: Select from dropdown
- **Show Unpublished**: Toggle to see drafts
- **Active Only**: Show only non-expired causes

---

## Public URLs

- **List View**: `/causes/list`
- **Search**: `/causes/list?search=water`
- **Grid View**: `/causes/grid`
- **Details**: `/causes/details/{slug}`

---

## Database Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Title | string(200) | Yes | Cause name |
| Slug | string(200) | Auto | URL-friendly identifier |
| ShortDescription | string(500) | No | Brief summary |
| Description | text | No | Full description (HTML) |
| Category | string(100) | No | Water, Education, Healthcare, etc. |
| FeaturedImageUrl | string(500) | No | Image path |
| GoalAmount | decimal(18,2) | Yes | Target amount |
| RaisedAmount | decimal(18,2) | No | Current amount (default: 0) |
| StartDate | DateTime | No | Campaign start |
| EndDate | DateTime | No | Campaign end |
| DonationUrl | string(500) | No | External donation link |
| Tags | string(500) | No | Comma-separated |
| IsPublished | bool | No | Visible to public (default: false) |
| IsFeatured | bool | No | Highlighted (default: false) |
| ViewCount | int | No | Page views (default: 0) |
| CreatedDate | DateTime | Auto | Creation timestamp |
| UpdatedDate | DateTime | Auto | Last update timestamp |

---

## Progress Calculation

Progress is automatically calculated:
```
Progress % = (Raised Amount / Goal Amount) × 100
```

Example:
- Goal: $10,000
- Raised: $7,500
- Progress: 75%

---

## Categories

Predefined categories:
- Water
- Education
- Healthcare
- Children
- Other

Add custom categories by typing in the field.

---

## Image Upload

**Supported formats**: JPG, PNG, GIF  
**Recommended size**: 1200×800 pixels  
**Max file size**: Configured in server settings  
**Storage**: `/wwwroot/uploads/causes/`

---

## Service Methods (for developers)

```csharp
// Get all causes
var causes = await _causeService.GetAllCausesAsync();

// Get featured causes
var featured = await _causeService.GetFeaturedCausesAsync(count: 3);

// Get active (non-expired) causes
var active = await _causeService.GetActiveCausesAsync();

// Search causes
var results = await _causeService.SearchCausesAsync("education");

// Get by slug
var cause = await _causeService.GetCauseBySlugAsync("clean-water-project");

// Get similar causes
var similar = await _causeService.GetSimilarCausesAsync(causeId, count: 4);

// Create cause
var newCause = await _causeService.CreateCauseAsync(cause);

// Update cause
var updated = await _causeService.UpdateCauseAsync(cause);

// Delete cause
await _causeService.DeleteCauseAsync(id);
```

---

## Testing

Run all Causes tests:
```bash
dotnet test --filter "FullyQualifiedName~Cause"
```

Run specific test categories:
```bash
# Service tests only
dotnet test --filter "FullyQualifiedName~CauseServiceTests"

# Controller tests only
dotnet test --filter "FullyQualifiedName~CausesControllerTests"

# Integration tests only
dotnet test --filter "FullyQualifiedName~CausesControllerIntegrationTests"
```

---

## Common Tasks

### Update Raised Amount
1. Go to `/admin/causes`
2. Click "Edit" on the cause
3. Update "Raised Amount" field
4. Save - progress bar updates automatically

### Feature a Cause
1. Go to `/admin/causes`
2. Click "Edit" on the cause
3. Check "Is Featured"
4. Save - cause appears in featured sections

### End a Campaign
1. Go to `/admin/causes`
2. Click "Edit" on the cause
3. Set "End Date" to today or past
4. Save - cause marked as expired

### Publish a Draft
1. Go to `/admin/causes`
2. Check "Show Unpublished" to see drafts
3. Click "Edit" on draft
4. Check "Is Published"
5. Save - cause now visible to public

---

## Troubleshooting

**Cause not appearing on public site**:
- ? Check "Is Published" is checked
- ? Verify not expired (End Date)
- ? Clear browser cache

**Image not uploading**:
- ? Check file size < max upload limit
- ? Verify file format (JPG, PNG, GIF)
- ? Check folder permissions on `/wwwroot/uploads/causes/`

**Slug already exists**:
- ? System auto-appends number (e.g., "my-cause-1")
- ? Or manually edit slug to make unique

**Progress not updating**:
- ? Verify Goal Amount > 0
- ? Check Raised Amount entered correctly
- ? Formula: (Raised / Goal) × 100

---

## Integration with Other Features

### Media Library
- Upload images via Media Library
- Copy URL to "Featured Image URL" field

### Pages
- Link to causes: `/causes/details/{slug}`
- Embed cause list: Use partial view (future)

### Blog Posts
- Reference causes in blog content
- Link to specific causes

### Events
- Link fundraising events to causes
- Add cause URL to event donation button

---

## SEO Best Practices

1. **Title**: Clear, descriptive, under 60 characters
2. **Short Description**: Compelling, under 160 characters
3. **Slug**: Lowercase, hyphens, keywords
4. **Description**: Well-formatted HTML, headers, lists
5. **Images**: Alt text in title field
6. **Tags**: Relevant keywords, comma-separated

---

## Security

- ? Admin area requires authentication
- ? Role-based access (Admin, Editor)
- ? CSRF protection on forms
- ? File upload validation
- ? SQL injection protection (EF Core)
- ? XSS protection (Razor encoding)

---

## Support

**Documentation**:
- Implementation: `docs/development/causes-functionality.md`
- Test Coverage: `docs/testing/causes-test-coverage.md`
- Known Issues: `docs/issues/causes-integration-tests-failing.md`

**Code Files**:
- Service: `EllisHope/Services/CauseService.cs`
- Public Controller: `EllisHope/Controllers/CausesController.cs`
- Admin Controller: `EllisHope/Areas/Admin/Controllers/CausesController.cs`

---

**Last Updated**: December 14, 2024  
**Version**: 1.0  
**Status**: Production Ready ?
