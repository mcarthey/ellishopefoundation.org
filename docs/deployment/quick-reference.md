# ?? Quick Reference: HTTPS Deployment

## Current Status

? **Domain:** ellishopefoundation.org + www.ellishopefoundation.org  
? **SSL Certificate:** Requested from Let's Encrypt via SmarterASP.net  
?? **Application:** Updated and ready to deploy with HTTPS  

---

## What You Just Did

? Selected the right SSL option: `ellishopefoundation.org + www.ellishopefoundation.org`

This covers both:
- https://ellishopefoundation.org
- https://www.ellishopefoundation.org

---

## What Happens Next

### Phase 1: Wait for SSL (5-15 minutes)

SmarterASP.net is automatically:
1. Validating your domain ownership
2. Requesting certificate from Let's Encrypt
3. Installing the certificate
4. Activating HTTPS

**You don't need to do anything** - just wait for confirmation email or check the SmarterASP control panel.

### Phase 2: Test HTTPS (When ready)

Once SSL is active, test these URLs:
```
https://ellishopefoundation.org  ? Should show green padlock ??
https://www.ellishopefoundation.org  ? Should show green padlock ??
```

### Phase 3: Deploy Updated Code

**After confirming HTTPS works:**

1. **Publish the application**
   ```
   Right-click EllisHope ? Publish ? mcarthey-001-site1 - Web Deploy
   ```

2. **What this does:**
   - Enables automatic HTTP ? HTTPS redirect
   - Activates security headers (HSTS, CSP, etc.)
   - Makes cookies secure

3. **Test the redirect:**
   ```
   Visit: http://ellishopefoundation.org (no 's')
   Should redirect to: https://ellishopefoundation.org
   ```

### Phase 4: Verify Security

1. **SSL Labs Test**
   ```
   https://www.ssllabs.com/ssltest/
   Enter: ellishopefoundation.org
   Target: Grade A or A+
   ```

2. **Test login**
   ```
   https://ellishopefoundation.org/Admin/Account/Login
   Login: admin@ellishope.org / Admin@123456
   Then: CHANGE PASSWORD IMMEDIATELY!
   ```

---

## Files Ready for Deployment

### Already Updated:
- ? `web.config` - HTTPS redirect enabled
- ? `Program.cs` - Cookie security configured
- ? `appsettings.Production.json` - Database & API keys set

### What Gets Deployed:
When you publish, these HTTPS features activate:
- Automatic HTTP ? HTTPS redirect
- Secure cookies (`Secure` flag)
- HSTS (forces HTTPS for 1 year)
- Content Security Policy
- XSS protection headers

---

## Important URLs

### Production Site (Current)
```
HTTP: http://mcarthey-001-site1.qtempurl.com/
HTTPS: Not yet available
```

### Production Site (After SSL)
```
HTTP: http://ellishopefoundation.org (redirects to HTTPS)
HTTPS: https://ellishopefoundation.org ?
WWW: https://www.ellishopefoundation.org ?
Admin: https://ellishopefoundation.org/Admin/Account/Login
```

### Testing Tools
```
SSL Test: https://www.ssllabs.com/ssltest/
Security Headers: https://securityheaders.com/
Mobile Friendly: https://search.google.com/test/mobile-friendly
Page Speed: https://developers.google.com/speed/pagespeed/insights/
```

---

## Checklists Available

Use these to track your progress:

1. **`POST-SSL-CHECKLIST.md`** ? Use this first!
   - Step-by-step after SSL is active
   - Testing procedures
   - Security validation
   - Browser compatibility checks

2. **`HTTPS-SETUP-CHECKLIST.md`**
   - Overall project tracking
   - All 8 phases
   - Timeline and milestones

3. **`HTTPS-SETUP-GUIDE.md`**
   - Detailed instructions
   - Troubleshooting guide
   - Common issues and solutions

