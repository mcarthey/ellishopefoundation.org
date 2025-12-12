# PowerShell Git Scripts

## Prerequisites

Install GitHub CLI:
```powershell
winget install --id GitHub.cli
```

After installation, authenticate:
```powershell
gh auth login
```

## Scripts

### quick-merge.ps1 (Recommended)

The fastest way to merge your changes. Creates a PR with auto-generated description and merges it automatically.

```powershell
# From your feature branch, run:
.\quick-merge.ps1

# Or with a custom title:
.\quick-merge.ps1 "Add admin dashboard and authentication"
```

**What it does:**
1. Pushes any unpushed commits
2. Gathers all commit messages from your branch
3. Creates a detailed PR description
4. Merges the PR
5. Deletes the remote and local branch
6. Switches you back to main
7. Pulls the latest changes

### merge-branch.ps1 (Advanced)

Full-featured script with more control.

```powershell
# Auto-merge (default)
.\merge-branch.ps1

# Custom title
.\merge-branch.ps1 -Title "My feature description"

# Create PR but don't auto-merge
.\merge-branch.ps1 -AutoMerge $false
```

## Typical Workflow

```powershell
# 1. Make your changes and commit them
git add .
git commit -m "Add new feature"

# 2. Push and merge (one command!)
.\quick-merge.ps1

# Done! You're back on main with all changes merged.
```

## Tips

- The scripts automatically generate PR descriptions from your commit messages
- Write good commit messages and your PRs will have great descriptions
- The script won't work if you're on main/master branch (safety feature)
- Branches are automatically cleaned up after merge
