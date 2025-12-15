# User Management & Portal System - Design Document

**Date:** December 15, 2024  
**Status:** ?? **Design Phase**  
**Scope:** Comprehensive authentication, authorization, and user portal system

---

## ?? **Vision**

Create a multi-tiered user system where different user types have access to appropriate features:

- **Admins** - Full system control
- **Board Members** - Oversight, reporting, approvals
- **Sponsors** - View sponsored clients, donations, impact
- **Clients** - Personal portal with programs, progress, communication
- **Members** - Basic account holders (future clients, newsletter, etc.)

---

## ?? **User Roles & Capabilities**

### 1. **Admin**
**Access:** Everything

**Capabilities:**
- User management (create, edit, delete, assign roles)
- Content management (pages, blog, events, causes)
- Media library management
- System settings
- Reports and analytics
- Financial oversight
- Program assignment

**Portal Features:**
- Admin dashboard
- User list with filters
- System health monitoring
- Audit logs

---

### 2. **Board Member**
**Access:** View-mostly with limited approvals

**Capabilities:**
- View all reports and analytics
- Approve program assignments
- Approve sponsor-client pairings
- View financial summaries
- Export reports
- Send communications to members

**Portal Features:**
- Executive dashboard
- Report viewer
- Approval queue
- Member directory (view-only)

---

### 3. **Sponsor**
**Access:** Their sponsored clients only

**Capabilities:**
- View sponsored client profiles
- View client progress and programs
- View donation history
- Send messages to clients (via system)
- Upload encouragement messages
- View impact reports

**Portal Features:**
- Sponsor dashboard
- "My Clients" list
- Donation history
- Impact summary
- Message center

---

### 4. **Client**
**Access:** Their own data only

**Capabilities:**
- View membership status
- View assigned programs (exercise, nutrition)
- Track progress
- View sponsor information
- Complete Q&A/intake forms
- Upload progress photos (optional)
- Message foundation/sponsor
- View billing/costs
- Update profile

**Portal Features:**
- Client dashboard
- My Programs
- Progress tracker
- Profile management
- Billing/payments
- Message center
- Resources library

---

### 5. **Member**
**Access:** Basic account features

**Capabilities:**
- Maintain profile
- Subscribe to newsletter
- Register for events
- Make donations
- View public resources
- Apply to become a client

**Portal Features:**
- Member dashboard
- Event registration
- Donation history
- Profile settings
- Application forms

---

## ??? **Database Schema**

### Extended User Model

```csharp
public class ApplicationUser : IdentityUser
{
    // Basic Info
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Contact
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    
    // Account Status
    public MembershipStatus Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Client-specific
    public int? SponsorId { get; set; }
    public ApplicationUser? Sponsor { get; set; }
    public ICollection<ApplicationUser> SponsoredClients { get; set; }
    
    public decimal? MonthlyFee { get; set; }
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    
    // Navigation
    public ICollection<Program> AssignedPrograms { get; set; }
    public ICollection<ProgressEntry> ProgressEntries { get; set; }
    public ICollection<QAResponse> QAResponses { get; set; }
    public ICollection<Message> SentMessages { get; set; }
    public ICollection<Message> ReceivedMessages { get; set; }
    public ICollection<Payment> Payments { get; set; }
}

public enum MembershipStatus
{
    Pending,      // Application submitted
    Active,       // Current member/client
    Inactive,     // Temporarily paused
    Expired,      // Membership lapsed
    Cancelled     // Account closed
}
```

### New Domain Models

