-- Fix Admin User Data Migration
-- This script updates the admin user to have correct UserRole, Status, and IsActive values
-- Run this on your production database if the admin user was created before the ApplicationUser changes

-- Update admin user to have correct values
UPDATE AspNetUsers
SET 
    UserRole = 4,           -- UserRole.Admin = 4
    Status = 1,             -- MembershipStatus.Active = 1
    IsActive = 1,           -- true
    FirstName = COALESCE(NULLIF(FirstName, ''), 'System'),
    LastName = COALESCE(NULLIF(LastName, ''), 'Administrator')
WHERE 
    Email = 'admin@ellishope.org'
    OR Id IN (SELECT UserId FROM AspNetUserRoles WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Admin'));

-- Verify the update
SELECT 
    Email,
    FirstName,
    LastName,
    UserRole,
    Status,
    IsActive,
    EmailConfirmed,
    JoinedDate
FROM AspNetUsers
WHERE Email = 'admin@ellishope.org';

-- Expected Results:
-- UserRole: 4 (Admin)
-- Status: 1 (Active)
-- IsActive: 1 (true)
-- FirstName: System
-- LastName: Administrator
