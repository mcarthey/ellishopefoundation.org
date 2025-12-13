# HTTPS Setup Checklist

Use this checklist to track your progress setting up HTTPS for your production site.

## Phase 1: Domain Setup

- [ ] **Choose Domain Name**
  - Recommended: `ellishopefoundation.org`
  - Alternative: `ellishope.org` or other
  - Decision: ____________________

- [ ] **Purchase Domain**
  - Registrar: ____________________
  - Date Purchased: ____________________
  - Renewal Date: ____________________
  - Cost: $____________________

- [ ] **Add Domain to SmarterASP.net**
  - Control Panel ? Domains ? Add Domain
  - Domain added: ____________________
  - Date added: ____________________

- [ ] **Get DNS Information from SmarterASP.net**
  - A Record IP: ____________________
  - CNAME Value: ____________________
  - Nameservers (if using): ____________________

- [ ] **Update DNS at Domain Registrar**
  - A Record configured: Yes / No
  - CNAME Record configured: Yes / No
  - Date updated: ____________________

- [ ] **Wait for DNS Propagation**
  - Started: ____________________
  - Test with: `nslookup ellishopefoundation.org`
  - Completed: ____________________

## Phase 2: SSL Certificate Installation

- [ ] **Request SSL Certificate**
  - Method: Free (Let's Encrypt) / Paid
  - Requested date: ____________________
  - Provider: SmarterASP.net / Other: ____________________

- [ ] **Verify Domain Ownership**
  - Method: Email / DNS / HTTP
  - Completed: ____________________

- [ ] **Install SSL Certificate**
  - Certificate issued: ____________________
  - Certificate installed: ____________________
  - Expiration date: ____________________

- [ ] **Test HTTPS Access**
  - URL tested: https://____________________
  - Padlock shows: Yes / No
  - Certificate valid: Yes / No

## Phase 3: Enable HTTPS Redirect

- [ ] **Enable HTTPS Redirect**
  - Method: IIS (web.config) / SmarterASP Control Panel
  - Enabled date: ____________________

- [ ] **Update web.config**
  - Uncomment HTTPS redirect section
  - Deploy updated web.config
  - Date deployed: ____________________

- [ ] **Test HTTP to HTTPS Redirect**
  - Visit: http://ellishopefoundation.org
  - Redirects to HTTPS: Yes / No
  - Working correctly: Yes / No

## Phase 4: Application Updates (Optional)

- [ ] **Enable Strict HTTPS in Code**
  - Update `appsettings.Production.json`:
    ```json
    "Security": {
      "RequireHttps": true,
      "UseHsts": true
    }
    ```
  - Re-publish application
  - Date deployed: ____________________

- [ ] **Enable Security Headers**
  - Uncomment HSTS in web.config
  - Uncomment CSP in web.config
  - Deploy updated web.config
  - Date deployed: ____________________

## Phase 5: Testing & Validation

- [ ] **SSL Labs Test**
  - URL: https://www.ssllabs.com/ssltest/
  - Grade: ____________________
  - Date tested: ____________________

- [ ] **Test All Site Features**
  - [ ] Home page loads over HTTPS
  - [ ] Admin login works
  - [ ] Dashboard loads
  - [ ] Create blog post
  - [ ] Upload images
  - [ ] Create event
  - [ ] All static assets load (no mixed content)

- [ ] **Test on Multiple Browsers**
  - [ ] Chrome
  - [ ] Firefox
  - [ ] Edge
  - [ ] Safari (if available)
  - [ ] Mobile browser

- [ ] **Check Browser Console**
  - No security errors: Yes / No
  - No mixed content warnings: Yes / No
  - Cookies are secure: Yes / No

## Phase 6: SEO & Marketing Updates

- [ ] **Update Google Search Console**
  - Add HTTPS property
  - Submit sitemap
  - Date completed: ____________________

- [ ] **Update Social Media Links**
  - [ ] Facebook
  - [ ] Twitter
  - [ ] Instagram
  - [ ] LinkedIn
  - [ ] Other: ____________________

- [ ] **Update Email Signatures**
  - Change to: https://ellishopefoundation.org
  - Date updated: ____________________

- [ ] **Update Business Cards / Printed Materials**
  - Note for next print run: ____________________

## Phase 7: Security Hardening

- [ ] **Change Default Admin Password**
  - Old: `Admin@123456`
  - New: (stored securely in password manager)
  - Date changed: ____________________

- [ ] **Create Real Admin Account**
  - Email: ____________________
  - Password: (in password manager)
  - Date created: ____________________

- [ ] **Delete Default Admin (Optional)**
  - Confirm new admin works first!
  - Date deleted: ____________________

- [ ] **Set Up Database Backups**
  - Backup method: ____________________
  - Frequency: ____________________
  - Tested restore: Yes / No

- [ ] **Document All Credentials**
  - Database password: (in password manager)
  - Admin password: (in password manager)
  - FTP password: (in password manager)
  - Domain registrar: (in password manager)
  - SmarterASP.net: (in password manager)

## Phase 8: Monitoring & Maintenance

- [ ] **Set Calendar Reminders**
  - [ ] Domain renewal (annual)
  - [ ] SSL certificate renewal (if not auto-renew)
  - [ ] Database backup verification (monthly)
  - [ ] Security updates (monthly)

- [ ] **Monitor Site Uptime**
  - Tool: ____________________
  - Set up alerts: Yes / No

- [ ] **Set Up Error Logging**
  - Method: ____________________
  - Review frequency: ____________________

## Troubleshooting Notes

Use this space to document any issues and their solutions:

**Issue 1:**
- Problem: ____________________
- Solution: ____________________
- Date resolved: ____________________

**Issue 2:**
- Problem: ____________________
- Solution: ____________________
- Date resolved: ____________________

**Issue 3:**
- Problem: ____________________
- Solution: ____________________
- Date resolved: ____________________

---

## Quick Reference

**Production Site:** http://mcarthey-001-site1.qtempurl.com/ (temporary)  
**Future Site:** https://ellishopefoundation.org (after domain setup)  
**Admin Login:** /Admin/Account/Login  
**Database Server:** SQL1002.site4now.net  
**Database Name:** db_ab82c4_ellishopedb  

**Support Contacts:**
- SmarterASP.net Support: ____________________
- Domain Registrar Support: ____________________
- SSL Provider Support: ____________________

---

## Completion Status

- Phase 1 (Domain Setup): ? Not Started / ? In Progress / ? Complete
- Phase 2 (SSL Installation): ? Not Started / ? In Progress / ? Complete
- Phase 3 (HTTPS Redirect): ? Not Started / ? In Progress / ? Complete
- Phase 4 (App Updates): ? Not Started / ? In Progress / ? Complete
- Phase 5 (Testing): ? Not Started / ? In Progress / ? Complete
- Phase 6 (SEO/Marketing): ? Not Started / ? In Progress / ? Complete
- Phase 7 (Security): ? Not Started / ? In Progress / ? Complete
- Phase 8 (Monitoring): ? Not Started / ? In Progress / ? Complete

**Overall Project Status:** ____________________  
**Target Completion Date:** ____________________  
**Actual Completion Date:** ____________________  

---

## Notes

Add any additional notes, observations, or reminders here:
