# Media Migration Tool - User Guide

## Overview

The Media Migration Tool helps you consolidate all legacy images from scattered directories into the centralized Media Library system. This tool:

- ? Moves images from `/uploads/blog/`, `/uploads/events/`, `/uploads/causes/`, and `/Unsplash/` to `/uploads/media/`
- ? Adds all images to the Media Library database
- ? Detects and skips duplicate images
- ? Updates all database references automatically
- ? Optionally deletes old files after migration

## Accessing the Migration Tool

1. Log in to the Admin area
2. Navigate to **Admin ? Media Migration**
3. You'll see a dashboard showing:
   - Total legacy images found
   - Total file size
   - Database references
   - Broken references (if any)

## Before You Start

### ?? IMPORTANT: Create a Backup!

Before running the migration, **ALWAYS create a backup** of:
1. Your entire `wwwroot/uploads/` folder
2. Your database

### Check the Statistics

The migration tool shows you:
- **Total Legacy Images**: How many images will be migrated
- **File Size**: How much disk space is being used
- **Database References**: How many blog posts, events, and causes use these images
- **Broken References**: Content that references missing images

## Running the Migration

### Step 1: Analyze

When you first open the tool, it automatically scans:
- `/wwwroot/uploads/blog/` - Blog images
- `/wwwroot/uploads/events/` - Event images
- `/wwwroot/uploads/causes/` - Cause images
- `/wwwroot/Unsplash/` - Unsplash imports

### Step 2: Configure Options

You have two checkboxes:

#### ? Update database references (RECOMMENDED)
- **Checked**: Automatically updates all BlogPosts, Events, and Causes to point to new image locations
- **Unchecked**: Only copies images, doesn't update references (you'll need to update manually)

**Recommendation**: ALWAYS keep this checked unless you have a specific reason not to.

#### ?? Delete old files after migration
- **Checked**: Permanently deletes files from legacy directories after successful migration
- **Unchecked**: Keeps old files (safer option)

**Recommendation**: 
1. **First run**: Leave UNchecked - test that everything works
2. **Second run**: Check this box after verifying everything works correctly

### Step 3: Run Migration

1. Review your settings
2. Click **"Migrate All Images"**
3. Confirm the action
4. Wait for completion (may take a few minutes for many images)

### What Happens During Migration

```
For each image file:
  1. Read file from legacy directory
  2. Calculate file hash (SHA256) for duplicate detection
  3. Check if already in Media Library ? Skip if yes
  4. Check if duplicate exists (by hash) ? Use existing if yes
  5. Copy to /uploads/media/ with new GUID filename
  6. Add entry to MediaLibrary database table
  7. Track old path ? new path mapping

After all images:
  1. Update database references (if checked)
     - BlogPosts.FeaturedImageUrl
     - Events.FeaturedImageUrl
     - Causes.FeaturedImageUrl
  2. Delete old files (if checked and migration successful)
```

## Understanding the Results

After migration, you'll see a summary:

- **Migrated**: Number of images successfully copied and added to Media Library
- **Already in library**: Images that were already migrated (safe to skip)
- **Failed**: Images that couldn't be migrated (check errors)
- **DB references updated**: How many database records were updated
- **Files deleted**: How many old files were removed (if delete was checked)

### Example Results

```
Migration completed!
• Migrated: 47
• Already in library: 3
• Failed: 0
• DB references updated: 50
• Files deleted: 0
```

This means:
- 47 new images were added to Media Library
- 3 images were already there (probably from a previous partial migration)
- No errors occurred
- 50 content items were updated to use new paths
- Old files were kept (delete was unchecked)

## Fixing Broken References

If the tool shows broken references:

### Option 1: Run Migration (EASIEST)
If the files exist in legacy directories, the migration will fix them automatically.

### Option 2: Manual Fix
1. Click **"View Broken References"**
2. You'll see a table showing:
   - Type (BlogPost, Event, or Cause)
   - ID and Title
   - Missing image path
3. Click **"Edit"** for each item
4. Use the Media Picker to select a new image
5. Save

### Option 3: Remove Broken References
If the images are truly lost:
1. Edit each affected content item
2. Remove the broken image reference
3. Either select a new image or leave blank
4. Save

