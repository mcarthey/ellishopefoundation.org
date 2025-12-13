# Post-SSL Installation Checklist

## SSL Certificate Status

? **Certificate Requested:** ellishopefoundation.org + www.ellishopefoundation.org  
? **Waiting for:** Let's Encrypt to issue certificate (5-15 minutes)  
?? **Started:** ____________________  
? **Completed:** ____________________  

---

## After SSL Certificate is Active

### Immediate Testing (Do This First!)

1. **Test HTTPS Access**
   ```
   Visit: https://ellishopefoundation.org
   Expected: Green padlock ?? shows in browser
   Status: ? Working / ? Not Working
   ```

2. **Test WWW Subdomain**
   ```
   Visit: https://www.ellishopefoundation.org
   Expected: Green padlock ?? shows in browser
   Status: ? Working / ? Not Working
   ```

3. **Check Certificate Details**
   ```
   Click padlock ? View Certificate
   Expected: 
   - Issued to: ellishopefoundation.org
   - Issued by: Let's Encrypt
   - Valid from: ____________________
   - Valid until: ____________________ (should be ~90 days from now)
   Status: ? Verified
   ```

4. **Test Login Over HTTPS**
   ```
   Visit: https://ellishopefoundation.org/Admin/Account/Login
   Login with:
   - Email: admin@ellishope.org
   - Password: Admin@123456
   Expected: Successfully redirects to Dashboard
   Status: ? Working / ? Not Working
   ```

5. **Check Cookies Are Secure**
   ```
   F12 ? Application ? Cookies ? https://ellishopefoundation.org
   Find: .AspNetCore.Identity.Application
   Expected:
   - Secure: ? true
   - HttpOnly: ? true
   - SameSite: Lax
   Status: ? Verified
   ```

---

### Deploy Updated web.config (HTTPS Redirect Enabled)

**?? IMPORTANT: Only do this AFTER confirming HTTPS works above!**

1. **Current Status**
   - ? `web.config` has been updated locally with HTTPS redirect enabled
   - ? Security headers enabled (HSTS, CSP, etc.)
   - ? Ready to deploy

2. **Publish Updated Application**
   ```
   Visual Studio:
   1. Right-click EllisHope project
   2. Select "Publish"
   3. Choose "mcarthey-001-site1 - Web Deploy"
   4. Click "Publish"
   
   Date deployed: ____________________
   Status: ? Complete
   ```

3. **Test HTTP to HTTPS Redirect**
   ```
   Visit: http://ellishopefoundation.org (no 's')
   Expected: Automatically redirects to https://ellishopefoundation.org
   Status: ? Working / ? Not Working
   ```

4. **Test WWW Redirect**
   ```
   Visit: http://www.ellishopefoundation.org (no 's')
   Expected: Automatically redirects to https://www.ellishopefoundation.org
   Status: ? Working / ? Not Working
   ```

---

### Security Validation

1. **SSL Labs Test**
   ```
   Visit: https://www.ssllabs.com/ssltest/
   Enter: ellishopefoundation.org
   Click: Submit
   
   Expected Grade: A or A+
   Actual Grade: ____________________
   Date tested: ____________________
   Status: ? Complete
   ```

2. **Check Security Headers**
   ```
   Visit: https://securityheaders.com/
   Enter: https://ellishopefoundation.org
   
   Expected Headers Present:
   - ? Strict-Transport-Security (HSTS)
   - ? X-Frame-Options
   - ? X-Content-Type-Options
   - ? X-XSS-Protection
   - ? Content-Security-Policy
   
   Grade: ____________________
   Date tested: ____________________
   Status: ? Complete
   ```

3. **Test Mixed Content**
   ```
   Visit: https://ellishopefoundation.org
   Open: F12 ? Console
   Expected: No mixed content warnings
   Status: ? No warnings / ? Has warnings
   
   If warnings found, note them here:
   ____________________
   ```

---

### Functional Testing

Test all major features over HTTPS:

1. **Home Page**
   - [ ] Loads completely
   - [ ] All images load
   - [ ] All CSS loads
   - [ ] All JavaScript works
   - [ ] No console errors

2. **Admin Portal**
   - [ ] Login page loads
   - [ ] Can log in successfully
   - [ ] Dashboard loads
   - [ ] Can create blog post
   - [ ] Can upload images
   - [ ] Can create event
   - [ ] TinyMCE editor works
   - [ ] Can save and publish content

3. **Public Pages**
   - [ ] Blog listing loads
   - [ ] Individual blog posts load
   - [ ] Event listing loads
   - [ ] Individual events load
   - [ ] About page loads
   - [ ] Contact page loads (if exists)

4. **Media/Images**
   - [ ] All existing images display
   - [ ] Can upload new images
   - [ ] Image thumbnails generate
   - [ ] Unsplash integration works

---

### Browser Compatibility

Test on multiple browsers to ensure HTTPS works everywhere:

- [ ] **Chrome** (Desktop)
  - Version: ____________________
  - Status: ? Working
  
- [ ] **Firefox** (Desktop)
  - Version: ____________________
  - Status: ? Working
  
