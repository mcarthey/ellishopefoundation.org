# TinyMCE API Key Setup Guide

## Why Do You Need a TinyMCE API Key?

TinyMCE is a powerful rich text editor used in your Blog and Events admin pages. While TinyMCE offers a free tier with no credit card required, they require an API key to prevent abuse of their CDN.

## Getting Your Free TinyMCE API Key

### Step 1: Sign Up for TinyMCE Cloud

1. Visit https://www.tiny.cloud/auth/signup/
2. Fill in your details:
   - **Email**: Your work email
   - **Name**: Your name
   - **Company**: Ellis Hope Foundation
   - **Role**: Developer
3. Click "Get Started - It's FREE"
4. Verify your email address

### Step 2: Get Your API Key

1. Log in to https://www.tiny.cloud/my-account/
2. Navigate to **Dashboard** or **API Key Management**
3. Your **API Key** will be displayed (looks like: `abc123def456ghi789jkl012mno345pq`)
4. Copy this key

### Step 3: Configure Your Application

#### Option A: Using User Secrets (Recommended for Development)

```powershell
cd EllisHope
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_API_KEY_HERE"
```

Replace `YOUR_API_KEY_HERE` with your actual API key from Step 2.

#### Option B: Environment Variables (Production)

**Windows:**
```powershell
setx TinyMCE__ApiKey "YOUR_API_KEY_HERE"
```

**Linux/Mac:**
```bash
export TinyMCE__ApiKey="YOUR_API_KEY_HERE"
```

**Azure App Service:**
1. Navigate to: Azure Portal ? Your App Service ? Configuration ? Application settings
2. Add new setting:
   - **Name**: `TinyMCE__ApiKey`
   - **Value**: Your API key

### Step 4: Verify Configuration

1. **Start your application**:
   ```powershell
   cd EllisHope
   dotnet run
   ```

2. **Navigate to**: `/Admin/Blog/Create`

3. **Verify**: The TinyMCE editor should load without warnings

## TinyMCE Free Tier Limits

The free tier includes:
- ? **Unlimited domain usage** (localhost, staging, production)
- ? **1,000 editor loads per month** (more than enough for your use case)
- ? **Premium plugins** (image upload, code sample, etc.)
- ? **Cloud delivery** (no need to self-host)
- ? **SSL/HTTPS support**

For the Ellis Hope Foundation website, the free tier is perfect!

## Alternative: Self-Hosted TinyMCE (No API Key Required)

If you prefer not to use an API key, you can self-host TinyMCE:

### Step 1: Download TinyMCE

1. Visit https://www.tiny.cloud/get-tiny/self-hosted/
2. Download the latest version (currently v6)
3. Extract to `EllisHope/wwwroot/lib/tinymce/`

### Step 2: Update Views

Replace the CDN script tag in all views:

**FROM:**
```html
<script src="https://cdn.tiny.cloud/1/@(ViewData["TinyMceApiKey"] ?? "no-api-key")/tinymce/6/tinymce.min.js"></script>
```

**TO:**
```html
<script src="~/lib/tinymce/tinymce.min.js"></script>
```

### Step 3: Update Controllers

Remove the `SetTinyMceApiKey()` calls from:
- `BlogController.cs`
- `EventsController.cs`

### Pros & Cons

**CDN with API Key (Recommended):**
- ? Always up-to-date
- ? Fast global delivery
- ? No maintenance
- ? Requires API key
- ? External dependency

**Self-Hosted:**
- ? No API key needed
- ? Full offline capability
- ? Manual updates required
- ? Larger app size
- ? Slower delivery (no CDN)

## Troubleshooting

### "Domain is not approved" Error

If you see this error:
1. Log in to https://www.tiny.cloud/my-account/
2. Go to **Approved Domains**
3. Add your domain (e.g., `localhost`, `ellishopefoundation.org`)
4. Wait 5-10 minutes for propagation

### API Key Not Working

1. **Verify user secrets are set**:
   ```powershell
   cd EllisHope
   dotnet user-secrets list
   ```

2. **Check for the key**:
   ```
   TinyMCE:ApiKey = abc123def456...
   ```

3. **Restart your application**

4. **Check browser console** (F12) for error messages

### Still Getting "no-api-key" Warning

If the warning persists after setting the key:

1. **Clear browser cache** (Ctrl + Shift + Delete)
2. **Hard refresh** (Ctrl + F5)
3. **Verify ViewData is set** in controller:
   - Add breakpoint in `SetTinyMceApiKey()`
   - Verify `_configuration["TinyMCE:ApiKey"]` has a value

## Security Best Practices

### ? DO
- ? Use User Secrets for development
- ? Use environment variables or Azure Key Vault for production
- ? Keep your API key confidential
- ? Add approved domains in TinyMCE dashboard

### ? DON'T
- ? Commit API key to Git
- ? Share API key publicly
- ? Use the same key across multiple unrelated projects
- ? Hard-code the key in source files

## Quick Reference Commands

```powershell
# Set TinyMCE API key
dotnet user-secrets set "TinyMCE:ApiKey" "YOUR_KEY"

# List all secrets (verify it's set)
dotnet user-secrets list

# Remove TinyMCE API key
dotnet user-secrets remove "TinyMCE:ApiKey"

# Get help
dotnet user-secrets --help
```

## Resources

- [TinyMCE Pricing](https://www.tiny.cloud/pricing/) - Compare plans
- [TinyMCE Documentation](https://www.tiny.cloud/docs/) - Full docs
- [TinyMCE Cloud Dashboard](https://www.tiny.cloud/my-account/) - Manage your account
- [Self-Hosted Downloads](https://www.tiny.cloud/get-tiny/self-hosted/) - Download TinyMCE

## Support

If you encounter issues:
1. Check the [Troubleshooting](#troubleshooting) section above
2. Consult [SECRETS-MANAGEMENT.md](SECRETS-MANAGEMENT.md) for general secrets setup
3. Visit [TinyMCE Support](https://www.tiny.cloud/contact/)

---

**Recommendation**: Use the **free cloud-hosted version with API key** - it's the easiest and most maintainable option for your project.