## Removing Duplicate Images

After migration, you can remove duplicates:

1. Click **"Remove Duplicates"**
2. The tool will:
   - Find images with the same file hash
   - Keep the oldest one (first uploaded)
   - Delete duplicates from database and disk
3. You'll see how many duplicates were removed

**When to use**: 
- After migrating if the same image was in multiple legacy directories
- After importing lots of Unsplash images

## Recommended Migration Process

### Phase 1: Test Migration
```
1. Backup everything
2. Run migration with:
   ? Update database references
   ? Delete old files
3. Test your site thoroughly:
   - Visit blog posts
   - Visit events
   - Visit causes
   - Check all images display correctly
4. Check Media Library:
   - Browse images
   - Search for images
   - Verify categorization
```

### Phase 2: Cleanup (After Testing)
```
1. Remove duplicates (if any)
2. Fix any broken references
3. Verify everything still works
```

### Phase 3: Final Deletion (Optional)
```
1. Run migration again with:
   ? Update database references
   ? Delete old files
2. Verify site still works
3. Delete empty legacy directories:
   - /wwwroot/uploads/blog/
   - /wwwroot/uploads/events/
   - /wwwroot/uploads/causes/
   - /wwwroot/Unsplash/
```

## Troubleshooting

### Issue: "No images found"
**Solution**: 
- Check that legacy directories exist
- Verify files have image extensions (.jpg, .png, .gif, .webp, .svg)
- Check file permissions

### Issue: Some images failed to migrate
**Solution**:
- Check the error messages
- Verify file isn't corrupted
- Check disk space
- Ensure write permissions on /uploads/media/

### Issue: Database references not updated
**Solution**:
- Verify "Update database references" was checked
- Check that content exists in database
- Look for error messages
- Try running migration again

### Issue: Broken references after migration
**Solution**:
- Check if files existed in legacy directories
- Verify file paths are correct
- Use "View Broken References" to identify and fix

### Issue: Images appear twice
**Solution**:
- Run "Remove Duplicates"
- This finds images with same content and removes extras

## FAQs

**Q: Will this affect my live site?**
A: During migration, no. Your site continues to work. After migration, if you checked "Delete old files," the old paths won't work anymore, but database references will be updated to new paths.

**Q: Can I run the migration multiple times?**
A: Yes! The tool is idempotent - it skips images already in Media Library. Safe to run again if it fails or you add more legacy images.

**Q: What if I upload new images to legacy directories after migration?**
A: Don't do this! Use the Media Manager for all new uploads. If you must, you can run migration again to catch them.

**Q: Will this migrate images inside blog post content?**
A: No, only featured images. Content images (inline in rich text) aren't migrated automatically. This is a future enhancement.

**Q: How long does migration take?**
A: Depends on image count:
- 50 images: ~1 minute
- 200 images: ~5 minutes
- 1000 images: ~20 minutes

**Q: Can I stop the migration?**
A: Not recommended. Let it complete. If it fails, you can run again and it will skip already-migrated images.

**Q: What happens to image filenames?**
A: Original filenames are stored in the Media Library database (FileName column), but physical files are renamed to GUIDs to prevent naming conflicts.

**Q: Can I reverse the migration?**
A: Not automatically, but if you didn't check "Delete old files," the originals are still there. You'd need to manually revert database changes.

## Success Checklist

After migration, verify:
- [ ] All blog posts display images correctly
- [ ] All events display images correctly
- [ ] All causes display images correctly
- [ ] Media Library shows all migrated images
- [ ] Search in Media Library works
- [ ] Images are properly categorized
- [ ] No broken image links on site
- [ ] "Broken References" shows 0 issues

## Next Steps

After successful migration:
1. ? Train team to use Media Library for all new uploads
2. ? Update documentation to reflect new workflow
3. ? Remove legacy upload code (future task)
4. ? Consider CDN integration (future enhancement)
5. ? Set up automated backups of /uploads/media/

## Support

If you encounter issues:
1. Check error messages in migration results
2. Review this documentation
3. Check server logs for detailed errors
4. Contact development team with:
   - Number of images being migrated
   - Error messages received
   - Screenshot of migration results

---

**Remember**: Always backup before migration! ??
