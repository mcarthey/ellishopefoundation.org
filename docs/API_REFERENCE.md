# API Reference � Ellis Hope Foundation (Complete)

This is the canonical API reference for the Ellis Hope application. It documents the main endpoints (Razor views and important POST actions) used by the admin UI and public site. Keep this file up to date when routes, actions or authorization change.

Notes
- Admin area controllers live under `Areas/Admin/Controllers/` and use routes like `/Admin/{Controller}/{Action}/{id?}`.
- Public controllers live under the main app `Controllers/` and have routes like `/{Controller}/{Action}/{id?}` or custom patterns defined in `Program.cs`.
- Authentication: ASP.NET Core Identity. Roles: `Admin`, `BoardMember`, `Editor`, `Sponsor`, `Client`, `Member`.
- Anti-forgery: POST endpoints expect a valid antiforgery token in production. Integration tests may run with antiforgery disabled or using a test override.

CONTENTS
- Admin area
  - AccountController
  - ApplicationsController
  - BlogController (Admin)
  - BoardMemberController
  - CausesController (Admin)
  - ClientController
  - DashboardController
  - EventsController (Admin)
  - MediaController
  - MediaMigrationController
  - MemberController
  - PagesController (Admin)
  - ProfileController
  - SponsorController
  - UsersController
- Public controllers
  - HomeController
  - AboutController
  - BlogController (Public)
  - CausesController (Public)
  - EventController (Public)
  - MyApplicationsController
  - ContactController
  - ServicesController
  - TeamController
  - FaqController

---

ADMIN AREA
=========

AccountController
-----------------
Base path: `/Admin/Account`

- GET `/Admin/Account/Login` � show login page.
- POST `/Admin/Account/Login` � sign in (fields: `Email`, `Password`, `RememberMe`). Anti-forgery required.
- POST `/Admin/Account/Logout` � sign out. Anti-forgery required.
- GET `/Admin/Account/Register` � registration page.
- POST `/Admin/Account/Register` � register new user (RegisterViewModel fields). Anti-forgery required.
- GET `/Admin/Account/AccessDenied` � access denied view.
- GET `/Admin/Account/Lockout` � lockout view.

Notes: Account actions are [AllowAnonymous] for Login/Register views but POST actions require antiforgery.

ApplicationsController
----------------------
Base path: `/Admin/Applications`

(Full reference � key endpoints)
- GET `/Admin/Applications` � list applications; optional `status`, `searchTerm` query parameters. Roles: Admin, BoardMember.
- GET `/Admin/Applications/Index` � alias for list (same as `/Admin/Applications`).
- GET `/Admin/Applications/NeedingReview` � list apps needing current user's review.
- GET `/Admin/Applications/Details/{id}` � app details with votes/comments. Roles: Admin, BoardMember.
- GET `/Admin/Applications/Review/{id}` � alias to Details for review.
- POST `/Admin/Applications/Vote` � cast vote (VoteFormViewModel: ApplicationId, Decision, Reasoning, ConfidenceLevel). Roles: BoardMember. Anti-forgery required.
- POST `/Admin/Applications/Comment` � add comment (CommentFormViewModel: ApplicationId, Content, IsPrivate, ...). Anti-forgery required.
- GET `/Admin/Applications/Approve/{id}` � show approve form. Roles: Admin.
- POST `/Admin/Applications/Approve` � approve application (ApproveApplicationViewModel). Roles: Admin. Anti-forgery required.
- GET `/Admin/Applications/Reject/{id}` � show reject form. Roles: Admin.
- POST `/Admin/Applications/Reject` � reject application (RejectApplicationViewModel). Roles: Admin. Anti-forgery required.
- GET `/Admin/Applications/RequestInfo/{id}` � show request info form.
- POST `/Admin/Applications/RequestInfo` � request info (RequestInformationViewModel). Anti-forgery required.
- POST `/Admin/Applications/StartReview/{id}` � start review workflow (Admin). Anti-forgery required.
- GET `/Admin/Applications/Statistics` � view statistics summary.

BlogController (Admin)
----------------------
Base path: `/Admin/Blog`

- GET `/Admin/Blog` � list/filter posts (`searchTerm`, `categoryFilter`, `showUnpublished`). Roles: Admin, Editor.
- GET `/Admin/Blog/Index` � alias for list
- GET `/Admin/Blog/Create` � show create form.
- POST `/Admin/Blog/Create` � create post (BlogPostViewModel). Anti-forgery required. Roles: Admin, Editor.
- GET `/Admin/Blog/Edit/{id}` � edit form.
- POST `/Admin/Blog/Edit/{id}` � update post. Anti-forgery required.
- POST `/Admin/Blog/Delete/{id}` � delete post. Anti-forgery required.

