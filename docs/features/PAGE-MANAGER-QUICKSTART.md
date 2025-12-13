# Page Content Manager - Quick Start Checklist

## ? System is Ready!

The Page Content Manager has been successfully implemented and is ready to use.

## What You Can Do Now

### 1. Access the Page Manager
```
URL: https://localhost:7049/admin/pages
Login: Use your admin credentials
```

### 2. Default Pages Created
The system has automatically created these pages:
- [ ] Home
- [ ] About  
- [ ] Team
- [ ] Services
- [ ] Contact

### 3. Edit Your First Page (Example: Team Page)

**Step A: Add Header Image**
1. Go to `/admin/pages`
2. Click "Edit Content" on "Team" page
3. Scroll to "Add New Image"
4. Enter details:
   - Image Key: `HeaderBackground`
   - Select Media: Choose your Unsplash image (`xh4mG4cqHGg.jpg`)
   - Display Order: `0`
5. Click "Add Image"
6. ? Header image is now configured!

**Step B: Add Welcome Text**
1. Scroll to "Add New Content Section"
2. Enter details:
   - Section Key: `WelcomeText`
   - Content Type: `Rich Text`
   - Content: `<h2>Meet Our Amazing Volunteers</h2><p>Our team is dedicated to making a difference...</p>`
3. Click "Add Section"
4. ? Welcome text is now configured!

**Step C: Update Your Team View**
Edit `Views/Team/v1.cshtml` to use the page content:

```razor
@inject EllisHope.Services.IPageService PageService
@{
    var page = await PageService.GetPageByNameAsync("Team");
    var headerImage = page?.PageImages.FirstOrDefault(i => i.ImageKey == "HeaderBackground");
    var welcomeText = page?.ContentSections.FirstOrDefault(s => s.SectionKey == "WelcomeText");
}

<!-- Use the header image -->
<div class="page-title-area">
    <div class="breadcrumb-wrapper" 
         data-background="@(headerImage?.Media.FilePath ?? "/assets/img/page-title/page-title-bg.jpg")">
        <!-- rest of your header code -->
    </div>
</div>

<!-- Use the welcome text -->
<section class="team-section pt-180 pt-lg-100 pb-125 pb-lg-15">
    <div class="container">
        <div class="title-three text-center mb-70">
            @if (welcomeText != null)
            {
                @Html.Raw(welcomeText.Content)
            }
            else
            {
                <h5 class="sub-title before-none wow fadeInUp">Team</h5>
                <h2 class="title wow fadeInUp" data-wow-delay="0.1">
                    <span class="position-relative">
                        Meet<img class="title-img" src="~/assets/img/shape/shape-11.png" alt="shape">
                    </span> our Volunteers
                </h2>
            }
        </div>
        <!-- rest of your team members code -->
    </div>
</section>
```

## Quick Reference

### Common Section Keys
```
WelcomeText          - Page introduction
AboutDescription     - About/mission text
CallToAction         - CTA text
ContactInfo          - Contact information
ServicesOverview     - Services description
TeamIntro            - Team introduction
```

### Common Image Keys
```
HeroImage            - Main banner image
HeaderBackground     - Page header background
AboutImage           - About section image
TeamPhoto            - Team photo
Logo                 - Logo image
IconXxx              - Icons (IconHealth, IconEducation, etc.)
```

### URLs
```
Admin Pages List:    /admin/pages
Edit Page:           /admin/pages/edit/{id}
Media Library:       /admin/media
Upload Media:        /admin/media/upload
Unsplash Search:     /admin/media/unsplashsearch
```

## Testing Checklist

- [ ] Can access `/admin/pages`
- [ ] Can see list of 5 default pages
- [ ] Can click "Edit Content" on a page
- [ ] Can add an image from Media Library
- [ ] Can add a content section
- [ ] Can edit existing content
- [ ] Can change an image using Media Picker
- [ ] Can remove an image
- [ ] Content saves successfully
- [ ] Success messages appear
- [ ] Changes reflect on public site

## Troubleshooting

### Can't See Pages List
**Problem**: 404 or Access Denied  
**Solution**: Ensure you're logged in with Admin or Editor role

### No Images in Media Library
**Problem**: Dropdown is empty  
**Solution**: First upload images at `/admin/media/upload` or import from Unsplash

### Content Not Showing on Site
**Problem**: Added content but doesn't appear  
**Solution**: Update your view files to use `IPageService` (see Step C above)

### TinyMCE Not Loading
**Problem**: Rich text editor doesn't appear  
**Solution**: Check TinyMCE API key in `appsettings.json`

## Next Steps

### For Non-Technical Users
1. ? Log in to admin panel
2. ? Explore existing pages
3. ? Add content to 1-2 pages
4. ? Upload more images to Media Library
5. ? Experiment with Rich Text editor

### For Developers
1. ? Update existing views to use PageService
2. ? Add more page-specific Section/Image Keys
3. ? Create helper methods for common patterns
4. ? Consider caching page content
5. ? Add more default pages as needed

## Documentation

- **User Guide**: `docs/features/page-content-manager-guide.md`
- **Implementation Summary**: `docs/features/page-content-manager-summary.md`
- **Media Library Guide**: `docs/features/media-library-guide.md` (to be created)

## Support

If you encounter issues:
1. Check browser console for errors
2. Check application logs
3. Verify Media Library has images
4. Ensure you have Admin/Editor role
5. Review documentation in `/docs/features/`

---

**Status**: ? Ready for Use  
**Build**: ? Successful  
**Tests**: ?? Manual testing required  
**Documentation**: ? Complete  

**Start editing your pages now at `/admin/pages`!** ??
