# Setting Up HTTPS on SmarterASP.net

## Overview

Your application is currently configured to work with both HTTP and HTTPS. Once you enable HTTPS on SmarterASP.net, it will automatically start using secure cookies.

## Step-by-Step HTTPS Setup

### Option 1: Using SmarterASP.net's Free SSL (Recommended)

SmarterASP.net typically provides free SSL certificates through Let's Encrypt for custom domains.

#### 1. **Get a Custom Domain**

You need a custom domain (you can't use HTTPS with the temporary `qtempurl.com` domain).

**Domain Options:**
- `ellishopefoundation.org` (recommended)
- `ellishope.org`
- Any other domain you prefer

**Where to Buy:**
- Namecheap (~$10-15/year for .org)
- Google Domains
- GoDaddy
- Or use your existing domain

#### 2. **Add Domain to SmarterASP.net**

1. Log into SmarterASP.net control panel
2. Navigate to **"Domains"** or **"Domain Management"**
3. Click **"Add Domain"**
4. Enter your domain: `ellishopefoundation.org`

#### 3. **Update DNS Records**

At your domain registrar (Namecheap, GoDaddy, etc.), update DNS to point to SmarterASP.net:

**A Record:**
```
Type: A
Host: @
Value: [Your SmarterASP.net IP - they'll provide this]
TTL: Automatic or 3600
```

**CNAME Record (for www):**
```
Type: CNAME
Host: www
Value: [Your site hostname - e.g., win1135.site4now.net]
TTL: Automatic or 3600
```

**SmarterASP.net will provide these values** in their control panel under domain settings.

#### 4. **Enable SSL Certificate**

In SmarterASP.net control panel:

1. Go to **"SSL/TLS"** or **"Security"** section
2. Select your domain: `ellishopefoundation.org`
3. Click **"Install Free SSL Certificate"** (Let's Encrypt)
4. Or **"Install SSL Certificate"** if you have a paid certificate
5. Wait 5-15 minutes for certificate to be issued and installed

#### 5. **Force HTTPS** (After SSL is Active)

Once SSL is working:

1. In SmarterASP.net control panel:
   - Look for **"Force HTTPS"** or **"Redirect to HTTPS"** option
   - Enable it

2. Or add to your `web.config` (if it doesn't exist, create it in your project root):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Redirect to HTTPS" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="^OFF$" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

### Option 2: Using a Paid SSL Certificate

If you want a paid/extended validation certificate:

#### 1. **Purchase SSL Certificate**

**Providers:**
- Namecheap SSL (~$9/year)
- DigiCert
- Sectigo (formerly Comodo)
- Let's Encrypt (FREE)

#### 2. **Generate CSR (Certificate Signing Request)**

In SmarterASP.net control panel:
1. Go to **SSL/TLS** section
2. Click **"Generate CSR"**
3. Fill in your organization details:
   - Common Name: `ellishopefoundation.org`
   - Organization: Ellis Hope Foundation
   - City: [Your city]
   - State: [Your state]
   - Country: US
4. Submit and save the CSR

#### 3. **Submit CSR to SSL Provider**

1. Copy the CSR from SmarterASP.net
2. Paste it into your SSL provider's order form
3. Complete domain validation (email or DNS)

#### 4. **Install Certificate**

1. Download certificate files from SSL provider
2. In SmarterASP.net: **SSL/TLS** ? **Install SSL Certificate**
3. Upload certificate files
4. Enable HTTPS redirect

---

## Updating Your Application for HTTPS

### Current Code Already Supports HTTPS!

Your app is configured to automatically detect and use HTTPS:

```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
```

This means:
- ? Works with HTTP (current temporary URL)
- ? Automatically uses HTTPS when available
- ? No code changes needed when you enable SSL

### Optional: Enforce Strict HTTPS (After SSL is Active)

Once HTTPS is working, you can optionally make it more strict:

#### Update `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Security": {
    "RequireHttps": true,
    "UseHsts": true
  }
}
```

#### Update `Program.cs`:

```csharp
var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps", false);

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    
    // Use strict HTTPS when configured
    options.Cookie.SecurePolicy = requireHttps 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;
    
    options.Cookie.SameSite = requireHttps 
        ? SameSiteMode.Strict 
        : SameSiteMode.Lax;
});

// ...later in the file...

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    // Enable HSTS if configured
    if (requireHttps && builder.Configuration.GetValue<bool>("Security:UseHsts", true))
    {
        app.UseHsts();
    }
}

