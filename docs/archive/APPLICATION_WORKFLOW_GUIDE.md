# ?? Application Submission Workflow Guide

## ?? **Correct Workflow for Members to Submit Applications**

### **Step-by-Step Process:**

```
1. User Login
   ??? Navigate to: /Admin/Account/Login
   ??? Enter email/password
   ??? Click "Sign In"

2. After Login (Header Dropdown)
   ??? Click profile icon (top right)
   ??? Dropdown shows:
       ??? Member Portal (for Members) ?
       ??? Client Portal (for Clients)
       ??? Sponsor Portal (for Sponsors)
       ??? etc...

3. Click "Member Portal"
   ??? Redirects to: /Admin/Member/Dashboard
   ??? Which redirects to: /MyApplications ?

4. My Applications Page
   ??? Shows: List of your applications (if any)
   ??? Button: "New Application" or "Create Application"
   ??? Click the button

5. Application Form (6 Steps)
   ??? Step 1: Personal Information
   ??? Step 2: Funding Request
   ??? Step 3: Motivation
   ??? Step 4: Health & Fitness
   ??? Step 5: Program Requirements
   ??? Step 6: Review & Sign
   ??? Submit!

6. Confirmation
   ??? Email sent to applicant ?
   ??? Redirected to application details
   ??? Status: "Submitted - Awaiting Review"
```

---

## ?? **Authorization Requirements**

### **Who Can Access What:**

| URL | Required Role | Purpose |
|-----|---------------|---------|
| `/MyApplications` | **Any logged-in user** ? | View/create applications |
| `/Admin/Member/Dashboard` | **Member role** | Member portal (redirects to MyApplications) |
| `/Admin/Client/Dashboard` | **Client role** | Client portal |
| `/Admin/Sponsor/Dashboard` | **Sponsor role** | Sponsor portal |
| `/Admin/Applications` | **Admin/BoardMember** | Admin application review |

---

## ? **Troubleshooting "Not an Admin" Error**

### **Possible Causes:**

#### **1. User Not Logged In**
```
Error: "Not an admin" or redirects to login
Solution: Make sure you're logged in first
```

#### **2. Wrong Role Assigned**
```
Error: "Forbidden" or "Not authorized"
Check: User's role in database
Expected: UserRole = Member (or Client)
```

#### **3. Trying to Access Admin Pages**
```
Error: "Not an admin"
Wrong URL: /Admin/Dashboard (requires Admin/BoardMember/Editor)
Correct URL: /Admin/Member/Dashboard (redirects to /MyApplications)
```

#### **4. Session Expired**
```
Error: Redirects to login
Solution: Login again
```

---

## ? **Verify Your Setup**

### **Check User Role:**

```sql
-- In database, check user's role
SELECT 
    Email,
    FirstName,
    LastName,
    UserRole,
    Status,
    IsActive
FROM AspNetUsers
WHERE Email LIKE '%your-email%'
```

**Expected Result:**
```
Email: your-email@gmail.com (or +member version)
UserRole: 0 (Member) or 1 (Client)
Status: 1 (Active)
IsActive: 1 (True)
```

---

## ?? **Direct URLs (Shortcuts)**

### **For Testing:**

```
1. Login Page:
   https://localhost:7001/Admin/Account/Login

2. After Login - Direct to Applications:
   https://localhost:7001/MyApplications

3. New Application Form:
   https://localhost:7001/MyApplications/Create

4. Member Dashboard (redirects to MyApplications):
   https://localhost:7001/Admin/Member/Dashboard
```

---

## ?? **Email Testing with Gmail Plus**

### **Create Test Member Accounts:**

```
Test Member 1:
Email: your-email+member1@gmail.com
Role: Member
Password: TestPassword123!

Test Member 2:
Email: your-email+member2@gmail.com
Role: Member
Password: TestPassword123!

Test Client 1:
Email: your-email+client1@gmail.com
Role: Client
Password: TestPassword123!

Test Client 2:
Email: your-email+client2@gmail.com
Role: Client
Password: TestPassword123!
```