BoardMemberController
---------------------
Base path: `/Admin/BoardMember`

- GET `/Admin/BoardMember/Dashboard` � board member dashboard (list items needing review). Roles: BoardMember.
- Additional actions: team-specific pages and reports for board members.

CausesController (Admin)
------------------------
Base path: `/Admin/Causes`

- GET `/Admin/Causes` � list causes. Roles: Admin, Editor.
- GET `/Admin/Causes/Index` � alias for list
- GET `/Admin/Causes/Create` � create form.
- POST `/Admin/Causes/Create` � create cause. Anti-forgery required.
- GET `/Admin/Causes/Edit/{id}` � edit cause.
- POST `/Admin/Causes/Edit/{id}` � update cause. Anti-forgery required.
- POST `/Admin/Causes/Delete/{id}` � delete cause. Anti-forgery required.

ClientController
----------------
Base path: `/Admin/Client`

- GET `/Admin/Client/Dashboard` � client admin dashboard.
- GET `/Admin/Client/MyProfile` � view/edit client profile.
- GET `/Admin/Client/Progress` � client progress view.
- GET `/Admin/Client/Resources` � client resources.

DashboardController
-------------------
Base path: `/Admin/Dashboard`

- GET `/Admin/Dashboard/Index` � admin dashboard. Roles: Admin.

EventsController (Admin)
------------------------
Base path: `/Admin/Events`

- GET `/Admin/Events` � list events. Roles: Admin, Editor.
- GET `/Admin/Events/Index` � alias for list
- GET `/Admin/Events/Create` � create form.
- POST `/Admin/Events/Create` � create event. Anti-forgery required.
- GET `/Admin/Events/Edit/{id}` � edit event.
- POST `/Admin/Events/Edit/{id}` � update event. Anti-forgery required.
- POST `/Admin/Events/Delete/{id}` � delete event. Anti-forgery required.

MediaController
---------------
Base path: `/Admin/Media`

- GET `/Admin/Media` � media index/list. Roles: Admin, Editor.
- GET `/Admin/Media/Index` � alias for list
- GET `/Admin/Media/Upload` � show upload form.
- POST `/Admin/Media/Upload` � upload file (multipart form). Anti-forgery required.
- GET `/Admin/Media/UnsplashSearch` � Unsplash search UI.
- POST `/Admin/Media/UnsplashSearch` � perform Unsplash search.
- GET `/Admin/Media/Edit/{id}` � edit metadata.
- POST `/Admin/Media/Edit/{id}` � update metadata. Anti-forgery required.
- POST `/Admin/Media/Delete/{id}` � delete media. Anti-forgery required.
- POST `/Admin/Media/DeleteAllUnused` � remove unused media (Admin tool).
- GET `/Admin/Media/GetDuplicates` � list duplicate media (Admin tool).
- GET `/Admin/Media/GetUnusedMedia` � list unused media (Admin tool).
- POST `/Admin/Media/ImportFromUnsplash` � import image from Unsplash.
- POST `/Admin/Media/RemoveDuplicates` � remove duplicates.
- GET `/Admin/Media/GetMediaJson` � JSON API for media listing (may require auth).
- GET `/Admin/Media/Usages/{id}` � show usages for a media item.

MediaMigrationController
------------------------
Base path: `/Admin/MediaMigration`

- GET `/Admin/MediaMigration/Index` � migration overview.
- GET `/Admin/MediaMigration/BrokenReferences` � list broken media refs.
- POST `/Admin/MediaMigration/Migrate` � run migration tool.
- POST `/Admin/MediaMigration/RemoveDuplicates` � remove duplicates discovered during migration.

MemberController
----------------
Base path: `/Admin/Member`

- GET `/Admin/Member/Dashboard` � member admin dashboard.
- GET `/Admin/Member/Events` � member events listing.
- GET `/Admin/Member/MyProfile` � member profile.
- GET `/Admin/Member/Volunteer` � volunteer opportunities and management.

PagesController (Admin)
-----------------------
Base path: `/Admin/Pages`

- GET `/Admin/Pages` � list pages (Admin, Editor).
- GET `/Admin/Pages/Index` � alias for list
- GET `/Admin/Pages/Edit/{id}` � edit page (sections, images).
- POST `/Admin/Pages/UpdateContent` � update section content. Anti-forgery required.
- POST `/Admin/Pages/UpdateImage` � update image reference. Anti-forgery required.
- POST `/Admin/Pages/RemoveImage` � remove image from page. Anti-forgery required.
- GET `/Admin/Pages/MediaPicker` � media picker view.

