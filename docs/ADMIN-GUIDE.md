# Ellis Hope Foundation - Administrator Guide

**Version:** 1.0  
**Last Updated:** December 15, 2024  
**For:** Site Administrators & Board Members

---

## ?? Table of Contents

1. [Getting Started](#getting-started)
2. [User Management](#user-management)
3. [Profile Management](#profile-management)
4. [Content Management](#content-management)
5. [Media Management](#media-management)
6. [Blog Management](#blog-management)
7. [Event Management](#event-management)
8. [Cause Management](#cause-management)
9. [Security & Access](#security-access)
10. [Troubleshooting](#troubleshooting)

---

## ?? Getting Started

### Accessing the Admin Panel

1. **Navigate to:** `https://ellishopefoundation.org/Admin/Account/Login`
2. **Default Admin Credentials:**
   - Email: `admin@ellishope.org`
   - Password: `Admin@123456`
   - ?? **IMPORTANT:** Change this password immediately after first login!

### Admin Dashboard

After login, you'll see the admin dashboard with:
- **Statistics:** Quick overview of site activity
- **Navigation:** Left sidebar with all admin sections
- **Account Menu:** Top-right corner for profile/logout

### User Roles

| Role | Permissions | Access Level |
|------|-------------|--------------|
| **Admin** | Full access to everything | Complete control |
| **Board Member** | Manage content, users, events | High |
| **Editor** | Manage content only | Medium |
| **Sponsor** | View sponsor portal | Limited |
| **Client** | View client portal | Limited |
| **Member** | Basic access | Minimal |

---

## ?? User Management

### Creating a New User

1. **Navigate to:** Admin ? Users ? Create New User
2. **Fill in required fields:**
   - First Name, Last Name
   - Email (used for login)
   - Password (must meet requirements)
   - Phone Number
   - Role (select appropriate role)
   - Status (typically "Active")
3. **Optional fields:**
   - Date of Birth
   - Address, City, State, Zip
   - Emergency Contact
   - Monthly Fee (for sponsors/clients)
   - Sponsor assignment
4. **Click:** Save User
5. **Result:** Welcome email sent automatically

### Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### Viewing User Details

1. Go to **Users** list
2. Click **eye icon** (???) next to any user
3. View complete user information:
   - Basic info, contact details
   - Account timeline
   - Sponsored clients (if sponsor)
   - Activity history

### Editing Users

1. Click **pencil icon** (??) next to user
2. Update any field (except account creation date)
3. **Admin-Only Fields:**
   - Role assignment
   - Status (Active/Pending/Inactive)
   - Monthly fee
   - Sponsor assignment
   - Account deletion

### User Filters

Use filters to find users:
- **Search:** Name, email, or phone
- **Role:** Filter by role type
- **Status:** Active, Pending, Inactive
- **Active Filter:** Active/Inactive only

---

## ?? Profile Management

### Your Own Profile

**Access via:** Account icon (top-right) ? My Profile

**What You Can Edit:**
- ? Name, email, phone
- ? Address and contact info
- ? Emergency contact
- ? Password

**What Only Admins Can Edit:**
- ? Role
- ? Status
- ? Membership fees
- ? Account deletion

### Changing Your Password

1. **Account Menu** ? Change Password
2. Enter current password
3. Enter new password (must meet requirements)
4. Confirm new password
5. Click **Change Password**
6. You'll be automatically logged back in

---

## ?? Content Management

### Page Manager

**Purpose:** Manage static pages (About, Services, Contact, etc.)

#### Viewing Pages
1. Navigate to **Pages** in admin menu
2. See list of all pages with:
   - Page name and title
   - Published status
   - Last modified date

#### Editing Page Content
1. Click **Edit** on any page
2. **Two Types of Content:**

   **A. Template-Based Pages** (About, Services, Contact)
   - Content managed via code templates
   - Can only update meta description
   - For major changes, contact developer

   **B. Managed Content Pages** (FAQ, etc.)
   - Full visual editor
   - Add/edit/delete sections
   - Upload images
   - Reorder content

#### Adding New Sections
1. Click **Add Section**
2. Choose section type:
   - Rich Text (formatted content)
   - Plain Text
   - HTML (advanced)
3. Enter content
4. Set display order
5. Click **Save**

### Publishing/Unpublishing Pages
1. Edit the page
2. Toggle **Is Published** checkbox
3. Save changes
- **Published:** Visible to public
- **Unpublished:** Hidden from public

---

## ?? Media Management

### Media Library

**Purpose:** Centralized image storage and management

#### Uploading Images

1. Navigate to **Media** ? Upload
2. **Two Options:**

   **A. Upload Local File**
   - Click **Choose File**
   - Select image (max 10MB)
   - Fill in details:
     - Title, Alt Text (for accessibility)
     - Caption, Tags
     - Category (Page, Blog, Event, Team, etc.)
   - Click **Upload**

   **B. Import from Unsplash**
   - Search for professional images
   - Click **Use This Photo**
   - Image imported with attribution
   - Edit details as needed

#### Managing Images

**Viewing:**
- Grid view shows thumbnails
- List view shows details
- Filter by category, tags, or date

**Editing:**
1. Click image thumbnail
2. Update:
   - Title, Alt Text, Caption
   - Tags, Category
   - Generate new thumbnails
3. Click **Save**

**Deleting:**
1. Click **Delete** button
2. System checks if image is in use
3. Confirm deletion (if not in use)

#### Image Categories

- **Page:** Static page images
- **Blog:** Blog post images
- **Event:** Event photos
- **Team:** Staff/volunteer photos
- **Hero:** Banner/hero images
- **Gallery:** Photo galleries
- **Cause:** Cause/program images

---

## ?? Blog Management

### Creating a Blog Post

1. **Navigate to:** Admin ? Blog ? Create New Post
2. **Fill in:**
   - Title (auto-generates URL slug)
   - Summary/Excerpt
   - Full content (rich text editor)
   - Featured image (from Media Library)
   - Categories (select 1 or more)
   - Tags (comma-separated)
   - Meta description (SEO)
3. **Set Status:**
   - Save as Draft
   - OR Publish immediately
4. **Click:** Save Post

### Publishing Workflow

1. **Draft:** Work in progress, not public
2. **Published:** Live on site
3. **Set Published Date:** Schedule for future

### Blog Categories

Default categories:
- Children
- Education
- Healthcare
- Community
- Fundraising
- Volunteer

Add more in **Blog ? Categories**

### SEO Best Practices

- **Title:** 50-60 characters
- **Meta Description:** 150-160 characters
- **Alt Text:** Describe images for accessibility
- **Tags:** Use relevant, searchable terms

---

## ?? Event Management

### Creating an Event

1. **Navigate to:** Admin ? Events ? Create
2. **Required Fields:**
   - Event Title
   - Event Date
   - Location
   - Description
3. **Optional Fields:**
   - Start/End Time
   - Featured image
   - Organizer info (name, email, phone)
   - Registration URL
   - Max attendees
   - Tags
4. **Publish or Save as Draft**

### Event Visibility

- **Published:** Shows on Events page
- **Unpublished:** Hidden from public
- **Past Events:** Automatically archived

### Managing RSVPs (if enabled)

View registrations in Event details

---

## ?? Cause Management

### Creating a Cause/Program

1. **Navigate to:** Admin ? Causes ? Create
2. **Fill in:**
   - Title
   - Short Description (for cards)
   - Full Description
   - Featured Image
   - Category
   - Goal Amount
   - Current Amount Raised
3. **Advanced Options:**
   - Start/End Dates
   - Donation URL
   - Featured flag (shows on homepage)
   - Tags
4. **Click:** Save Cause

### Tracking Donations

Update "Raised Amount" field as donations come in

### Featured Causes

- Toggle "Is Featured" to show on homepage
- Limit to 3-4 featured causes for best display

---

## ?? Security & Access

### Role-Based Access

**Admin Role:**
- Full system access
- User management
- All content management
- System settings

**Board Member Role:**
- Content management
- User viewing (limited editing)
- Reports and analytics

**Editor Role:**
- Content management only
- Cannot manage users
- Cannot change system settings

### Password Security

**For Users:**
- Require strong passwords
- Encourage password changes every 90 days
- Never share passwords

**For Admins:**
- Use unique passwords
- Enable two-factor authentication (if available)
- Regularly review user access

### User Account Status

| Status | Meaning | Login Access |
|--------|---------|--------------|
| Active | Full access | ? Yes |
| Pending | Awaiting approval | ? No |
| Inactive | Temporarily disabled | ? No |
| Expired | Membership ended | ? No |
| Cancelled | Account closed | ? No |

---

## ?? Troubleshooting

### Common Issues

#### "I forgot my password"
1. Go to login page
2. Click "Forgot Password"
3. Enter email address
4. Check email for reset link
5. Create new password

#### "User can't login"
**Check:**
1. Account status is "Active"
2. Email is correct
3. Password meets requirements
4. Account is not locked out (after 5 failed attempts)

**Solution:** Admin can reset password in User Edit page

#### "Image won't upload"
**Common causes:**
- File too large (max 10MB)
- Invalid file type (must be JPG, PNG, GIF, WebP)
- File name has special characters

**Solution:** Resize image, use supported format

#### "Content not showing on public site"
**Check:**
1. Page/Post is Published
2. Published date is not in future
3. Clear browser cache
4. Check if content is set to specific roles only

#### "Email not sending"
**Check:**
1. SMTP settings configured
2. Email address is valid
3. Check spam folder
4. Contact IT support

### Getting Help

**Technical Issues:**
- Contact: support@ellishope.org
- Include: Screenshots, error messages, what you were doing

**Content Questions:**
- Contact: Communications Director
- Review this guide first

**User Access Issues:**
- Contact: System Administrator
- Provide: User's name and email

---

## ?? Support Contacts

**System Administrator:**
- Primary contact for technical issues
- User access and permissions

**IT Support:**
- Email: support@ellishope.org
- For urgent issues: [Phone Number]

**Training:**
- Schedule training sessions
- Request one-on-one help

---

## ?? Quick Reference

### Common Tasks Checklist

**Weekly:**
- [ ] Check new user registrations
- [ ] Review and approve pending content
- [ ] Respond to contact form submissions

**Monthly:**
- [ ] Update event listings
- [ ] Publish blog posts
- [ ] Review user accounts
- [ ] Update cause fundraising progress

**Quarterly:**
- [ ] Archive old events
- [ ] Clean up unused media
- [ ] Review and update pages
- [ ] User access audit

---

## ?? Training Resources

### Video Tutorials
- User Management (Coming Soon)
- Content Creation (Coming Soon)
- Media Library (Coming Soon)

### Live Training
- Contact admin for training session
- Group training available quarterly

---

**Questions or feedback on this guide?**  
Contact: support@ellishope.org

---

**Document Version History:**
- v1.0 (Dec 2024): Initial comprehensive guide

