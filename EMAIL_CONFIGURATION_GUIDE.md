# ?? Ellis Hope Foundation - Email Configuration Guide

## ?? **Your Hosting Provider Details**

```
Hosting Provider: Site4Now.net
Domain:          ellishopefoundation.org
Storage:         1000 MB
Secure Webmail:  https://mail5011.site4now.net
```

---

## ? **Complete Email Configuration**

### **1. Email Account Setup**

#### **Already Created:**
- ? `postmaster@ellishopefoundation.org`

#### **Recommended Additional Accounts:**

| Email Address | Purpose | Storage | Priority |
|---------------|---------|---------|----------|
| `noreply@ellishopefoundation.org` | System notifications | 300MB | HIGH ? |
| `info@ellishopefoundation.org` | General inquiries | 300MB | MEDIUM |
| `admin@ellishopefoundation.org` | Internal admin | 200MB | MEDIUM |
| `board@ellishopefoundation.org` | Board communications | 200MB | LOW |

**Total: 1000MB perfectly distributed** ?

---

## ?? **Application Configuration**

### **appsettings.json (Updated!):**

```json
{
  "EmailSettings": {
    "SmtpHost": "mail.ellishopefoundation.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@ellishopefoundation.org",
    "SmtpPassword": "YOUR_PASSWORD_HERE",
    "EnableSsl": true,
    "FromEmail": "noreply@ellishopefoundation.org",
    "FromName": "Ellis Hope Foundation"
  }
}
```

### **Production Configuration (`appsettings.Production.json`):**

Create this file for production deployment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_DATABASE_CONNECTION_STRING"
  },
  "AppSettings": {
    "BaseUrl": "https://ellishopefoundation.org"
  },
  "EmailSettings": {
    "SmtpHost": "mail.ellishopefoundation.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@ellishopefoundation.org",
    "SmtpPassword": "YOUR_SECURE_PASSWORD",
    "EnableSsl": true,
    "FromEmail": "noreply@ellishopefoundation.org",
    "FromName": "Ellis Hope Foundation"
  }
}
```

---

## ?? **Step-by-Step Setup**

### **Step 1: Create Additional Email Accounts (10 min)**

**In your hosting control panel:**

1. **Create `noreply@ellishopefoundation.org`**
   ```
   Account: noreply@ellishopefoundation.org
   Password: [Generate strong password]
   Quota: 300MB
   ```

2. **Create `info@ellishopefoundation.org`** (Optional, but recommended)
   ```
   Account: info@ellishopefoundation.org
   Password: [Generate strong password]
   Quota: 300MB
   ```

### **Step 2: Configure Catch-All (Recommended!)** ?

**What is Catch-All?**
- Catches all emails sent to undefined addresses
- Example: `support@ellishopefoundation.org` ? forwards to main account
- Great for typos: `noreply@ellishopefoundation.org` typo ? still works!

**Enable Catch-All:**
```
1. Login to hosting control panel
2. Email Settings ? Catch-All
3. Enable: YES ?
4. Forward to: postmaster@ellishopefoundation.org
```

**Benefits:**
- ? Never miss emails due to typos
- ? Create virtual addresses without actual accounts
- ? Professional appearance

### **Step 3: Configure Domain Alias (Optional)**

**What is Domain Alias?**
- Use `@ellis-hope.org` and `@ellishopefoundation.org` interchangeably
- Only useful if you own multiple domains

**Recommendation:** Skip unless you own other domains ?

### **Step 4: Update Application Settings**

**Development (`appsettings.json`):**
```json
"EmailSettings": {
  "SmtpHost": "mail.ellishopefoundation.org",
  "SmtpPort": 587,
  "SmtpUsername": "postmaster@ellishopefoundation.org",
  "SmtpPassword": "",  // Empty for dev (emails won't send)
  "EnableSsl": true,
  "FromEmail": "noreply@ellishopefoundation.org",
  "FromName": "Ellis Hope Foundation"
}
```

**Production (Environment Variable or Secret Manager):**
```bash
# Don't commit password to git!
# Use environment variable or Azure Key Vault

