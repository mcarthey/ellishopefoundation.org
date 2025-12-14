# Media Manager Integration Guide

## Overview

The Media Manager is now fully integrated with all content creation forms (Blog, Events, Causes). This provides centralized image management, eliminates duplication, and ensures consistency across the site.

## Architecture

### Unified Upload Directory Structure

```
wwwroot/
??? uploads/
    ??? media/              # ALL managed media (centralized)
        ??? {guid}.jpg      # Local uploads
        ??? unsplash/       # Unsplash imports
            ??? {photoId}.jpg
```

### Legacy Upload Directories (Deprecated)

These directories are maintained for backward compatibility only:

```
wwwroot/
??? uploads/
    ??? blog/    # Legacy blog uploads (deprecated)
    ??? events/  # Legacy event uploads (deprecated)
    ??? causes/  # Legacy cause uploads (deprecated)
```

**Note**: New uploads should always go through the Media Manager to `/uploads/media/`.

## Components

### 1. Media Picker Component

**Location**: `Views/Shared/_MediaPicker.cshtml`

**Purpose**: Reusable partial view that provides:
- Browse Media Library button
- Upload new file option
- Image preview
- Category filtering

**Usage**:

```razor
@{
    ViewData["FieldName"] = "FeaturedImageUrl";
    ViewData["FieldLabel"] = "Featured Image";
    ViewData["AllowUpload"] = true;
    ViewData["Category"] = "3"; // MediaCategory.Blog
}
<partial name="_MediaPicker" model="@Model.FeaturedImageUrl" />
```

**Parameters**:
- `FieldName`: Name of the form field (default: "FeaturedImageUrl")
- `FieldLabel`: Display label (default: "Featured Image")
- `AllowUpload`: Enable direct file upload (default: true)
- `Category`: Filter by MediaCategory enum value

**MediaCategory Enum Values**:
- `0` - Uncategorized
- `1` - Hero
- `2` - Page
- `3` - Blog
- `4` - Event
- `5` - Cause
- `6` - Team
- `7` - Gallery

### 2. Media Library Modal

The Media Picker includes a built-in modal that:
- Displays all managed media in a grid
- Supports search by filename, title, alt text
- Filters by category
- Shows image dimensions
- Allows quick selection
- Provides links to upload/import new images

### 3. Controller Integration

All Create/Edit controllers now handle both scenarios:

1. **Media Library Selection** (Recommended):
   - User selects existing image from Media Library
   - `FeaturedImageUrl` contains the media path
   - No file upload occurs
   - No duplication

2. **Direct Upload** (Legacy):
   - User clicks "Upload New" and selects from device
   - `FeaturedImageFile` contains the IFormFile
   - File is saved to legacy directory
   - Bypasses Media Manager tracking

**Controller Logic**:

```csharp
// Prioritize Media Library selection
if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
{
    entity.FeaturedImageUrl = model.FeaturedImageUrl;
}
else if (model.FeaturedImageFile != null)
{
    // Legacy direct upload
    entity.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
}
```

## Benefits

### 1. Centralized Management
- All images in one location
- Browse entire library when adding content
- No need to upload same image multiple times

### 2. Better Organization
- Category-based filtering
- Searchable by filename, title, tags
- Metadata tracking (alt text, dimensions, etc.)

### 3. Unsplash Integration
- Import professional photos directly to Media Library
- Proper attribution tracking
- Available for all content types

### 4. Usage Tracking (Future)
- Track where images are used
- Prevent deletion of in-use images
- Find unused images for cleanup

### 5. Bulk Operations (Future)
- Mass upload
- Batch categorization
- Bulk tagging

## User Workflows

### Creating Content with Existing Image

1. Navigate to Create form (Blog/Event/Cause)
2. Scroll to Featured Image section
3. Click "Browse Media Library"
4. Search/filter for desired image
5. Click on image to select
6. Image appears in preview
7. Save content

### Creating Content with New Image

**Option A: Through Media Library (Recommended)**
1. Navigate to Create form
2. Click "Browse Media Library"
3. Click "Upload to Media Library" link
4. Upload image with metadata
5. Return to Create form
6. Select newly uploaded image

**Option B: Direct Upload (Legacy)**
1. Navigate to Create form
2. Click "Upload New" button
3. Select file from device
4. Preview appears
5. Save content

**Note**: Option B bypasses the Media Manager and should be avoided when possible.

## Migration Strategy

### Phase 1: Integration (Completed)
? Create Media Picker component
? Update all Create/Edit forms
? Update all controllers
? Maintain backward compatibility