All emails arrive at: `your-email@gmail.com` ?

---

## ?? **Alternative Workflows**

### **Option 1: Direct Link (Simplest)**

```
1. Login
2. Navigate directly to: /MyApplications
3. Click "New Application"
4. Fill form
5. Submit
```

### **Option 2: Via Member Portal**

```
1. Login
2. Click "Member Portal" in header
3. Auto-redirected to /MyApplications
4. Click "New Application"
5. Fill form
6. Submit
```

### **Option 3: Via Profile**

```
1. Login
2. Click "My Profile"
3. Navigate to /MyApplications (add link if needed)
4. Click "New Application"
5. Fill form
6. Submit
```

---

## ?? **Expected Behavior**

### **Member Role:**

```
Login ? Member Dashboard ? MyApplications
??? Can: Create applications
??? Can: View own applications
??? Can: Edit drafts
??? Cannot: See other users' applications
??? Cannot: Vote or approve
```

### **Client Role:**

```
Login ? Client Dashboard
??? Can: View client portal
??? Can: Create applications
??? Can: See sponsor info
??? Can: Track progress
```

### **Admin/BoardMember:**

```
Login ? Admin Dashboard
??? Can: See ALL applications
??? Can: Review and vote
??? Can: Approve/reject
??? Can: Manage users
```

---

## ?? **Debug Steps**

### **If You Get "Not an Admin" Error:**

```
1. Check Current URL
   ??? If /Admin/Dashboard ? Correct! (needs Admin role)
   ??? If /Admin/Member/Dashboard ? Should redirect

2. Check Browser Console
   ??? F12 ? Console tab
   ??? Look for errors

3. Check Network Tab
   ??? F12 ? Network tab
   ??? Click "Member Portal"
   ??? See redirect chain

4. Verify Login
   ??? Check header shows your name
   ??? Check dropdown shows "Member" badge
   ??? Try logout/login again

5. Check User Role in Admin
   ??? Login as Admin
   ??? Go to Users list
   ??? Find your test user
   ??? Verify Role = Member
```

---

## ? **Quick Test**

### **Test the Full Workflow:**

```bash
1. Create Member Account
   ??? Admin ? Users ? Create
   ??? Email: your-email+testmember@gmail.com
   ??? Role: Member
   ??? Status: Active
   ??? Password: TestPassword123!

2. Logout

3. Login as Member
   ??? Email: your-email+testmember@gmail.com
   ??? Password: TestPassword123!

4. After Login
   ??? Click profile icon (top right)
   ??? Should see "Member Portal" option
   ??? Click it

5. Should See
   ??? URL: /MyApplications
   ??? Page: "My Applications"
   ??? Button: "New Application" or "Create Application"

6. Click "New Application"
   ??? URL: /MyApplications/Create
   ??? Page: 6-step wizard form

7. Fill Out Form
   ??? Complete all 6 steps
   ??? Submit

8. Check Email
   ??? Should receive "Application Submitted" email
   ??? To: your-email@gmail.com (without +testmember)
```

---

## ?? **Success Indicators**

### **You'll Know It's Working When:**

```
? Login successful
? See your name in header
? "Member Portal" option visible in dropdown
? Clicking it redirects to /MyApplications
? "New Application" button visible
? Form loads successfully
? Can progress through all 6 steps
? Submit button works
? Receive confirmation email
? Application shows in "My Applications" list
```

---

## ?? **Summary**

### **Correct Path:**

```
Member Login
    ?
Click "Member Portal" (in header dropdown)
    ?
Redirects to /MyApplications
    ?
Click "New Application"
    ?
Fill 6-step form
    ?
Submit
    ?
Email sent! ?
```

### **The "Not an Admin" error means:**

Either:
1. You're trying to access `/Admin/Dashboard` (requires Admin role)
2. User doesn't have correct role assigned
3. Session expired
4. Not logged in

### **Solution:**

1. Make sure user Role = Member (or Client)
2. Make sure Status = Active
3. Login first
4. Use correct URL: /MyApplications

---

**Try it and let me know what happens!** ??