ProfileController
-----------------
Base path: `/Admin/Profile`

- GET `/Admin/Profile/Index` � show current user's profile.
- POST `/Admin/Profile/Edit` � update profile information. Anti-forgery required.
- POST `/Admin/Profile/ChangePassword` � change password. Anti-forgery required.

SponsorController
-----------------
Base path: `/Admin/Sponsor`

- GET `/Admin/Sponsor/Dashboard` � sponsor dashboard.
- GET `/Admin/Sponsor/MyProfile` � sponsor profile.
- GET `/Admin/Sponsor/ClientDetails` � view client details assigned to sponsor.

UsersController
---------------
Base path: `/Admin/Users`

- GET `/Admin/Users` � list users. Roles: Admin.
- GET `/Admin/Users/Index` � alias for list
- GET `/Admin/Users/Create` � create user form.
- POST `/Admin/Users/Create` � create user, assign role. Anti-forgery required.
- GET `/Admin/Users/Edit/{id}` � edit user.
- POST `/Admin/Users/Edit/{id}` � update user. Anti-forgery required.
- GET `/Admin/Users/Details` � view user details.
- POST `/Admin/Users/Delete` � delete user (confirmation flow).
- POST `/Admin/Users/DeleteConfirmed` � delete confirmed action.


PUBLIC CONTROLLERS
==================

HomeController
--------------
Base path: `/` (root)

- GET `/` � home page (Index).
- GET `/Home/Index` � alias for home page.
- GET `/Home/Index2` � alternate home/demo page.
- GET `/Home/Index3` � alternate home/demo page.
- GET `/Home/Index4` � alternate home/demo page.
- GET `/Home/Index5` � alternate home/demo page.
- GET `/Home/Index6` � alternate home/demo page.
- GET `/Home/Index7` � alternate home/demo page.
- GET `/Home/Index8` � alternate home/demo page.

AboutController
---------------
- GET `/About` � about page (alias: `/About/Index`).

BlogController (Public)
------------------------
- GET `/Blog` � list posts, filters and search.
- GET `/Blog/classic` � classic layout listing.
- GET `/Blog/grid` � grid layout listing.
- GET `/Blog/details` � details view (by slug normally).

CausesController (Public)
-------------------------
- GET `/Causes` � list causes
- GET `/Causes/details` � cause details
- GET `/Causes/grid` � grid layout
- GET `/Causes/list` � list layout

EventController (Public)
------------------------
- GET `/Event` � list events
- GET `/Event/details` � details by slug
- GET `/Event/grid` � grid layout
- GET `/Event/list` � list layout

MyApplicationsController
------------------------
Base path: `/MyApplications`

- GET `/MyApplications` � index of user's applications (alias)
- GET `/MyApplications/Index` � alias for the list view
- GET `/MyApplications/Create` � start multi-step application wizard (GET for first step)
- POST `/MyApplications/Create` � post form data for steps; supports draft saving and final submit. Anti-forgery required.
- GET `/MyApplications/Details/{id}` � applicant can view their application.
- GET `/MyApplications/Edit/{id}` � edit draft.
- POST `/MyApplications/UploadDocument` � upload supporting documents (multipart). Anti-forgery required.
- POST `/MyApplications/Withdraw/{id}` � withdraw an application.

ContactController
-----------------
- GET `/Contact` and `/Contact/Index` � contact form
- POST `/Contact` � send message (anti-forgery)
- POST `/Contact/v2` � alternate contact API/version

ServicesController
------------------
- GET `/Services` and `/Services/Index` � services offered list
- GET `/Services/v2` � alternate services view

TeamController
--------------
- GET `/Team/details` � team member details
- GET `/Team/v1` and `/Team/v2` � alternate team views

FaqController
-------------
- GET `/Faq` and `/Faq/Index` � FAQ page

ErrorController
---------------
- GET `/Error` and `/Error/Index` � error view (used by exception handler)


DOCUMENTATION & MAINTENANCE NOTES
================================
- When adding or changing routes, update this file and `docs/DEVELOPER-GUIDE.md`.
- For any new POST endpoints: add antiforgery, update integration tests and E2E README.
- For API endpoints returning JSON (if added), include sample request/response structures.

This file is intended to be the complete reference for application endpoints. Keep it current.
