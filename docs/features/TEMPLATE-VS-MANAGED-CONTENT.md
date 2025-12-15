# Template vs Managed Content - Solution

**Date:** December 14, 2024  
**Issue:** Page Manager showed "no images" even though site had images  
**Status:** ? **Solved**

---

## ?? **The Problem**

### Two Image Locations
1. **Template Images:** `/wwwroot/assets/img/` (from Kuki template)
   - Hardcoded in views
   - Not in database
   - Not in Media Library

2. **Managed Images:** `/wwwroot/uploads/media/` (Media Library)
   - Uploaded via admin panel
   - Stored in database
   - Managed through Media Library

### The Disconnect
- Site displays template images (from `/assets/`)
- Page Manager only looks for managed images (from `/uploads/`)
- **Result:** Admin shows "no images" even though images appear on site

---

## ? **The Solution: Hybrid Display System**

Show BOTH template and managed content, with clear visual distinction:

### Visual Indicators

**?? Green Border = Managed**
```
??????????????????????????????
? Hero Banner Image     [?]  ? ? Green badge: "Managed"
? ??????????????????????????  ?
? ?? [Your custom image]      ?
? ? Customized via Media      ?
?   Library                   ?
??????????????????????????????
```

**?? Yellow Border = Template**
```
??????????????????????????????
? About Image           [?]  ? ? Yellow badge: "Template"
? ??????????????????????????  ?
? ?? [Template default image] ?
? ? Using template default:   ?
?   /assets/img/about.jpg     ?
?                             ?
? [Select from Media Library] ?
? [Update]                    ?
??????????????????????????????
```

---

## ?? **How It Works**

### 1. Template Service Tracks Both

**Before:**
```csharp
new EditableImage { 
    Key = "HeroBanner", 
    Label = "Hero Banner Image"
}
```

**After:**
```csharp
new EditableImage { 
    Key = "HeroBanner", 
    Label = "Hero Banner Image",
    CurrentTemplatePath = "/assets/img/hero/hero-img-6.png",  // Template location
    FallbackPath = "/assets/img/hero/hero-img-6.png"
}
```

### 2. Smart Image Resolution

```csharp
public class EditableImage
{
    // Database-managed (Media Library)
    public string? CurrentImagePath { get; set; }
    public int? CurrentMediaId { get; set; }
    
    // Template default
    public string? CurrentTemplatePath { get; set; }
    public string? FallbackPath { get; set; }
    
    // Helper: What actually gets displayed
    public string EffectiveImagePath => 
        CurrentImagePath ??           // 1st: Use custom (if set)
        CurrentTemplatePath ??         // 2nd: Use template default
        FallbackPath ??                // 3rd: Use fallback
        "/assets/img/default.jpg";     // 4th: Generic default
    
    // Helper: Where is this coming from?
    public string ImageSource => 
        CurrentImagePath != null ? "Media Library" : 
        CurrentTemplatePath != null ? "Template (not managed)" : 
        "No image";
    
    // Helper: Is this managed?
    public bool IsManagedImage => CurrentMediaId.HasValue;
}
```

### 3. Visual Display Logic

```razor
@if (image.IsManagedImage)
{
    <span class="badge bg-success">Managed</span>
    <small>? Customized via Media Library</small>
}
else if (!string.IsNullOrEmpty(image.CurrentTemplatePath))
{
    <span class="badge bg-warning">Template</span>
    <small>? Using template default: <code>@image.CurrentTemplatePath</code></small>
}
```

---

## ?? **User Experience**

### When User Opens Page Editor

#### Scenario 1: Fresh Install (No Customization Yet)
```
All images show:
- ?? Yellow "Template" badge
- Current template image displayed
- Warning: "Using template default"
- Can select new image from Media Library
```

#### Scenario 2: Some Customization
```
Mixed display:
- ?? Green for customized images (from Media Library)
- ?? Yellow for template defaults (not yet customized)
- Clear indication which is which
```

#### Scenario 3: Fully Customized
```
All images show:
- ?? Green "Managed" badge
- Custom images displayed
- "Revert" button to go back to template
```

---

## ?? **Migration Path**

### Users Can Choose:

**Option 1: Keep Template Images**
- Do nothing
- Site continues using template images
- Yellow badges show which are template

**Option 2: Gradually Migrate**
- Replace images one at a time
- Mix of green (managed) and yellow (template)
- No rush to migrate everything

**Option 3: Full Migration**
- Replace all template images
- Upload better images to Media Library
- All images managed in one place

**Option 4: Revert**
- Click "Revert" to go back to template default
- Removes custom image, uses template again

---

## ?? **Benefits**

### ? **Solves Original Problem**
- Shows ALL images (template + managed)
- No more confusion about "missing" images
- Clear indication of image source

### ? **No Forced Migration**
- Template images still work
- Users can migrate when ready
- No breaking changes

### ? **Clear Visual Feedback**
- ?? Green = Custom/Managed
- ?? Yellow = Template/Default
- Easy to see what's customized

### ? **Flexible**
- Keep template images if they're good
- Replace only what needs changing
- Revert to template if needed

### ? **Educational**
- Shows file paths for template images
- Users learn where content comes from
- Transparency in content management

---

## ?? **Example: Home Page Hero Banner**