// Enable HTTPS redirection when required
if (app.Environment.IsDevelopment() || requireHttps)
{
    app.UseHttpsRedirection();
}
```

---

## Testing HTTPS Setup

### 1. **Test SSL Certificate**

After enabling SSL, test at:
- https://www.ssllabs.com/ssltest/
- Enter your domain: `ellishopefoundation.org`
- Should get an **A** or **A+** rating

### 2. **Test Your Site**

1. Visit: `https://ellishopefoundation.org`
2. Check for green padlock ?? in browser
3. Test login: `https://ellishopefoundation.org/Admin/Account/Login`
4. Verify cookies are secure (F12 ? Application ? Cookies)

**Cookies should show:**
- `Secure`: ? true
- `HttpOnly`: ? true
- `SameSite`: Lax or Strict

### 3. **Test HTTP to HTTPS Redirect**

1. Visit: `http://ellishopefoundation.org` (no 's')
2. Should automatically redirect to: `https://ellishopefoundation.org`

---

## Common Issues and Solutions

### Issue 1: "Your connection is not private" Error

**Cause**: SSL certificate not installed or not trusted

**Solution**:
1. Wait 15 minutes after requesting certificate
2. Clear browser cache
3. Verify certificate is installed in SmarterASP.net control panel

### Issue 2: Mixed Content Warnings

**Cause**: Your site loads resources (images, CSS, JS) over HTTP

**Solution**: Update all URLs to use HTTPS or relative URLs:

```html
<!-- Bad -->
<img src="http://example.com/image.jpg">

<!-- Good -->
<img src="https://example.com/image.jpg">
<img src="/assets/img/image.jpg">
```

### Issue 3: Infinite Redirect Loop

**Cause**: Both IIS and application are forcing HTTPS redirect

**Solution**: Choose one:
- Use IIS/web.config redirect (recommended)
- OR use `app.UseHttpsRedirection()` in code
- Not both!

### Issue 4: Cookies Still Not Working

**Cause**: Check browser developer tools

**Solution**:
```
F12 ? Console ? Look for errors
F12 ? Application ? Cookies ? Check .AspNetCore.Identity.Application cookie
```

---

## Recommended Timeline

### Week 1: Get Domain & Basic Setup
1. ? Purchase domain (`ellishopefoundation.org`)
2. ? Add domain to SmarterASP.net
3. ? Update DNS records
4. ? Wait for DNS propagation (24-48 hours)

### Week 2: Enable SSL
1. ? Request free SSL certificate (Let's Encrypt)
2. ? Install and activate SSL
3. ? Test HTTPS access
4. ? Enable HTTP to HTTPS redirect

### Week 3: Lock Down Security
1. ? Update `appsettings.Production.json` with `RequireHttps: true`
2. ? Re-publish application
3. ? Test all functionality
4. ? Run SSL Labs test

---

## Cost Breakdown

| Item | Cost | Notes |
|------|------|-------|
| Domain (.org) | $10-15/year | One-time purchase, annual renewal |
| SSL Certificate | **FREE** | Let's Encrypt via SmarterASP.net |
| **Total** | **$10-15/year** | Just the domain! |

**Or with paid SSL:**
| Domain + SSL Bundle | $20-30/year | Namecheap, GoDaddy bundles |

---

## What to Do RIGHT NOW

### Immediate Action Items:

1. **Register Domain** (if you haven't already)
   - Go to Namecheap.com or your preferred registrar
   - Search for: `ellishopefoundation.org`
   - Purchase for 1-2 years

2. **Contact SmarterASP.net Support**
   - Ask: "How do I set up a free SSL certificate for my custom domain?"
   - Ask: "What DNS records do I need to configure?"
   - They'll provide specific instructions for your account

3. **Keep Current Setup**
   - Your app works fine on HTTP for now
   - Once HTTPS is enabled, it will automatically switch to secure mode
   - No code changes required!

---

## After HTTPS is Working

Once you have HTTPS active on your custom domain:

1. ? Test the site thoroughly
2. ? Update all links/bookmarks to use `https://`
3. ? Update any marketing materials with new domain
4. ? Submit site to Google Search Console
5. ? Set up Google Analytics (if desired)

---

## Need Help?

If you encounter any issues during setup, let me know:
- What step you're on
- Any error messages you see
- Screenshots from SmarterASP.net control panel

I can help troubleshoot specific issues as they come up!

---

## Summary

? Your code is **already HTTPS-ready**  
? Just need to enable SSL on SmarterASP.net  
? Costs only $10-15/year for domain  
? Free SSL certificate via Let's Encrypt  
? No additional code changes needed  

**Next Step**: Purchase your domain and contact SmarterASP.net support for SSL setup instructions specific to your account.