### Phase 2: Migration (Future)
- [ ] Create script to move legacy images to Media Library
- [ ] Populate Media table with existing images
- [ ] Update database references
- [ ] Delete legacy upload directories

### Phase 3: Cleanup (Future)
- [ ] Remove direct upload option
- [ ] Remove legacy upload code from controllers
- [ ] Simplify ViewModels (remove IFormFile properties)

## Technical Details

### Media Picker JavaScript API

**Functions**:
- `openMediaLibraryModal(fieldName, category)` - Opens modal with optional category filter
- `loadMediaLibrary(fieldName)` - Fetches and displays media
- `selectMedia(fieldName, imageUrl)` - Selects an image
- `clearMediaSelection(fieldName)` - Removes selection
- `toggleLocalUpload(fieldName)` - Shows/hides direct upload
- `previewLocalImage(input, fieldName)` - Previews selected file

### AJAX Endpoint

**URL**: `/Admin/Media/GetMediaJson?category={id}`

**Response**:
```json
[
  {
    "id": 1,
    "fileName": "example.jpg",
    "filePath": "/uploads/media/abc123.jpg",
    "altText": "Example image",
    "width": 1920,
    "height": 1080,
    "category": 3,
    "source": 0
  }
]
```

### Form Submission Behavior

The Media Picker creates a hidden input field:

```html
<input type="hidden" name="FeaturedImageUrl" value="/uploads/media/image.jpg" />
```

When selecting from Media Library:
- Hidden input is populated
- File input remains empty
- Controller receives URL only

When using direct upload:
- Hidden input remains empty
- File input contains IFormFile
- Controller receives file for upload

## Best Practices

### For Administrators

1. **Use Media Library for all new uploads**
   - Better organization
   - Searchable
   - Reusable across content

2. **Add descriptive metadata**
   - Alt text for accessibility
   - Titles for searchability
   - Tags for categorization

3. **Choose appropriate category**
   - Helps with filtering
   - Improves organization
   - Aids in finding images

### For Developers

1. **Always use Media Picker component**
   - Don't create custom file inputs
   - Maintains consistency
   - Future-proof

2. **Prioritize FeaturedImageUrl in controllers**
   - Check URL before File
   - Prevents unnecessary uploads
   - Leverages centralized storage

3. **Handle legacy images gracefully**
   - Don't delete Media Library images
   - Only delete from legacy directories
   - Check path before deletion

## Troubleshooting

### Images not appearing in Media Library

**Check**:
1. Are images in `/wwwroot/uploads/media/`?
2. Are they in the Media database table?
3. Is the category filter correct?
4. Try clearing search term

**Solution**: Images must be uploaded through Media Manager or imported from Unsplash to appear in the library.

### Direct upload not working

**Check**:
1. Is `AllowUpload` set to true in ViewData?
2. Is `FeaturedImageFile` property in ViewModel?
3. Is `enctype="multipart/form-data"` on form?

### Selected image not saving

**Check**:
1. Is hidden input being populated?
2. Is controller checking `FeaturedImageUrl` before `FeaturedImageFile`?
3. Check browser console for JavaScript errors

### Modal not opening

**Check**:
1. Is Bootstrap JavaScript included?
2. Are there JavaScript errors in console?
3. Is `_MediaPicker` partial included in view?

## Future Enhancements

### Planned Features

1. **Advanced Search**
   - Filter by date range
   - Filter by dimensions
   - Filter by file size

2. **Image Editing**
   - Crop/resize in browser
   - Apply filters
   - Compress images

3. **CDN Integration**
   - Automatic CDN upload
   - Optimized delivery
   - Bandwidth savings

4. **Usage Analytics**
   - Track image views
   - Popular images report
   - Unused images detection

5. **Bulk Operations**
   - Multi-select
   - Batch categorize
   - Mass delete

6. **AI Features**
   - Auto-generate alt text
   - Content-aware tagging
   - Smart categorization

## API Reference

### ViewData Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| FieldName | string | "FeaturedImageUrl" | Form field name |
| FieldLabel | string | "Featured Image" | Display label |
| AllowUpload | bool | true | Show upload option |
| Category | string | "" | Filter category (0-7) |

### Model Binding

The partial expects the current image URL as its model:

```razor
<partial name="_MediaPicker" model="@Model.FeaturedImageUrl" />
```

## Support

For issues or questions:
1. Check this documentation
2. Review browser console for errors
3. Check server logs
4. Refer to Media Manager documentation

## Change Log

### Version 1.0 (Current)
- Initial integration
- Media Picker component
- All forms updated
- Backward compatible

### Version 0.9 (Legacy)
- Separate upload directories
- No central management
- Direct file uploads only
