# Media Migration Tool - Quick Reference

## ?? Purpose
Clean up legacy images from scattered directories and consolidate into centralized Media Library.

## ?? Access
Navigate to: **Admin ? Media Migration**  
URL: `/Admin/MediaMigration`

## ?? What You'll See

### Statistics
| Metric | What It Means |
|--------|---------------|
| Legacy Images | Files in old directories |
| Total Size | Disk space used |
| Database References | Content using these images |
| Broken References | Missing or invalid images |

### Directories Scanned
- `/uploads/blog/` ? Blog images
- `/uploads/events/` ? Event images
- `/uploads/causes/` ? Cause images
- `/Unsplash/` ? Unsplash imports

## ?? Migration Options

| Option | Recommended | What It Does |
|--------|-------------|--------------|
| ? Update database references | ? YES | Auto-updates BlogPosts, Events, Causes |
| ? Delete old files | ?? LATER | Removes files from legacy dirs |

**First Run**: Check update refs, UNCHECK delete files  
**Second Run**: Check both (after testing)

## ?? Quick Start

```
1. ?? BACKUP FIRST!
   - Backup /wwwroot/uploads/
   - Backup database

2. Run Migration
   - Go to /Admin/MediaMigration
   - Review stats
   - ? Update database references
   - ? Delete old files (not yet)
   - Click "Migrate All Images"
   - Wait for completion

3. Test Everything
   - Browse blog posts
   - Browse events
   - Browse causes
   - Check all images load
   - Check Media Library

4. Cleanup (if all good)
   - Re-run with delete option
   - Remove empty directories
```

## ? What Gets Migrated

| From | To | Category |
|------|-----|----------|
| `/uploads/blog/*.jpg` | `/uploads/media/{guid}.jpg` | Blog |
| `/uploads/events/*.jpg` | `/uploads/media/{guid}.jpg` | Event |
| `/uploads/causes/*.jpg` | `/uploads/media/{guid}.jpg` | Cause |
| `/Unsplash/*.jpg` | `/uploads/media/unsplash/{id}.jpg` | Uncategorized |

## ?? Migration Results

After migration, you'll see:
```
Migration completed!
• Migrated: 47 (newly added to library)
• Already in library: 3 (skipped, already there)
• Failed: 0 (errors)
• DB references updated: 50 (content items fixed)
• Files deleted: 0 (if delete option checked)
```

## ?? Features

### Duplicate Detection
- Calculates SHA256 hash of each file
- Skips files with same hash
- Uses existing image instead
- Saves disk space

### Smart Skipping
- Already in MediaLibrary? ? SKIP
- Duplicate hash? ? SKIP (reuse existing)
- New image? ? MIGRATE

### Database Updates
- BlogPosts.FeaturedImageUrl ? new path
- Events.FeaturedImageUrl ? new path
- Causes.FeaturedImageUrl ? new path

## ??? Additional Tools

### Remove Duplicates
```
Click: "Remove Duplicates"
? Finds images with same hash
? Keeps oldest
? Deletes extras
? Saves disk space
```

### View Broken References
```
Click: "View Broken References"
? Shows table of content with missing images
? Provides edit links
? Helps fix issues
```

## ?? Safety Features

? Idempotent - safe to run multiple times  
? Skips already-migrated images  
? Detailed error reporting  
? Optional file deletion  
? Transaction-safe database updates  

## ?? Troubleshooting

| Problem | Fix |
|---------|-----|
| No images found | Check directories exist |
| Some failed | Check error messages, file permissions |
| Broken refs after | Use "View Broken References" tool |
| Duplicates remain | Run "Remove Duplicates" |
| Old files still there | Rerun with delete option |

## ?? Checklist

Before Migration:
- [ ] Backup /wwwroot/uploads/
- [ ] Backup database
- [ ] Note current image counts
- [ ] Test on staging first (if available)

After Migration:
- [ ] Check migration results
- [ ] Test blog posts
- [ ] Test events
- [ ] Test causes
- [ ] Check Media Library
- [ ] View broken references (should be 0)
- [ ] Remove duplicates
- [ ] Re-test everything

Final Cleanup:
- [ ] Re-run with delete option
- [ ] Delete empty legacy directories
- [ ] Update team procedures
- [ ] Monitor for new legacy uploads

## ?? Pro Tips

1. **Always backup first** - Seriously!
2. **Test before deleting** - Run twice (test, then delete)
3. **Check broken refs** - Fix before final cleanup
4. **Remove duplicates** - Frees disk space
5. **Train your team** - Use Media Manager going forward

## ?? Remember

- **ONE migration** consolidates everything
- **TWO options**: update refs (yes), delete files (later)
- **THREE steps**: backup ? migrate ? test
- **ZERO fear**: it's safe and reversible

## ?? Need Help?

1. Check error messages in results
2. Review `docs/user-guides/media-migration-guide.md`
3. Check server logs
4. Contact dev team with:
   - Migration results screenshot
   - Error messages
   - Number of images involved

---

**Access**: /Admin/MediaMigration  
**Status**: ? Ready  
**Safety**: High (if you backup!)