export EmailSettings__SmtpPassword="YOUR_SECURE_PASSWORD"
```

---

## ?? **Security Best Practices**

### **1. Never Commit Passwords to Git**

Create `appsettings.Development.json` (gitignored):
```json
{
  "EmailSettings": {
    "SmtpPassword": "development-testing-password"
  }
}
```

Add to `.gitignore`:
```
appsettings.Development.json
appsettings.Production.json
```

### **2. Use Environment Variables in Production**

In your hosting environment:
```
EmailSettings__SmtpHost = mail.ellishopefoundation.org
EmailSettings__SmtpPort = 587
EmailSettings__SmtpUsername = postmaster@ellishopefoundation.org
EmailSettings__SmtpPassword = [SECURE_PASSWORD]
EmailSettings__EnableSsl = true
```

### **3. Generate Strong Passwords**

For each email account:
```
Minimum: 16 characters
Include: Uppercase, lowercase, numbers, symbols
Example: Kp9$mN2@vL5#xR8^qT3!
```

---

## ?? **Testing Email Configuration**

### **Option 1: Quick Test in Controller**

Add to any controller temporarily:

```csharp
[HttpGet("test-email")]
public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
{
    try
    {
        await emailService.SendEmailAsync(
            "your-personal-email@gmail.com",  // ? Your email
            "Test from Ellis Hope Foundation",
            "<h1>Success!</h1><p>Email configuration is working correctly!</p>"
        );
        
        return Ok("Email sent! Check your inbox.");
    }
    catch (Exception ex)
    {
        return BadRequest($"Email failed: {ex.Message}");
    }
}
```

### **Option 2: Submit Test Application**

1. Run the application
2. Create test account
3. Submit application
4. Check your email!

---

## ?? **Email Account Structure**

### **Recommended Setup:**

```
postmaster@ellishopefoundation.org (300MB)
??? Purpose: Catch-all destination
??? Access: Admin monitoring
??? Contains: Bounces, errors, catch-all emails

noreply@ellishopefoundation.org (300MB) ? SMTP SENDER
??? Purpose: Application notifications
??? Access: System only (automated)
??? Contains: All outgoing automated emails

info@ellishopefoundation.org (300MB)
??? Purpose: General inquiries
??? Access: Staff monitoring
??? Contains: User responses, questions

admin@ellishopefoundation.org (100MB)
??? Purpose: Internal admin communications
??? Access: Admin team
??? Contains: Staff communications
```

---

## ?? **SMTP Server Options**

Your host provides TWO options:

### **Option 1: Standard (Recommended)** ?

```json
{
  "SmtpHost": "mail.ellishopefoundation.org",
  "SmtpPort": 587,
  "EnableSsl": true
}
```

**Pros:**
- ? Clean configuration
- ? Uses your domain
- ? Professional appearance
- ? Standard port (587)

### **Option 2: Direct Host**

```json
{
  "SmtpHost": "mail5011.site4now.net",
  "SmtpPort": 587,
  "EnableSsl": true
}
```

**When to use:**
- Only if Option 1 fails
- DNS propagation issues
- Troubleshooting

---

## ?? **Port Options**

| Port | Type | Use When | Security |
|------|------|----------|----------|
| **587** | STARTTLS | **Default** ? | Encrypted |
| 465 | SSL/TLS | Port 587 blocked | Encrypted |
| 8889 | Alternative | ISP blocks 25/587 | Check with host |
| 25 | Plain | **Avoid** ? | Not encrypted |

**Recommendation: Use Port 587** ?

---

## ?? **Troubleshooting**

### **Issue 1: Authentication Failed**

```
Error: "535 Authentication failed"

Solutions:
1. Verify username includes full email address:
   ? postmaster@ellishopefoundation.org
   ? postmaster

2. Check password (no spaces, special chars)

3. Verify account is active in hosting panel

4. Try webmail login to confirm credentials:
   https://mail5011.site4now.net
```

### **Issue 2: Connection Timeout**

```
Error: "Connection timeout"

Solutions:
1. Check firewall allows port 587 outbound

2. Try alternative port 465:
   "SmtpPort": 465

