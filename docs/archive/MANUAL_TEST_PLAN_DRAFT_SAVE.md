# ?? Manual Test Plan for Draft Save Bug Fix

## ? **THE FIX: Conditional Hidden Fields**

### **What Was Wrong (v2):**
1. Hidden fields were posting empty values for steps not yet completed
2. Database rejected NULL values for required fields like `CommitmentStatement`
3. Generic error message didn't help users understand the problem

### **The Solution:**
1. Only include hidden fields for steps that have been **completed**
2. Check if data exists before adding hidden fields
3. Better error messages that users can actually understand

---

## ?? **What Changed:**

### **Smart Hidden Fields:**
```html
@* Only include Step 3 if PersonalStatement exists *@
@if (currentStep != 3 && !string.IsNullOrEmpty(Model.PersonalStatement))
{
    <input type="hidden" asp-for="PersonalStatement" />
    <input type="hidden" asp-for="CommitmentStatement" />
}
```

### **User-Friendly Error Messages:**
- ? OLD: "An error occurred while saving the entity changes. See the inner exception for details."
- ? NEW: "Unable to save your draft at this time. Please try again or contact support if the problem persists."

---