```csharp
// Programs assigned to clients
public class Program
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ProgramType Type { get; set; } // Exercise, Nutrition, Combined
    public string? DocumentUrl { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    
    public string ClientId { get; set; }
    public ApplicationUser Client { get; set; }
    
    public string? AssignedById { get; set; }
    public ApplicationUser? AssignedBy { get; set; }
}

// Client progress tracking
public class ProgressEntry
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public ApplicationUser Client { get; set; }
    
    public DateTime EntryDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? BodyFat { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    
    public Dictionary<string, string> Measurements { get; set; } // Chest, waist, etc.
}

// Q&A Responses (intake forms)
public class QAResponse
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public ApplicationUser Client { get; set; }
    
    public int QuestionId { get; set; }
    public Question Question { get; set; }
    
    public string Response { get; set; }
    public DateTime ResponseDate { get; set; }
}

public class Question
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public QuestionType Type { get; set; } // Text, MultipleChoice, YesNo, Scale
    public string? Options { get; set; } // JSON for multiple choice
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string Category { get; set; } // Health, Fitness, Goals, etc.
}

// Internal messaging
public class Message
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public ApplicationUser Sender { get; set; }
    
    public string RecipientId { get; set; }
    public ApplicationUser Recipient { get; set; }
    
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime SentDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
}

// Payments/Billing
public class Payment
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public ApplicationUser Client { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}
```

---

## ?? **Authorization Strategy**

### Role-Based Access Control (RBAC)

```csharp
[Authorize(Roles = "Admin")]                    // Admin only
[Authorize(Roles = "Admin,BoardMember")]        // Admin or Board
[Authorize(Roles = "Admin,Editor")]             // Content management
[Authorize(Roles = "Sponsor")]                  // Sponsors only
[Authorize(Roles = "Client")]                   // Clients only
[Authorize]                                     // Any authenticated user
```

### Claim-Based Authorization (Advanced)

```csharp
// For fine-grained permissions
[Authorize(Policy = "CanViewClientData")]
[Authorize(Policy = "CanAssignPrograms")]
[Authorize(Policy = "CanApproveSponsors")]
```

### Resource-Based Authorization

```csharp
// For checking if user can access specific resource
var authResult = await _authorizationService.AuthorizeAsync(
    User, 
    resource, 
    "CanViewOwnData");
```

---

## ?? **UI/UX Design**

### Portal Landing (After Login)

**Route:** `/Portal` or `/Dashboard`  
**Layout:** `_PortalLayout.cshtml`

**Navigation:**
```
???????????????????????????????????????
? Ellis Hope Foundation   [User Menu] ?
???????????????????????????????????????
? [Dashboard] [Programs] [Messages]   ?  ? Role-specific nav
???????????????????????????????????????
?                                     ?
?  [Main Content Area]                ?
?  - Role-specific dashboard          ?
?  - Widgets and cards                ?
?  - Quick actions                    ?
?                                     ?
???????????????????????????????????????
```

### Dashboard Widgets by Role

**Admin:**
- Total users by role
- New registrations this month
- Active programs count
- Recent activity feed
- System health

**Board Member:**
- Pending approvals
- Monthly report summary
- Active clients count
- Fundraising progress

**Sponsor:**
- My clients (cards with photos)
- Recent client progress
- Upcoming milestones
- Donation summary

**Client:**
- Current programs
- Progress this week
- Upcoming appointments
- Messages from sponsor/foundation
- Payment status

**Member:**
- Upcoming events
- Latest blog posts
- Donation impact
- Application status

---

## ?? **Page Structure**

### Admin Area
```
/Admin
  /Users
    Index         - User list with filters
    Create        - Add new user
    Edit/{id}     - Edit user
    Details/{id}  - View user details
    Delete/{id}   - Delete confirmation
    
  /Programs
    Index         - Program templates
    Assign        - Assign to client
    
  /Reports
    MembershipReport
    ProgramReport
    FinancialReport
```

### Portal Area
```
/Portal
  /Dashboard      - Role-specific landing
  
  /Profile
    View          - View own profile
    Edit          - Edit profile
    ChangePassword
    
  /Programs       - (Client only)
    Index         - My programs
    Details/{id}  - Program details
    
  /Progress       - (Client only)
    Index         - Progress history
    Add           - Log progress
    
  /Clients        - (Sponsor only)
    Index         - My sponsored clients
    Details/{id}  - Client details
    
  /Messages
    Inbox
    Sent
    Compose
    Details/{id}
    
  /Billing        - (Client only)
    Index         - Payment history
    Pay           - Make payment
```