3. Verify server is accessible:
   telnet mail.ellishopefoundation.org 587

4. Contact hosting support if persistent
```

### **Issue 3: Emails Go to Spam**

```
Solutions:
1. Verify SPF record (ask hosting provider)

2. Enable DKIM (ask hosting provider)

3. Use "noreply@" but include reply-to header

4. Avoid spam trigger words:
   ? "FREE", "WINNER", "CLICK HERE"
   ? "Application Submitted", "Review Status"

5. Warm up the email account:
   - Send to yourself first
   - Gradually increase volume
   - Don't send 100 emails on day 1
```

### **Issue 4: Port 587 Blocked**

```
Some ISPs block SMTP ports. Try:

1. Port 465 (SSL):
   "SmtpPort": 465

2. Alternative port 8889:
   "SmtpPort": 8889

3. Contact your ISP for whitelist
```

---

## ? **Post-Setup Checklist**

### **Email Accounts:**
- [ ] Create `noreply@ellishopefoundation.org`
- [ ] Set strong password (16+ chars)
- [ ] Test webmail login: https://mail5011.site4now.net
- [ ] Optional: Create `info@`, `admin@` accounts

### **Configuration:**
- [ ] Update `appsettings.json` with SMTP settings
- [ ] **DON'T** commit password to git
- [ ] Create `appsettings.Production.json` for deployment
- [ ] Add sensitive files to `.gitignore`

### **Features:**
- [ ] Enable Catch-All (forward to postmaster)
- [ ] Skip Domain Alias (unless needed)
- [ ] Set up email forwarding to personal email (monitoring)

### **Testing:**
- [ ] Test email sending with test endpoint
- [ ] Submit test application
- [ ] Verify email received
- [ ] Check spam folder
- [ ] Verify "From" address shows correctly

### **DNS (Usually Auto-configured):**
- [ ] Verify MX record: `igw11.site4now.net`
- [ ] Check SPF record (ask hosting support if needed)
- [ ] Optional: Enable DKIM (ask hosting support)

---

## ?? **Recommended Configuration**

### **For Ellis Hope Foundation:**

```json
{
  "EmailSettings": {
    "SmtpHost": "mail.ellishopefoundation.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@ellishopefoundation.org",
    "SmtpPassword": "USE_ENVIRONMENT_VARIABLE",
    "EnableSsl": true,
    "FromEmail": "noreply@ellishopefoundation.org",
    "FromName": "Ellis Hope Foundation"
  }
}
```

### **Email Strategy:**

```
Outgoing (Automated):
??? From: noreply@ellishopefoundation.org
    Reply-To: info@ellishopefoundation.org

Catch-All:
??? Forward all undefined addresses to: postmaster@

Manual Responses:
??? Use: info@ellishopefoundation.org
```

---

## ?? **Email Templates Will Use:**

```
Application Submitted:
  From: noreply@ellishopefoundation.org
  Reply-To: info@ellishopefoundation.org
  Subject: Application Submitted Successfully

Board Notification:
  From: noreply@ellishopefoundation.org
  Reply-To: board@ellishopefoundation.org
  Subject: New Application Received

Approval Notification:
  From: noreply@ellishopefoundation.org
  Reply-To: info@ellishopefoundation.org
  Subject: Congratulations! Application Approved
```

---

## ?? **Final Setup Summary**

### **What You Have:**
? Professional domain email (`@ellishopefoundation.org`)  
? 1000MB storage (plenty for 10+ years)  
? SSL/TLS encryption  
? Webmail access  
? SMTP/IMAP/POP3 support  

### **What to Do:**
1. Create `noreply@` email account (10 min)
2. Update application configuration (5 min)
3. Enable catch-all forwarding (2 min)
4. Test email sending (5 min)

### **Total Setup Time: ~25 minutes** ??

---

## ?? **Ready to Deploy!**

Your email system is:
- ? Professional
- ? Secure
- ? Scalable
- ? Cost-effective (included in hosting)

**Next Steps:**
1. Create the email accounts
2. Update the password in `appsettings.json`
3. Test it!
4. ?? Start sending notifications!