### Initial State (Template)
```
???????????????????????????????????????
? Hero Banner Image            [?]   ?
? ???????????????????????????????????  ?
? ?? [hero-img-6-unsplash.png]        ?
?                                     ?
? ? Using template default:           ?
?   /assets/img/hero/                 ?
?   hero-img-6-unsplash.png           ?
?                                     ?
? Change to: [Select Media Library ?] ?
? [Update]                            ?
???????????????????????????????????????
```

### After Customization (Managed)
```
???????????????????????????????????????
? Hero Banner Image            [?]   ?
? ???????????????????????????????????  ?
? ?? [my-custom-hero.jpg]             ?
?                                     ?
? ? Customized via Media Library      ?
?                                     ?
? Change to: [my-custom-hero.jpg ?]   ?
? [Update]  [? Revert to Template]   ?
???????????????????????????????????????
```

### After Revert (Back to Template)
```
???????????????????????????????????????
? Hero Banner Image            [?]   ?
? ???????????????????????????????????  ?
? ?? [hero-img-6-unsplash.png]        ?
?                                     ?
? ? Using template default:           ?
?   /assets/img/hero/                 ?
?   hero-img-6-unsplash.png           ?
?                                     ?
? Change to: [Select Media Library ?] ?
? [Update]                            ?
???????????????????????????????????????
```

---

## ?? **Best Practices**

### For Content Editors

**Start Gradually:**
1. Don't replace everything at once
2. Start with 1-2 key images
3. Test on live site
4. Continue as needed

**Use Template as Reference:**
- Template shows what's currently live
- Good baseline for size/style
- Can keep if it works well

**Organize Media Library:**
- Upload images with good filenames
- Add descriptions/alt text
- Use categories

### For Developers

**View Integration:**
```csharp
// In views, use this pattern:
@inject IPageService PageService
@{
    var page = await PageService.GetPageByNameAsync("Home");
    var heroImage = page?.PageImages.FirstOrDefault(i => i.ImageKey == "HeroBanner");
}

<!-- Fallback chain: Custom ? Template ? Default -->
<div data-background="@(heroImage?.Media?.FilePath ?? "/assets/img/hero/hero-img-6.png")">
    <!-- Hero content -->
</div>
```

**Or use helper:**
```csharp
// If template service is available
@inject IPageTemplateService TemplateService
@{
    var template = TemplateService.GetPageTemplate("Home");
    var heroConfig = template.Images.First(i => i.Key == "HeroBanner");
}

<div data-background="@heroConfig.EffectiveImagePath">
    <!-- Will show custom, template, or fallback -->
</div>
```

---

## ?? **Understanding the Code**

### Key Properties

**`CurrentImagePath`** (Media Library)
- Path to custom uploaded image
- Example: `/uploads/media/abc123.jpg`
- Only set if user customized

**`CurrentTemplatePath`** (Template Default)
- Path to original template image
- Example: `/assets/img/hero/hero-img-6.png`
- Always set by PageTemplateService

**`EffectiveImagePath`** (What's Actually Used)
- Smart resolution: Custom ? Template ? Fallback
- What gets displayed on the page
- Null-safe with fallback chain

**`IsManagedImage`** (Status Flag)
- `true` = User customized (has CurrentMediaId)
- `false` = Using template default
- Drives UI styling (green vs yellow)

---

## ?? **Troubleshooting**

### Q: Image shows wrong path in admin
**A:** Check if Media Library has the image. If not, it's showing template default (expected behavior).

### Q: Changed image but site still shows old one
**A:** Clear browser cache. Or check if view is still hardcoded (needs to use PageService).

### Q: "Template" badge but I want "Managed"
**A:** Upload image to Media Library, then select it in dropdown and click Update.

### Q: How to remove custom image?
**A:** Click "Revert" button. This removes custom image and shows template default again.

### Q: Can I delete template images?
**A:** Not recommended until ALL pages are migrated. Keep `/assets/img/` as fallback.

---

## ?? **Migration Checklist**

### Phase 1: Understand Current State
- [ ] Log in to `/admin/pages`
- [ ] Open each page editor
- [ ] Note which images are ?? Yellow (template)
- [ ] Note which images are ?? Green (managed)

### Phase 2: Evaluate Template Images
- [ ] Are template images good quality?
- [ ] Do they match brand/messaging?
- [ ] Which ones need replacing?

### Phase 3: Prepare Replacements
- [ ] Upload new images to Media Library
- [ ] Add alt text and descriptions
- [ ] Ensure proper dimensions

### Phase 4: Gradual Migration
- [ ] Replace high-priority images first (hero banners)
- [ ] Test each change on live site
- [ ] Continue with lower-priority images
- [ ] Monitor for broken images

### Phase 5: Optional Cleanup
- [ ] Once fully migrated, can archive `/assets/img/`
- [ ] Update views to use PageService everywhere
- [ ] Remove hardcoded image paths

---

## ? **Summary**

**Problem:** Page Manager couldn't see template images  
**Solution:** Show both template and managed content with clear visual distinction

**Key Features:**
- ? Shows ALL images (template + managed)
- ? Clear visual indicators (green/yellow)
- ? File path transparency
- ? Flexible migration path
- ? Revert capability
- ? No forced migration

**Result:** Users now see exactly what's on the site and can manage it effectively!

---

**Status:** ? Implemented  
**Build:** ? Successful  
**User Experience:** ? Clear and intuitive  
**Migration:** ? Optional and gradual