- [ ] **Edge** (Desktop)
  - Version: ____________________
  - Status: ? Working
  
- [ ] **Safari** (Desktop - if available)
  - Version: ____________________
  - Status: ? Working
  
- [ ] **Chrome** (Mobile)
  - Device: ____________________
  - Status: ? Working
  
- [ ] **Safari** (iOS - if available)
  - Device: ____________________
  - Status: ? Working

---

### Update External References

Once HTTPS is confirmed working, update all external references:

1. **Social Media Profiles**
   - [ ] Facebook page ? Update website URL
   - [ ] Twitter/X bio ? Update website URL
   - [ ] Instagram bio ? Update website URL
   - [ ] LinkedIn company page ? Update website URL
   - [ ] Other: ____________________ ? Update

2. **Google Services**
   - [ ] Google Search Console ? Add HTTPS property
   - [ ] Google Analytics ? Update default URL (if using)
   - [ ] Google My Business ? Update website URL (if applicable)

3. **Email Signatures**
   - [ ] Update email signature to use https://
   - [ ] Notify team members to update theirs

4. **Printed Materials**
   - [ ] Note for next business card print run
   - [ ] Note for next brochure print run
   - [ ] Update any digital PDFs/downloads

---

### Security Hardening

1. **Change Default Admin Password**
   ```
   Login as: admin@ellishope.org
   Current password: Admin@123456
   
   ?? CHANGE TO STRONG PASSWORD IMMEDIATELY!
   
   New password: (Save in password manager)
   Date changed: ____________________
   Status: ? Complete
   ```

2. **Create Production Admin Account** (Optional)
   ```
   Consider creating a new admin with your actual email
   
   New Admin Details:
   - Email: ____________________
   - Password: (Save in password manager)
   - Date created: ____________________
   Status: ? Complete
   ```

3. **Delete Default Admin** (Optional - Only after new admin is verified!)
   ```
   ?? Only do this after confirming new admin account works!
   
   Deleted: admin@ellishope.org
   Date deleted: ____________________
   Status: ? Complete
   ```

---

### Monitoring Setup

1. **Set Up Uptime Monitoring** (Optional but recommended)
   ```
   Services:
   - UptimeRobot (free)
   - Pingdom
   - StatusCake
   
   Service chosen: ____________________
   Configured: ? Yes / ? No
   Alert email: ____________________
   ```

2. **SSL Certificate Auto-Renewal**
   ```
   Let's Encrypt certificates expire after 90 days
   
   ? SmarterASP.net should auto-renew
   
   Set calendar reminder to check: ____________________
   (Recommend: 75 days from now)
   ```

3. **Domain Renewal Reminder**
   ```
   Domain purchased: ____________________
   Domain expires: ____________________
   
   Set calendar reminder: ____________________
   (Recommend: 30 days before expiration)
   ```

---

### Performance Testing

1. **Page Load Speed**
   ```
   Visit: https://developers.google.com/speed/pagespeed/insights/
   Enter: https://ellishopefoundation.org
   
   Mobile Score: ____________________
   Desktop Score: ____________________
   Date tested: ____________________
   ```

2. **Mobile Responsiveness**
   ```
   Visit: https://search.google.com/test/mobile-friendly
   Enter: https://ellishopefoundation.org
   
   Result: ? Mobile-friendly / ? Not mobile-friendly
   Date tested: ____________________
   ```

---

### Documentation

1. **Update README** (if needed)
   ```
   Update production URL in repository documentation
   Status: ? Complete
   ```

2. **Document Production Credentials**
   ```
   Store securely in password manager:
   - ? Production database password
   - ? Admin account password
   - ? FTP password
   - ? Domain registrar login
   - ? SmarterASP.net login
   - ? Any API keys
   
   Password manager used: ____________________
   Status: ? Complete
   ```

---

## Troubleshooting Log

If you encounter any issues, document them here:

### Issue 1:
- **Problem:** ____________________
- **When:** ____________________
- **Error Message:** ____________________
- **Solution:** ____________________
- **Resolved:** ? Yes / ? No
- **Date Resolved:** ____________________

### Issue 2:
- **Problem:** ____________________
- **When:** ____________________
- **Error Message:** ____________________
- **Solution:** ____________________
- **Resolved:** ? Yes / ? No
- **Date Resolved:** ____________________

### Issue 3:
- **Problem:** ____________________
- **When:** ____________________
- **Error Message:** ____________________
- **Solution:** ____________________
- **Resolved:** ? Yes / ? No
- **Date Resolved:** ____________________

---

## Final Sign-Off

- [ ] All tests passed
- [ ] HTTPS redirect working
- [ ] Security headers enabled
- [ ] SSL Labs grade: A or better
- [ ] Admin password changed
- [ ] All major features tested
- [ ] External references updated
- [ ] Monitoring set up
- [ ] Documentation updated

**Production Launch Date:** ____________________  
**Launched By:** ____________________  
**Status:** ? LIVE ?

---

## Notes

Additional observations or future improvements:

____________________
____________________
____________________
____________________
