# ?? Manual Test Plan for Draft Save Bug Fix

## ? **THE FIX: Hidden Fields in Multi-Step Form**

### **What Was Wrong:**
When you clicked "Save & Exit" on Step 2, the form only posted Step 2 fields because Step 1 fields weren't in the HTML (they were hidden by the `@if (currentStep == 1)` condition).

### **The Solution:**
Added hidden `<input type="hidden">` fields for ALL other steps, so when you post from any step, ALL the data goes to the server!

---

## ?? **Manual Test Steps**

### **Test 1: Basic Draft Save**

1. ? Login as Mr. Happy Path
2. ? Go to `/MyApplications`
3. ? Click "New Application"
4. ? Fill out Step 1:
   - First Name: `Test`
   - Last Name: `User`
   - Phone: `555-1234`
   - Email: `test@test.com` ? **CRITICAL FIELD!**
   - Address: `123 Test St`
   - City: `Milwaukee`
   - State: `WI`
   - ZIP: `53202`
5. ? Click "Save as Draft"
6. ? Should see "Application saved as draft" message
7. ? Click "Edit" or "Continue" on the draft
8. ? **VERIFY**: All Step 1 data should be there!

---

### **Test 2: Save from Step 2** (THE BUG TEST!)

1. ? Continue from Test 1 OR start fresh with a new draft
2. ? Fill Step 1 completely (if starting fresh)
3. ? Click "Next" to go to Step 2
4. ? Check "Gym Membership"
5. ? Enter Estimated Monthly Cost: `150`
6. ? Click "Save & Exit" (top right button) ? **THE CRITICAL MOMENT!**
7. ? Should see "Draft saved successfully" message
8. ? Click "Edit" on the draft again
9. ? **VERIFY**: Email field should have `test@test.com` ? **IF THIS IS BLANK, BUG STILL EXISTS!**
10. ? **VERIFY**: All Step 1 data should be there!
11. ? Click "Next" to go to Step 2
12. ? **VERIFY**: "Gym Membership" should be checked
13. ? **VERIFY**: Estimated Monthly Cost should be `150`

---

### **Test 3: Save from Step 3**

1. ? Continue from Test 2
2. ? On Step 2, click "Next" to go to Step 3
3. ? Fill Personal Statement: `I am very motivated to improve my health and fitness through this wonderful program.`
4. ? Fill Expected Benefits: `I expect to gain better health, more energy, and improved overall quality of life.`
5. ? Fill Commitment: `I am committed to completing this program and will dedicate the necessary time and effort.`
6. ? Click "Save & Exit"
7. ? Edit the draft again
8. ? **VERIFY**: Step 1 data still there (especially Email!)
9. ? Go to Step 2 ? **VERIFY**: Gym Membership still checked
10. ? Go to Step 3 ? **VERIFY**: All three text areas have your text

---

### **Test 4: Complete Multi-Step Save**

1. ? Create new draft
2. ? Fill Step 1, click "Save as Draft"
3. ? Edit, go to Step 2, fill it, click "Save & Exit"
4. ? Edit, go to Step 3, fill it, click "Save & Exit"
5. ? Edit, go to Step 4, fill it, click "Save & Exit"
6. ? Edit, verify ALL data from Steps 1-4 is preserved!

---

## ?? **What to Look For**

### **? SUCCESS Signs:**
- "Draft saved successfully" message appears
- Email field is NOT empty when you re-edit
- ALL fields from previous steps are preserved
- Form navigation works smoothly
- No database errors in the console

### **? FAILURE Signs:**
- Email field is empty after save
- Any Step 1 required fields are empty
- Database error about NULL values
- Data from previous steps disappears

---

## ?? **If It Still Doesn't Work:**

### **Check These:**

1. **View Source** in browser when on Step 2:
   - Right-click ? View Page Source
   - Search for `<input type="hidden" name="Email"`
   - **Should exist!** If not, hidden fields aren't rendering

2. **Browser Developer Tools:**
   - Open F12
   - Go to Network tab
   - Click "Save & Exit"
   - Look at the POST request
   - Click on it ? Payload/Form Data
   - **Should see ALL fields** including Email, FirstName, etc.

3. **Check the Console:**
   - Any red errors?
   - Any validation errors?

---

## ?? **Expected Form POST Data**

When you click "Save & Exit" from Step 2, the POST should include:

```
Id: 1
CurrentStep: 2
Status: 0

--- Step 1 (via hidden fields) ---
FirstName: Test
LastName: User
Email: test@test.com  ? MUST BE HERE!
PhoneNumber: 555-1234
Address: 123 Test St
City: Milwaukee
State: WI
ZipCode: 53202

--- Step 2 (visible fields) ---
FundingTypesRequested: GymMembership
EstimatedMonthlyCost: 150
ProgramDurationMonths: 12

--- Step 3-6 (via hidden fields, might be empty) ---
PersonalStatement: (empty or previous value)
...etc
```

---

## ? **Technical Details**

### **What We Added:**

In `Edit.cshtml`, around line 100:

```html
@if (currentStep != 1)
{
    <input type="hidden" asp-for="FirstName" />
    <input type="hidden" asp-for="LastName" />
    <input type="hidden" asp-for="PhoneNumber" />
    <input type="hidden" asp-for="Email" />  ? THIS SAVES THE DAY!
    <!-- ... more fields ... -->
}
```

These hidden fields ensure that when you're on Step 2, the form STILL posts Step 1 data!

---

## ?? **Why This Works:**

```
OLD WAY (Broken):
Step 2 HTML only has:
  - Checkboxes
  - Cost field
  - Duration field
Post to server ? Only Step 2 data ? Step 1 overwritten with nulls ? ERROR!

NEW WAY (Fixed):
Step 2 HTML has:
  - Checkboxes (visible)
  - Cost field (visible)
  - Hidden inputs for Step 1 (not visible but included in POST!)
Post to server ? ALL data ? Full update ? SUCCESS!
```

---

## ?? **Ready to Test!**

1. Stop the app if running
2. Rebuild: `dotnet build`
3. Run: `dotnet run`
4. Navigate to: `https://localhost:7049`
5. Follow Test 2 above

**Good luck! ?? No more crying! ??**

---

*P.S. If it STILL doesn't work, check the browser DevTools Network tab to see what's actually being posted!*
