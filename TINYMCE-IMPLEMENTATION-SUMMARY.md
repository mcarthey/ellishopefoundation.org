# ? TinyMCE API Key Implementation Summary

## Problem

When creating or editing blog posts and events, you were seeing a warning popup:
> "A valid API key is required to continue using TinyMCE."

This was because the views were using TinyMCE's CDN with `no-api-key` placeholder.

## Solution Implemented

### ? **Recommended Approach: Free TinyMCE Cloud with API Key**

You now have a complete integration that:
1. ? Uses TinyMCE's free cloud-hosted version
2. ? Stores API key securely using User Secrets
3. ? Passes key to views via ViewData
4. ? Falls back to "no-api-key" if not configured (with warning)

## Files Modified

| File | Changes |
|------|---------|
| `BlogController.cs` | Added `IConfiguration` injection and `SetTinyMceApiKey()` method |
| `EventsController.cs` | Added `IConfiguration` injection and `SetTinyMceApiKey()` method |
| `Blog/Create.cshtml` | Updated TinyMCE CDN URL to use `@ViewData["TinyMceApiKey"]` |
| `Blog/Edit.cshtml` | Updated TinyMCE CDN URL to use `@ViewData["TinyMceApiKey"]` |
| `Events/Create.cshtml` | Updated TinyMCE CDN URL to use `@ViewData["TinyMceApiKey"]` |
| `Events/Edit.cshtml` | Updated TinyMCE CDN URL to use `@ViewData["TinyMceApiKey"]` |
| `appsettings.json` | Added `TinyMCE:ApiKey` configuration section |
| `SECRETS-MANAGEMENT.md` | Added TinyMCE to secrets documentation |
| `README.md` | Added TinyMCE setup instructions |
| `SECRETS-QUICK-REFERENCE.md` | Added TinyMCE to quick reference |
| `TINYMCE-SETUP.md` | **NEW** - Complete TinyMCE setup guide |

## Next Steps for You

### 1. Get Your Free TinyMCE API Key (5 minutes)

1. **Sign up**: https://www.tiny.cloud/auth/signup/
   - Email: your work email
   - Company: Ellis Hope Foundation
   - Click "Get Started - It's FREE"

2. **Get API Key**: 
   - Log in to https://www.tiny.cloud/my-account/
   - Copy your API key (format: `abc123def456...`)

### 2. Configure Your Application (1 minute)

```powershell
cd EllisHope
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_ACTUAL_API_KEY_HERE"
```

Replace `YOUR_ACTUAL_API_KEY_HERE` with the key from step 1.

### 3. Test the Fix (1 minute)

```powershell
dotnet run
```

Then navigate to:
- http://localhost:5000/Admin/Blog/Create
- Verify TinyMCE loads **without** the warning popup

## What You Get

### TinyMCE Free Tier Benefits

- ? **1,000 editor loads/month** (plenty for your site)
- ? **Premium plugins** included (image upload, tables, etc.)
- ? **Cloud delivery** via CDN (fast, always up-to-date)
- ? **Unlimited domains** (localhost, staging, production)
- ? **No credit card required**

### Features Now Available

All these TinyMCE features will work without warnings:
- ? Rich text formatting (bold, italic, etc.)
- ? Image upload and embedding
- ? Tables and lists
- ? Code blocks with syntax highlighting
- ? Link management
- ? Emoticons and special characters
- ? Responsive editing

## Alternative Option: Self-Hosted TinyMCE

If you prefer not to use an API key (not recommended):

1. Download TinyMCE from https://www.tiny.cloud/get-tiny/self-hosted/
2. Extract to `wwwroot/lib/tinymce/`
3. Update script tags in views to use local version
4. See [TINYMCE-SETUP.md](TINYMCE-SETUP.md) for detailed instructions

**Why not recommended?**
- ? Requires manual updates
- ? Larger application size
- ? No CDN benefits
- ? More maintenance overhead

## Verification Checklist

After setting up your API key:

- [ ] API key obtained from TinyMCE Cloud
- [ ] User secret configured: `dotnet user-secrets list` shows `TinyMCE:ApiKey`
- [ ] Application restarted
- [ ] Blog Create page loads without warning
- [ ] Blog Edit page loads without warning
- [ ] Event Create page loads without warning
- [ ] Event Edit page loads without warning
- [ ] TinyMCE editor is fully functional

## Troubleshooting

### Still Seeing the Warning?

1. **Verify secret is set**:
   ```powershell
   cd EllisHope
   dotnet user-secrets list | Select-String "TinyMCE"
   ```
   
   Should show: `TinyMCE:ApiKey = your-key-here`

2. **Clear browser cache**: Ctrl + Shift + Delete

3. **Hard refresh**: Ctrl + F5

4. **Check browser console** (F12) for errors

### "Domain not approved" Error?

1. Log in to https://www.tiny.cloud/my-account/
2. Go to **Approved Domains**
3. Add `localhost` (for development)
4. Wait 5-10 minutes

### API Key Not Loading in Views?

Add a breakpoint in `BlogController.Create()` at the `SetTinyMceApiKey()` line:
- Verify `_configuration["TinyMCE:ApiKey"]` has a value
- Verify `ViewData["TinyMceApiKey"]` is set

## Documentation Resources

- **Complete Setup Guide**: [TINYMCE-SETUP.md](TINYMCE-SETUP.md)
- **Secrets Management**: [SECRETS-MANAGEMENT.md](SECRETS-MANAGEMENT.md)
- **Quick Reference**: [SECRETS-QUICK-REFERENCE.md](SECRETS-QUICK-REFERENCE.md)
- **TinyMCE Docs**: https://www.tiny.cloud/docs/

## Benefits Summary

### Before
- ? Warning popup on every page load
- ? Unprofessional user experience
- ? Uncertainty about TinyMCE functionality

### After
- ? Clean, professional editor experience
- ? No warnings or popups
- ? Free tier with generous limits
- ? Properly configured secrets management
- ? Comprehensive documentation for team

## Security Notes

Your TinyMCE API key is:
- ? Stored in User Secrets (development)
- ? Not committed to Git
- ? Environment-specific (dev/staging/prod can have different keys)
- ? Documented for team setup

---

**Estimated Time to Fix**: 7 minutes total
- Sign up & get key: 5 minutes
- Configure secrets: 1 minute
- Test: 1 minute

**Cost**: $0 (free tier)

**Benefit**: Professional rich text editing experience for your team! ??
