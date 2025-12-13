# HTTPS Configuration for Production

## Current Issue: Temporary URL Uses HTTP Only

Your SmarterASP.net temporary URL (`http://mcarthey-001-site1.qtempurl.com/`) only supports HTTP, not HTTPS.

This caused authentication cookies to fail because the app was configured to require HTTPS.

## Changes Made

### 1. Cookie Security Policy
**Before:**
```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? Requires HTTPS
```

**After:**
```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // ? Uses HTTPS if available
```

### 2. SameSite Cookie Policy
**Before:**
```csharp
options.Cookie.SameSite = SameSiteMode.Strict; // ? Too restrictive
```

**After:**
```csharp
options.Cookie.SameSite = SameSiteMode.Lax; // ? Better compatibility
```

### 3. HTTPS Redirection
**Before:**
```csharp
app.UseHttpsRedirection(); // ? Forced HTTPS redirect (infinite loop on HTTP-only sites)
```

**After:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); // ? Only in development
}
```

## When You Get Your Custom Domain with HTTPS

Once you have `https://ellishopefoundation.org` or similar with a valid SSL certificate:

### Option 1: Enable via Configuration (Recommended)

Add to `appsettings.Production.json`:
```json
{
  "Security": {
    "RequireHttps": true
  }
}
```

Then update `Program.cs`:
```csharp
var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps", false);

builder.Services.ConfigureApplicationCookie(options =>
{
    // ... other settings ...
    options.Cookie.SecurePolicy = requireHttps 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;
});

// ...

if (app.Environment.IsDevelopment() || requireHttps)
{
    app.UseHttpsRedirection();
}
```

### Option 2: Direct Code Update

Update `Program.cs`:
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? Safe with HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // ? More secure with HTTPS
});

// ...

// Enable HTTPS redirection
app.UseHttpsRedirection();
```

## Security Implications

### Current Setup (HTTP Only):
- ?? **Medium Security**: Cookies transmitted in plain text
- ? **Mitigation**: `HttpOnly` flag prevents JavaScript access
- ?? **Risk**: Session hijacking possible on unsecured networks
- ? **Acceptable**: For temporary/testing environment

### With HTTPS (Recommended for Production):
- ? **High Security**: Encrypted transmission
- ? **Secure cookies**: Can't be intercepted
- ? **Browser trust**: Valid SSL certificate
- ? **SEO benefit**: Google prefers HTTPS

## Testing After Deployment

1. **Publish updated code**
2. **Test login**: `http://mcarthey-001-site1.qtempurl.com/Admin/Account/Login`
   - Email: `admin@ellishope.org`
   - Password: `Admin@123456`
3. **Verify redirect**: Should go to Dashboard after login
4. **Check authentication**: Browse to other admin pages

## Browser Developer Tools Check

After logging in, check cookies (F12 ? Application ? Cookies):

**Should see:**
- `.AspNetCore.Identity.Application` cookie
- `HttpOnly`: ? true
- `Secure`: ? false (because HTTP)
- `SameSite`: Lax

**When you have HTTPS, it should show:**
- `Secure`: ? true
- `SameSite`: Strict (if you upgrade)

## Next Steps

1. ? Deploy the updated code
2. ? Test login functionality
3. ?? When ready, get custom domain with SSL
4. ?? Re-enable strict HTTPS security
5. ?? Force HTTPS redirects

## SmarterASP.net SSL Information

Check if SmarterASP.net offers:
- Free Let's Encrypt SSL certificates
- Custom domain HTTPS support
- How to enable HTTPS on your account

Once HTTPS is available, update the security settings for maximum protection.
