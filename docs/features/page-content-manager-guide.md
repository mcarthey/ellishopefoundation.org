# Page Content Manager - User Guide

## Overview

The Page Content Manager is a comprehensive CMS (Content Management System) that allows non-technical users to edit website content, including text, images, and media without touching code.

## Features

### ? Visual Page Editor
- Edit any page on your website through an intuitive admin interface
- Manage text content with rich text editor (TinyMCE)
- Add and manage images from your Media Library
- Organize content into sections with unique keys

### ? Media Integration
- Seamlessly integrates with the Media Library
- Select images from uploaded or Unsplash-sourced media
- Visual image picker for easy selection
- Automatic image management and tracking

### ? Content Sections
- Create multiple content sections per page
- Three content types:
  - **Rich Text**: Full HTML editor with formatting
  - **Plain Text**: Simple text without formatting
  - **Raw HTML**: For advanced users

## Architecture

### Database Structure

```
Pages
??? Id (Primary Key)
??? PageName (Unique identifier, e.g., "Home", "About")
??? Title (Page title for SEO)
??? MetaDescription (SEO meta description)
??? IsPublished (Published/Draft status)
??? CreatedDate
??? ModifiedDate
??? ContentSections (Collection)
??? PageImages (Collection)

ContentSections
??? Id
??? PageId (Foreign Key to Pages)
??? SectionKey (e.g., "WelcomeText", "AboutDescription")
??? Content (Text/HTML content)
??? ContentType ("Text", "RichText", "HTML")
??? DisplayOrder

PageImages
??? Id
??? PageId (Foreign Key to Pages)
??? MediaId (Foreign Key to MediaLibrary)
??? ImageKey (e.g., "HeroImage", "HeaderBackground")
??? DisplayOrder
??? Media (Navigation property)
```

### Service Layer

**`IPageService`** - Main interface for page management:
- `GetAllPagesAsync()` - List all pages
- `GetPageByIdAsync(id)` - Get specific page
- `GetPageByNameAsync(pageName)` - Get page by name
- `UpdateContentSectionAsync()` - Update text content
- `SetPageImageAsync()` - Assign image to page
- `RemovePageImageAsync()` - Remove image from page

## User Guide

### Accessing the Page Manager

1. Log in to Admin Panel: `/admin/account/login`
2. Navigate to **Pages** in the sidebar
3. You'll see a list of all website pages

### Editing a Page

#### Step 1: Select Page
1. From the Pages list, find the page you want to edit
2. Click **"Edit Content"** button
3. You'll see two main sections: Images and Content

#### Step 2: Managing Images

**Add a New Image:**
1. Scroll to "Add New Image" section
2. Enter an **Image Key** (e.g., "HeroImage", "TeamPhoto")
   - This identifies where the image is used
   - Use descriptive names like "HeaderBackground", "AboutImage"
3. Select image from **Media Library** dropdown
4. Set **Display Order** (0 = first, 1 = second, etc.)
5. Click **"Add Image"**

**Change an Existing Image:**
1. Find the image card you want to change
2. Click **"Change"** button
3. You'll see the Media Picker with all available images
4. Click **"Select This"** on your preferred image
5. System automatically updates the page

**Remove an Image:**
1. Find the image card
2. Click **"Remove"** button
3. Confirm the removal

#### Step 3: Managing Content Sections

**Edit Existing Content:**
1. Scroll to "Content Sections"
2. Find the section you want to edit (e.g., "WelcomeText")
3. Edit content in the text area or rich text editor
4. Click **"Update [SectionKey]"** button

**Add New Content Section:**
1. Scroll to "Add New Content Section"
2. Enter **Section Key** (e.g., "IntroText", "MissionStatement")
3. Select **Content Type**:
   - **Rich Text**: For formatted content (bold, italics, lists, etc.)
   - **Plain Text**: For simple text
   - **Raw HTML**: For advanced HTML code
4. Enter your content
5. Click **"Add Section"**

### Example Workflow: Editing Team Page Header

1. Go to **Admin > Pages**
2. Click **"Edit Content"** on "Team" page
3. Add Header Image:
   - Image Key: `HeaderBackground`
   - Select: Your uploaded Unsplash image
   - Display Order: `0`
   - Click "Add Image"
4. Add Welcome Text:
   - Section Key: `WelcomeText`
   - Content Type: `Rich Text`
   - Content: "Meet our amazing volunteers!"
   - Click "Add Section"
5. Result: Header image and text now appear on `/team/v1` page

## Section Keys and Image Keys

### Recommended Naming Conventions

**Image Keys:**
- `HeroImage` - Main banner/hero image
- `HeaderBackground` - Page header background
- `AboutImage` - About section image
- `TeamPhoto` - Team/group photo
- `Logo` - Company/organization logo
- `IconXxx` - Icons (IconEducation, IconHealth, etc.)

