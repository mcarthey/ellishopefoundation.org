# ?? Page Manager - Visual Guide

**Quick Reference for Content Editors**

---

## ?? Green Badge = Your Custom Content

```
???????????????????????????????????
? Hero Banner          [Managed]  ? ? Green badge
? ??????????????????????????????? ?
? ?? Your custom image here       ?
? ? Customized via Media Library  ?
?                                 ?
? Change to: [my-hero.jpg    ?]  ?
? [Update] [? Revert]            ?
???????????????????????????????????
```

**What this means:**
- ? You've uploaded/selected a custom image
- ? Image is in Media Library (`/uploads/`)
- ? You control this content
- ? Can revert to template default anytime

**Actions:**
- **Update:** Change to different image from Media Library
- **Revert:** Go back to template default

---

## ?? Yellow Badge = Template Default

```
???????????????????????????????????
? About Image      [Template]     ? ? Yellow badge
? ??????????????????????????????? ?
? ?? Template default image       ?
? ? Using template default:       ?
?   /assets/img/about-us.jpg      ?
?                                 ?
? Change to: [Select image   ?]  ?
? [Update]                        ?
???????????????????????????????????
```

**What this means:**
- ?? Using original template image
- ?? Image from website design (`/assets/`)
- ?? Not yet customized by you
- ?? Image path shown for reference

**Actions:**
- **Update:** Select from Media Library to customize

---

## ?? Understanding the Colors

### ?? Green (Managed)
```
Status:  Customized
Source:  Media Library
Path:    /uploads/media/your-image.jpg
Control: Full (can update or revert)
```

### ?? Yellow (Template)
```
Status:  Default
Source:  Template design
Path:    /assets/img/template-image.jpg
Control: Can customize by selecting from Media Library
```

---

## ?? Common Scenarios

### Scenario 1: First Time Editing

**You see:** All yellow badges  
**This means:** Site is using all template defaults  
**What to do:** 
1. Decide which images to customize
2. Upload new images to Media Library
3. Select them in dropdowns
4. Click "Update"

---

### Scenario 2: Some Customization

**You see:** Mix of green and yellow badges  
**This means:** Some images customized, others still template  
**What to do:**
- Green ones are done ?
- Yellow ones can be customized if needed
- No rush - both work fine

---

### Scenario 3: Made a Mistake

**You see:** Green badge but want original back  
**This means:** You customized but want template default  
**What to do:**
1. Click "Revert" button
2. Confirm you want to revert
3. Badge turns yellow ? back to template

---

## ?? Quick Tips

### ? **DO:**
- Start with high-priority images (hero banners)
- Upload quality images to Media Library first
- Preview changes on live site
- Use "Revert" if you change your mind

### ? **DON'T:**
- Rush to replace everything at once
- Worry about yellow badges - template works fine
- Delete template images (they're fallbacks)
- Upload huge image files (optimize first)

---

## ?? Reading the Interface

### Image Card Example
```
???????????????????????????????????????????
? Hero Banner Image          [Managed]    ? ? 1. Title and Status
? ???????????????????????????????????????  ?
? ?? [Image preview shown here]            ? ? 2. Visual preview
? ? Customized via Media Library           ? ? 3. Source indicator
?                                          ?
? Change to:                               ?
? [Select from dropdown ?]                 ? ? 4. Selection dropdown
?                                          ?
? [Update]  [Revert]                       ? ? 5. Action buttons
???????????????????????????????????????????
```

**1. Title and Status:**
- Name tells you what the image is for
- Badge shows managed (green) or template (yellow)

**2. Visual Preview:**
- See the actual image
- Confirms what's currently displayed

**3. Source Indicator:**
- Green ?: "Customized via Media Library"
- Yellow ?: "Using template default: /path/to/image.jpg"

**4. Selection Dropdown:**
- List of images from Media Library
- Select new image to change

**5. Action Buttons:**
- **Update:** Apply selected image
- **Revert:** Go back to template (only on green/managed)

---

## ?? Decision Flow

```
Is the template image good?
?
?? YES ? Keep it (stay yellow)
?         OR
?         Upload exact same to Media Library
?         (to make it managed/green)
?
?? NO ? Need better image
         ?
         ?? Have image?
         ?  ?? Upload to Media Library
         ?     Select in dropdown
         ?     Click Update
         ?     ? Now green/managed
         ?
         ?? Don't have image?
            ?? Browse Unsplash for free image
               Import to Media Library
               Select in dropdown
               Click Update
               ? Now green/managed
```

---

## ?? Common Questions

### Q: What does "Template (not managed)" mean?
**A:** The image is part of the original website design, stored in `/assets/img/`. It's not managed through the Media Library yet.

### Q: Should I replace all yellow badges?
**A:** No! Only replace images that don't fit your needs. Template images are perfectly functional.

### Q: How do I know which images to customize?
**A:** Focus on:
- Hero banners (most visible)
- Brand-specific images
- Images with wrong message/theme
- Low quality images

### Q: What happens if I click "Revert"?
**A:** Your custom image is removed and the template default is used again. Your custom image stays in Media Library, just not used on this page.

### Q: Can I undo changes?
**A:** Yes! Click "Revert" to go back to template default. Or select a different image and "Update" to change it.

### Q: Why show the file path for template images?
**A:** For transparency - you can see exactly where the current image is coming from.

---

## ?? At a Glance

| Badge Color | Status | Source | Can Edit? | Can Revert? |
|-------------|--------|--------|-----------|-------------|
| ?? Green | Managed | Media Library | ? Yes | ? Yes |
| ?? Yellow | Template | Template design | ? Yes | ? No (already at default) |

---

## ?? Learning Path

### Beginner (First Day)
1. Understand green vs yellow badges
2. View page editor, don't change anything
3. Look at what's currently displayed
4. Identify 1-2 images you want to change

### Intermediate (First Week)
1. Upload 1-2 images to Media Library
2. Replace 1-2 template images
3. See them turn from yellow to green
4. Preview on live site

### Advanced (Ongoing)
1. Gradually migrate more images
2. Organize Media Library
3. Use Unsplash for stock photos
4. Help others understand the system

---

**Remember:** Yellow badges are okay! They mean the site is working with template defaults. You only need to customize what doesn't fit your needs. ??