4. **`HTTPS-CONFIGURATION.md`**
   - Technical configuration details
   - Security implications
   - Future upgrades

---

## Default Credentials (CHANGE THESE!)

### Admin Account
```
Email: admin@ellishope.org
Password: Admin@123456
?? CHANGE THIS PASSWORD IMMEDIATELY AFTER FIRST LOGIN!
```

### Database
```
Server: SQL1002.site4now.net
Database: db_ab82c4_ellishopedb
Username: db_ab82c4_ellishopedb_admin
Password: EHFDbAdmin.123
?? Consider changing to stronger password
```

---

## Expected Timeline

| Event | Time | Status |
|-------|------|--------|
| SSL requested | Now | ? Done |
| SSL issued | 5-15 min | ? Waiting |
| HTTPS available | +5 min | ? Pending |
| Deploy updated code | After test | ? Pending |
| HTTP redirect active | After deploy | ? Pending |
| Change admin password | After deploy | ? Pending |
| **LAUNCH!** | ~30 min total | ? Pending |

---

## Quick Commands

### Check SSL Status (Browser)
```
Visit: https://ellishopefoundation.org
Look for: Green padlock ??
Click padlock: View certificate details
```

### Publish Application
```
Visual Studio:
1. Right-click EllisHope
2. Publish
3. mcarthey-001-site1 - Web Deploy
4. Publish button
```

### Test Everything
```
1. Visit: https://ellishopefoundation.org
2. Check: Green padlock present
3. Login: /Admin/Account/Login
4. Create: Test blog post
5. Verify: Images upload
6. Test: http://ellishopefoundation.org redirects
```

---

## Support Resources

### If SSL Isn't Working
1. Check SmarterASP.net control panel for status
2. Wait full 15 minutes before troubleshooting
3. Check email for validation requests
4. Contact SmarterASP.net support if > 30 minutes

### If Redirect Isn't Working
1. Verify web.config was deployed
2. Check IIS URL Rewrite module is enabled
3. Clear browser cache
4. Test in incognito/private window

### If Login Doesn't Work
1. Check cookies in browser dev tools (F12)
2. Verify `Secure` flag is set
3. Clear all site cookies and try again
4. Check console for JavaScript errors

---

## Success Criteria

You'll know everything is working when:

? `https://ellishopefoundation.org` shows green padlock  
? `http://ellishopefoundation.org` redirects to HTTPS  
? Admin login works  
? Can create and publish content  
? SSL Labs gives grade A or better  
? No browser console errors  
? Cookies show `Secure: true`  

---

## Next Action Items

### Right Now:
- ? Wait for SSL certificate to be issued
- ? Review `POST-SSL-CHECKLIST.md`
- ? Prepare to test HTTPS access

### After SSL is Active:
- ? Test HTTPS URLs
- ? Publish updated application
- ? Test HTTP redirect
- ? Run SSL Labs test
- ? Change admin password
- ? Update social media links

### This Week:
- ? Complete all security testing
- ? Test all site features
- ? Update external references
- ? Set up monitoring

---

## Emergency Contacts

**SmarterASP.net Support:**
- Control Panel: https://www.SmarterASP.net/cp
- Support: Available in control panel
- Documentation: Check their knowledge base

**If Site Goes Down:**
1. Check SmarterASP.net status page
2. Check error logs in control panel
3. Test database connection
4. Contact support if needed

---

## Status Dashboard

Mark each as complete:

- [ ] SSL certificate issued
- [ ] HTTPS tested and working
- [ ] Updated code deployed
- [ ] HTTP redirect working
- [ ] Security headers active
- [ ] SSL Labs grade A+
- [ ] Admin password changed
- [ ] All features tested
- [ ] External links updated
- [ ] **SITE IS LIVE!** ??

---

**Last Updated:** ____________________  
**Next Review:** ____________________  

?? **You're almost there! Just waiting on the SSL certificate now.**
