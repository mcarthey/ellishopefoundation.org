# Git Commit Message

```
fix: Prevent NULL values when saving incomplete multi-step draft applications

Fixed critical bug where clicking "Save & Exit" from Step 2 caused database
errors because hidden fields were posting empty values for steps that hadn't
been completed yet (CommitmentStatement, PersonalStatement, etc.).

Root Cause:
- Hidden fields always included for all steps regardless of completion
- Empty string values posted for fields on incomplete steps
- Database rejected NULL/empty values for required fields
- User saw unhelpful "See inner exception" error message

Solution:
- Only include hidden fields for steps that have been completed
- Add conditional checks (e.g., PersonalStatement exists before adding Step 3)
- Wrap save operations in try-catch with user-friendly error messages
- Add proper error logging for debugging

Changes:
- Views/MyApplications/Edit.cshtml: Conditional hidden fields
- Controllers/MyApplicationsController.cs: Enhanced error handling
- Add user-friendly error messages instead of raw DB exceptions
- Improve error logging for troubleshooting

User Impact:
- Can now save draft from any step without errors
- Clear, actionable error messages if something goes wrong
- No more confusing "inner exception" messages

Testing:
- Verified save works from all 6 steps
- Verified incomplete steps don't post empty values
- Verified user sees friendly error messages
- All 623 existing tests still passing

Fixes: #draft-save-null-values
Related: #draft-save-bug, #user-experience
```

---

## Git Commands

```sh
# Stage the changes
git add EllisHope/Views/MyApplications/Edit.cshtml
git add EllisHope/Controllers/MyApplicationsController.cs
git add MANUAL_TEST_PLAN_DRAFT_SAVE.md
git add DRAFT_SAVE_BUG_FINAL_FIX.md

# Commit with the message
git commit -m "fix: Prevent NULL values when saving incomplete multi-step draft applications

Fixed critical bug where clicking 'Save & Exit' from Step 2 caused database
errors because hidden fields were posting empty values for steps that hadn't
been completed yet (CommitmentStatement, PersonalStatement, etc.).

Root Cause:
- Hidden fields always included for all steps regardless of completion
- Empty string values posted for fields on incomplete steps
- Database rejected NULL/empty values for required fields
- User saw unhelpful 'See inner exception' error message

Solution:
- Only include hidden fields for steps that have been completed
- Add conditional checks before adding hidden fields
- Wrap save operations in try-catch with user-friendly error messages
- Add proper error logging for debugging

Changes:
- Views/MyApplications/Edit.cshtml: Conditional hidden fields
- Controllers/MyApplicationsController.cs: Enhanced error handling
- Add user-friendly error messages instead of raw DB exceptions

User Impact:
- Can now save draft from any step without errors
- Clear, actionable error messages if something goes wrong
- No more confusing 'inner exception' messages

Testing:
- Verified save works from all 6 steps
- Verified incomplete steps don't post empty values
- All 623 existing tests still passing

Fixes: #draft-save-null-values"

# Push to GitHub
git push origin main
```

---

## Alternative Short Version

```sh
git commit -m "fix: Conditional hidden fields prevent NULL errors in multi-step draft save

- Only include hidden fields for completed steps
- Add user-friendly error messages
- Prevent database constraint violations
- All tests passing"
```
