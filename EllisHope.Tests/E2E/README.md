# E2E Test Execution Guide

## Overview
This guide helps you run E2E tests to validate the manual test scenarios.

---

## Prerequisites

1. **App must be running:**
   ```powershell
   cd EllisHope
   dotnet run
   ```
   
   App should be running at: https://localhost:7049

2. **Test users must exist in database:**
   - mcarthey+happy@gmail.com (Member)
   - mcarthey+reject@gmail.com (Member)
   - mcarthey+moreinfo@gmail.com (Member)
   - mcarthey+board1@gmail.com (Board Member - Carol Davis)
   - mcarthey+board2@gmail.com (Board Member - David Evans)
   - mcarthey+client1@gmail.com (Client - Alice Johnson)
   - mcarthey+client2@gmail.com (Client - Bob Williams)
   
   All passwords: `Password123!`

---

## Running E2E Tests

### Option 1: Run ALL E2E Tests (that aren't skipped)

```powershell
# Run all E2E tests
dotnet test --filter "Category=E2E"

# With visible browser
$env:HEADED = "1"
dotnet test --filter "Category=E2E"
```

### Option 2: Run Individual Test Files

```powershell
# Run all tests in ApplicationHappyPathTests
dotnet test --filter "FullyQualifiedName~ApplicationHappyPathTests"

# Run all tests in ApplicationWorkflowScenarios
dotnet test --filter "FullyQualifiedName~ApplicationWorkflowScenarios"
```

### Option 3: Run Specific Test by Name

```powershell
# Previous Button test
dotnet test --filter "DisplayName~ApplicationForm_PreviousButton_NavigatesBackward"

# Save Draft test
dotnet test --filter "DisplayName~ApplicationForm_SaveAsDraft"

# Validation test
dotnet test --filter "DisplayName~ApplicationForm_Step3_RequiresMinimum50Characters"

# Happy Path Full Workflow
dotnet test --filter "DisplayName~HappyPath_CompleteApplicationWorkflow_Success"
```

### Option 4: Run Scenario Tests (from your manual test plan)

```powershell
# Happy Path Scenario
dotnet test --filter "DisplayName~Scenario_HappyPath_SubmitReviewVoteApprove"

# Rejection Scenario
dotnet test --filter "DisplayName~Scenario_RejectionPath_SubmitReviewVoteReject"

# Board Member Workflow
dotnet test --filter "DisplayName~Scenario_BoardMember_ReviewAndVote"

# Client Portal
dotnet test --filter "DisplayName~Scenario_Client_ViewProgressAndResources"

# Draft Workflow
dotnet test --filter "DisplayName~Scenario_DraftWorkflow_SaveResumeComplete"
```

### Option 5: Run in Headed Mode (See Browser)

```powershell
$env:HEADED = "1"
dotnet test --filter "Category=E2E"
```

### Option 6: Run in Slow Motion (Debug Issues)

```powershell
$env:SLOWMO = "500"  # 500ms delay between actions
$env:HEADED = "1"
dotnet test --filter "DisplayName~ApplicationForm_PreviousButton"
```

---

## Quick Test Commands (Copy & Paste)

```powershell
# 1. Start the app (Terminal 1)
cd E:\Documents\Work\dev\repos\EHF\EllisHopeFoundation\EllisHope
dotnet run

# 2. Run tests (Terminal 2)
cd E:\Documents\Work\dev\repos\EHF\EllisHopeFoundation

# Test navigation
$env:HEADED = "1"
dotnet test --filter "DisplayName~ApplicationForm_PreviousButton_NavigatesBackward"

# Test save draft
$env:HEADED = "1"
dotnet test --filter "DisplayName~ApplicationForm_SaveAsDraft"

# Test validation
$env:HEADED = "1"
dotnet test --filter "DisplayName~ApplicationForm_Step3_RequiresMinimum50Characters"

# Run full happy path (WARNING: Slow! ~2 minutes)
$env:HEADED = "1"
dotnet test --filter "DisplayName~HappyPath_CompleteApplicationWorkflow_Success"
