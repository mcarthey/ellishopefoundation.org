# Media Manager User Guide

## Quick Start

The Media Manager provides centralized image storage and management for your entire website. Instead of uploading images separately for each blog post, event, or cause, you can now:

- Upload images once to the Media Library
- Reuse them across multiple pages
- Browse and search your entire image collection
- Import professional photos from Unsplash

## Accessing the Media Manager

1. Log into the Admin Panel
2. Click **Media** in the navigation menu
3. You'll see all your uploaded and imported images

## Uploading Images

### Method 1: Through Media Library (Recommended)

1. Click **Media** in admin navigation
2. Click **Upload Image** button
3. Fill out the form:
   - **Select Image**: Choose file from your computer
   - **Title**: Optional descriptive title
   - **Alt Text**: Description for accessibility (auto-filled if left blank)
   - **Caption**: Optional caption
   - **Category**: Choose appropriate category:
     - Hero: Large header images
     - Page: General page content
     - Blog: Blog post images
     - Event: Event photos
     - Cause: Cause/campaign images
     - Team: Team member photos
     - Gallery: Gallery images
     - Uncategorized: Other images
   - **Tags**: Comma-separated keywords (e.g., "children, education, fundraising")
4. Click **Upload Image**

**Benefits**:
- Image appears in Media Library
- Searchable and reusable
- Properly categorized
- Includes metadata

### Method 2: Direct Upload (Not Recommended)

When creating/editing Blog posts, Events, or Causes:
1. Scroll to Featured Image section
2. Click **Upload New** button
3. Select file from computer
4. Save content

**Drawbacks**:
- Image not added to Media Library
- Cannot be reused
- No search/categorization
- No metadata tracking

> **?? Tip**: Always use Method 1 for better organization!

## Importing from Unsplash

Unsplash provides millions of free, professional-quality photos:

1. Click **Media** ? **Browse Unsplash**
2. Enter search term (e.g., "children playing", "education", "healthcare")
3. Click **Search Unsplash**
4. Browse results
5. Click **Import** on desired photo
6. Fill out the import form:
   - **Alt Text**: Auto-filled from Unsplash description
   - **Category**: Choose appropriate category
   - **Tags**: Add your own tags
7. Click **Import Photo**

**Notes**:
- Photographer credit is automatically tracked
- High-quality images (typically 1920×1080 or larger)
- Free to use with attribution
- Added to your Media Library immediately

## Using Images in Content

### When Creating Blog Posts, Events, or Causes

1. Navigate to Create form (e.g., Admin ? Blog ? Create)
2. Scroll to **Featured Image** section
3. Click **Browse Media Library**
4. Use the modal to find your image:
   - **Search**: Type filename, title, or tags
   - **Category**: Filter by image category
   - **Browse**: Scroll through grid of images
5. Click on desired image
6. Image appears in preview
7. Continue filling out form
8. Click **Save**

### Quick Actions in Media Library Modal

- **Upload to Media Library**: Opens upload page in new tab
- **Import from Unsplash**: Opens Unsplash search in new tab
- After uploading, return to original tab and refresh the modal

## Managing Your Media Library

### Searching Images

In the Media Library:
1. Use the search box at top
2. Search finds matches in:
   - Filenames
   - Titles
   - Alt text
   - Tags

### Filtering by Category

1. Use the **Category** dropdown
2. Select category to see only those images
3. Select "All Categories" to see everything

### Filtering by Source

1. Use the **Source** dropdown
2. Options:
   - Local: Images you uploaded
   - Unsplash: Imported from Unsplash

### Editing Image Metadata

1. Click **Edit** on any image
2. Update:
   - Alt Text (for SEO and accessibility)
   - Title
   - Caption
   - Category
   - Tags
3. Click **Update**

**Cannot change**:
- Actual image file
- Dimensions
- File size
- Upload date

### Deleting Images

1. Click **Delete** on image
2. Confirm deletion

**?? Warning**: 
- Check "Where Used" first to avoid breaking pages
- Deleted images cannot be recovered
- Use "Force Delete" only if you're sure

