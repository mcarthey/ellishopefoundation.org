# ?? EMAIL QUICK REFERENCE CARD

## ?? **Production Configuration**

```json
{
  "EmailSettings": {
    "SmtpHost": "mail.ellishopefoundation.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@ellishopefoundation.org",
    "SmtpPassword": "[YOUR_PASSWORD]",
    "EnableSsl": true,
    "FromEmail": "noreply@ellishopefoundation.org",
    "FromName": "Ellis Hope Foundation"
  }
}
```

---

## ?? **Quick Setup Checklist**

- [ ] Create `noreply@ellishopefoundation.org` (300MB)
- [ ] Generate strong password
- [ ] Test login at: https://mail5011.site4now.net
- [ ] Update `appsettings.json` password
- [ ] Enable catch-all ? postmaster@
- [ ] Test email sending
- [ ] Forward postmaster@ to your personal email

---

## ?? **Server Details**

| Setting | Value |
|---------|-------|
| **SMTP Host** | `mail.ellishopefoundation.org` |
| **SMTP Port** | `587` (TLS) or `465` (SSL) |
| **Webmail** | https://mail5011.site4now.net |
| **MX Record** | `igw11.site4now.net` |

---

## ?? **Quick Test**

```csharp
// Add to any controller temporarily
await _emailService.SendEmailAsync(
    "your-email@gmail.com",
    "Test Email",
    "<h1>It works!</h1>"
);
```

---

## ?? **Troubleshooting**

**Auth Failed?**
- Use full email: `postmaster@ellishopefoundation.org` (not just `postmaster`)

**Connection Timeout?**
- Try port `465` instead of `587`

**Emails in Spam?**
- Ask hosting about SPF/DKIM setup

---

## ? **Recommended Accounts**

```
noreply@ellishopefoundation.org  (300MB) - System emails
info@ellishopefoundation.org     (300MB) - User responses
admin@ellishopefoundation.org    (200MB) - Internal
postmaster@ellishopefoundation.org (200MB) - Catch-all
```

**Total: 1000MB** ?

