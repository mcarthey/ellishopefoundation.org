# Media Manager Integration - Quick Reference Card

## For Users

### Upload New Image to Media Library
1. Admin ? **Media** ? **Upload Image**
2. Select file, add title/alt text/tags
3. Choose category
4. **Upload**

### Use Image in Content
1. Create/Edit Blog/Event/Cause
2. Scroll to **Featured Image**
3. Click **Browse Media Library**
4. Search/filter for image
5. Click image to select
6. **Save** content

### Import from Unsplash
1. Admin ? **Media** ? **Browse Unsplash**
2. Search for topic
3. Click **Import** on desired image
4. Add metadata
5. **Import Photo**

## For Developers

### Add Media Picker to Form
```razor
@{
    ViewData["FieldName"] = "FeaturedImageUrl";
    ViewData["FieldLabel"] = "Featured Image";
    ViewData["AllowUpload"] = true;
    ViewData["Category"] = "3"; // 0-7
}
<partial name="_MediaPicker" model="@Model.FeaturedImageUrl" />
```

### Controller Logic
```csharp
// Create/Edit POST action
if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
{
    entity.FeaturedImageUrl = model.FeaturedImageUrl; // From Media Library
}
else if (model.FeaturedImageFile != null)
{
    entity.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile); // Legacy
}
```

### Safe Deletion
```csharp
// Only delete from legacy directories
if (!string.IsNullOrEmpty(entity.FeaturedImageUrl) && 
    (entity.FeaturedImageUrl.Contains("/uploads/causes/") || 
     entity.FeaturedImageUrl.Contains("/uploads/blog/") || 
     entity.FeaturedImageUrl.Contains("/uploads/events/")))
{
    DeleteFeaturedImage(entity.FeaturedImageUrl);
}
```

## MediaCategory Enum

| Value | Category | Use For |
|-------|----------|---------|
| 0 | Uncategorized | General images |
| 1 | Hero | Large header images |
| 2 | Page | Page content images |
| 3 | Blog | Blog post images |
| 4 | Event | Event photos |
| 5 | Cause | Cause/campaign images |
| 6 | Team | Team member photos |
| 7 | Gallery | Gallery images |

## Directory Structure

```
wwwroot/uploads/
??? media/              ? NEW: All uploads here
?   ??? {guid}.jpg
?   ??? unsplash/
?       ??? {id}.jpg
??? blog/               ? LEGACY: No new uploads
??? events/             ? LEGACY: No new uploads
??? causes/             ? LEGACY: No new uploads
```

## JavaScript API

### Functions
- `openMediaLibraryModal(fieldName, category)`
- `loadMediaLibrary(fieldName)`
- `selectMedia(fieldName, imageUrl)`
- `clearMediaSelection(fieldName)`
- `toggleLocalUpload(fieldName)`

### AJAX Endpoint
`GET /Admin/Media/GetMediaJson?category={0-7}`

## Common Scenarios

### Reuse Image Across Multiple Posts
1. Upload once to Media Library
2. For each post:
   - Browse Media Library
   - Select same image
   - Save

### Replace Image in Edit Form
1. Open Edit form
2. Click **Browse Media Library**
3. Select new image
4. Old selection cleared automatically
5. Save

### Remove Image
1. Open Edit form
2. Click **Remove** under current image
3. Save with no image

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Modal won't open | Check Bootstrap JS loaded |
| Images not loading | Check `/uploads/media/` exists |
| Can't find image | Clear category filter |
| Upload fails | Check file size (<10MB) |
| Preview not showing | Hard refresh (Ctrl+F5) |

## Best Practices

? **DO**:
- Use Media Library for all uploads
- Add descriptive alt text
- Choose correct category
- Tag images appropriately
- Delete unused images

? **DON'T**:
- Use direct upload (bypasses library)
- Upload duplicates
- Skip metadata
- Delete images in use

## Documentation Links

- **User Guide**: `docs/user-guides/media-manager-guide.md`
- **Integration Guide**: `docs/development/media-manager-integration.md`
- **Summary**: `docs/development/media-manager-integration-summary.md`
- **Before/After**: `docs/development/media-manager-before-after.md`

## Support

**Build Status**: ? Successful  
**Version**: 1.0  
**Status**: Production Ready

For help: Review docs ? Check console ? Contact dev team