**Section Keys:**
- `WelcomeText` - Welcome/intro text
- `AboutDescription` - About section content
- `MissionStatement` - Mission statement
- `VisionStatement` - Vision statement
- `ServicesOverview` - Services description
- `ContactInfo` - Contact information
- `CallToAction` - CTA text

## Content Types Explained

### Rich Text
- **Use When**: You need formatted content with bold, italics, headings, lists
- **Features**: Full TinyMCE editor with toolbar
- **Example**: Blog post content, about us descriptions

### Plain Text
- **Use When**: Simple text without formatting
- **Features**: Basic textarea
- **Example**: Short quotes, simple descriptions

### Raw HTML
- **Use When**: You need custom HTML (advanced users only)
- **Features**: Direct HTML input
- **Example**: Embedded widgets, custom layouts

## Media Library Integration

### How It Works
1. All images must first be uploaded to **Media Library**
2. From there, you can assign them to any page
3. Each image can be used on multiple pages
4. Removing from a page doesn't delete from Media Library

### Best Practices
1. **Upload First**: Always upload/import images to Media Library before editing pages
2. **Use Descriptive Names**: Give images meaningful titles and alt text
3. **Organize by Category**: Use Media Library categories (Hero, Page Headers, etc.)
4. **Optimize Images**: Use appropriate image sizes (handled automatically)

## Default Pages

The system automatically creates these pages on first run:
- **Home** - Homepage content
- **About** - About us page
- **Team** - Team/volunteers page
- **Services** - Services page
- **Contact** - Contact page

## Tips for Non-Technical Users

### ? DO:
- Use descriptive keys (e.g., "WelcomeHeading", not "Text1")
- Keep section keys consistent across pages
- Use Rich Text for formatted content
- Preview changes before saving (view site in new tab)
- Use Media Library for all images

### ? DON'T:
- Use spaces in keys (use "AboutText", not "About Text")
- Delete sections/images without confirming
- Use special characters in keys (@, #, $, etc.)
- Upload images directly to server - use Media Library
- Mix content types (stick to one type per section)

## Troubleshooting

### Image Not Appearing on Page
1. **Check**: Is image uploaded to Media Library?
2. **Check**: Is Image Key correct?
3. **Check**: Is page view actually using that Image Key?
4. **Solution**: Verify Image Key matches what's in your view template

### Content Not Saving
1. **Check**: Did you click "Update" button?
2. **Check**: Do you have Editor/Admin permissions?
3. **Check**: Is Section Key unique on this page?
4. **Solution**: Try again with unique Section Key

### Can't Find Image in Media Library
1. **Go to**: Admin > Media > Upload
2. **Or**: Admin > Media > Unsplash Search
3. Upload/import image first
4. Return to Pages and select it

## Security

### Who Can Edit Pages?
- Only users with **Admin** or **Editor** roles
- Regular users cannot access Page Manager
- All changes are logged with user info

### What's Protected?
- ? Page structure and code
- ? Database relationships
- ? System pages (protected from deletion)
- ? Media Library files

## API/Developer Reference

### Accessing Page Content in Views

```csharp
// In your controller
public async Task<IActionResult> TeamPage()
{
    var page = await _pageService.GetPageByNameAsync("Team");
    return View(page);
}

// In your view
@model EllisHope.Models.Domain.Page

<!-- Display content section -->
@{
    var welcomeText = Model.ContentSections
        .FirstOrDefault(s => s.SectionKey == "WelcomeText");
}
@if (welcomeText != null)
{
    @Html.Raw(welcomeText.Content)
}

<!-- Display page image -->
@{
    var heroImage = Model.PageImages
        .FirstOrDefault(i => i.ImageKey == "HeroImage");
}
@if (heroImage != null)
{
    <img src="@heroImage.Media.FilePath" alt="@heroImage.Media.AltText" />
}
```

### Service Methods

```csharp
// Get page content section
var section = await _pageService.GetContentSectionAsync(pageId, "WelcomeText");

// Update content section
await _pageService.UpdateContentSectionAsync(
    pageId, 
    "WelcomeText", 
    "New content here", 
    "RichText"
);

// Set page image
await _pageService.SetPageImageAsync(
    pageId, 
    "HeroImage", 
    mediaId, 
    displayOrder: 0
);

// Get all page images
var images = await _pageService.GetPageImagesAsync(pageId);
```

## Future Enhancements

Planned features:
- [ ] Page templates for common layouts
- [ ] Version history/rollback
- [ ] Content preview before saving
- [ ] Bulk operations (copy content between pages)
- [ ] Content scheduling (publish at specific time)
- [ ] Multi-language support
- [ ] Page permissions (per-page access control)

## Summary

The Page Content Manager provides:
- ? Easy content editing for non-technical users
- ? Visual media management
- ? Flexible content organization
- ? Integration with Media Library
- ? Rich text editing capabilities
- ? Safe, permission-based access

**All without touching code!** ??

---

**Related Documentation:**
- [Media Library Guide](./media-library-guide.md)
- [Admin Panel Overview](./admin-panel-overview.md)
- [Security & Permissions](../security/admin-security.md)