---

## ?? **User Flows**

### New Client Onboarding
1. Member applies to become client (form submission)
2. Admin reviews application
3. Admin approves ? Creates client account
4. Admin assigns sponsor (optional)
5. Admin assigns programs
6. Client receives welcome email with login
7. Client logs in, completes intake Q&A
8. Client accesses programs and portal

### Sponsor Workflow
1. Admin creates sponsor account
2. Admin assigns clients to sponsor
3. Sponsor logs in, sees sponsored clients
4. Sponsor views client progress
5. Sponsor sends encouragement messages
6. Sponsor makes additional donations

### Program Assignment
1. Admin/Board creates program (exercise/nutrition plan)
2. Admin assigns program to client
3. Client receives notification
4. Client views program in portal
5. Client tracks progress
6. Admin/Sponsor views progress

---

## ?? **Technical Implementation**

### Phase 1: Foundation (Week 1)
- [ ] Extend `IdentityUser` ? `ApplicationUser`
- [ ] Create migration for extended user fields
- [ ] Update `DbSeeder` with new roles
- [ ] Create `ApplicationUser` view models
- [ ] Update authentication to use `ApplicationUser`

### Phase 2: User Management (Week 1-2)
- [ ] Admin Users controller
- [ ] User CRUD operations
- [ ] Role assignment interface
- [ ] User filtering and search
- [ ] User detail view

### Phase 3: Domain Models (Week 2)
- [ ] `Program` model and migrations
- [ ] `ProgressEntry` model
- [ ] `Question` and `QAResponse` models
- [ ] `Message` model
- [ ] `Payment` model
- [ ] Services for each domain

### Phase 4: Portal Framework (Week 2-3)
- [ ] Portal area setup
- [ ] `_PortalLayout.cshtml`
- [ ] Role-based dashboard routing
- [ ] Navigation helpers
- [ ] Dashboard widgets

### Phase 5: Role-Specific Features (Week 3-4)
- [ ] Admin dashboard
- [ ] Board member dashboard
- [ ] Sponsor dashboard
- [ ] Client dashboard
- [ ] Member dashboard

### Phase 6: Client Features (Week 4)
- [ ] Program viewing
- [ ] Progress tracking
- [ ] Q&A forms
- [ ] Profile management

### Phase 7: Communication (Week 4-5)
- [ ] Internal messaging system
- [ ] Email notifications
- [ ] SMS notifications (future)

### Phase 8: Billing (Week 5)
- [ ] Payment history
- [ ] Payment processing (Stripe integration)
- [ ] Invoice generation

---

## ?? **Testing Strategy**

- Unit tests for services
- Integration tests for workflows
- UI tests for critical paths
- Role-based authorization tests
- Data privacy tests (clients can't see other clients)

---

## ?? **Success Metrics**

- All 5 roles implemented and working
- Users can self-serve common tasks
- Admin time reduced by 50%
- Client engagement increased
- Sponsor satisfaction improved

---

## ?? **Next Steps**

1. **Review this design** - Approve or suggest changes
2. **Prioritize features** - What's most critical first?
3. **Start implementation** - Phase by phase
4. **Iterate** - Add features as needed

---

**Questions for Discussion:**

1. **Payment Processing:** Do you want to integrate Stripe/PayPal now or later?
2. **Email:** Do you have an SMTP provider, or should I use SendGrid/Mailgun?
3. **SMS:** Future feature or include now?
4. **Photo Uploads:** For progress tracking - should we implement now?
5. **Q&A Forms:** Should these be customizable by admins, or predefined?

---

**Status:** ?? Design Complete - Awaiting Approval  
**Estimated Timeline:** 4-5 weeks for full implementation  
**Recommended Approach:** Incremental (2-3 features per week)