### Viewing Image Usage

Want to know where an image is used?

1. Click **Where Used** on any image
2. See list of all pages using this image:
   - Blog posts
   - Events
   - Causes
   - Other content

**Useful for**:
- Avoiding accidental deletion
- Finding orphaned images
- Content auditing

## Best Practices

### For Better Organization

1. **Use descriptive filenames**
   - Good: `children-education-classroom.jpg`
   - Bad: `IMG_1234.jpg`

2. **Always add alt text**
   - Helps with accessibility
   - Improves SEO
   - Makes images searchable

3. **Use appropriate categories**
   - Easier to find images later
   - Better organized library
   - Faster browsing

4. **Tag generously**
   - Multiple tags okay
   - Think of search terms
   - Include themes, colors, subjects

### For Better Performance

1. **Optimize images before upload**
   - Resize large images
   - Compress when possible
   - Target ~200KB per image

2. **Use appropriate dimensions**
   - Hero: 1920×1080 or larger
   - Blog/Events/Causes: 1200×800
   - Thumbnails: Auto-generated

3. **Delete unused images**
   - Check "Where Used" regularly
   - Remove old/outdated images
   - Keep library manageable

## Common Workflows

### Adding an Event with Photo

1. Go to Admin ? Events ? Create
2. Fill in event details (title, date, location, etc.)
3. In Featured Image section:
   - Click **Browse Media Library**
   - Search for "event" or similar
   - Select appropriate image
   - Or click "Upload to Media Library" to add new
4. Save event

### Creating Blog Post with Unsplash Image

1. Go to Admin ? Media ? Browse Unsplash
2. Search for topic (e.g., "technology")
3. Find and import desired image
4. Go to Admin ? Blog ? Create
5. Fill in blog details
6. In Featured Image section:
   - Click **Browse Media Library**
   - Find recently imported Unsplash image
   - Select it
7. Write post content
8. Save

### Reusing Image Across Multiple Causes

1. Upload/import image once to Media Library
2. Create first cause:
   - Browse Media Library
   - Select image
   - Save
3. Create second cause:
   - Browse Media Library
   - Select same image
   - Save
4. Same image used in both causes!

**Benefits**:
- No duplicate uploads
- Consistent imagery
- Easier to update (change once, affects all)

## Tips & Tricks

### Finding Recently Uploaded Images

Images are sorted by upload date (newest first) by default.

### Reusing Featured Images

Can't remember which blog post had that great photo?
1. Go to Media Library
2. Find the image
3. Click "Where Used"
4. See all usage locations

### Quick Category Assignment

When uploading, think ahead:
- Blog post images ? Blog category
- Event photos ? Event category
- General site images ? Page category

### Bulk Tagging (Future Feature)

Currently tags must be added one image at a time. Bulk operations coming soon!

## Troubleshooting

### "Image not found" error

**Solution**: 
1. Check if image was deleted from Media Library
2. Check if file exists in `/uploads/media/`
3. Re-upload if necessary

### Can't find recently uploaded image

**Check**:
1. Is category filter correct?
2. Clear search box
3. Select "All Categories"
4. Refresh page

### Image doesn't appear in content

**Check**:
1. Did you click on image in modal?
2. Does preview show in form?
3. Did you save the content?
4. Clear browser cache

### Upload fails

**Common causes**:
1. File too large (max 10MB)
2. Wrong file type (must be image)
3. Network connection issue

**Solutions**:
1. Resize/compress image
2. Use JPG, PNG, GIF, or WebP
3. Try again

## Getting Help

### Documentation
- Media Manager Integration Guide (developers)
- This user guide
- Video tutorials (coming soon)

### Support
Contact your system administrator if:
- Images won't upload
- Media Library won't load
- Errors persist after troubleshooting

## Feature Requests

Want to see new features in the Media Manager?
- Suggest via GitHub Issues
- Contact development team
- Check roadmap for planned features

---

**Last Updated**: December 2024  
**Version**: 1.0
