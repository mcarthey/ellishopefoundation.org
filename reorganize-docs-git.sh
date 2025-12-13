#!/bin/bash
# Git Move Script - Preserves file history
# Reorganizes documentation into /docs folder structure

echo "Creating /docs folder structure..."

# Create directories
mkdir -p docs/development
mkdir -p docs/deployment
mkdir -p docs/security

echo "Moving files with Git (preserves history)..."

# Development docs
if [ -f "CONFIGURATION.md" ]; then
    git mv CONFIGURATION.md docs/development/configuration.md
    echo "? Moved: CONFIGURATION.md ? docs/development/configuration.md"
fi

if [ -f "CI-CD-SETUP.md" ]; then
    git mv CI-CD-SETUP.md docs/development/ci-cd-setup.md
    echo "? Moved: CI-CD-SETUP.md ? docs/development/ci-cd-setup.md"
fi

if [ -f "SECRETS-MANAGEMENT.md" ]; then
    git mv SECRETS-MANAGEMENT.md docs/development/secrets-management.md
    echo "? Moved: SECRETS-MANAGEMENT.md ? docs/development/secrets-management.md"
fi

if [ -f "TINYMCE-SETUP.md" ]; then
    git mv TINYMCE-SETUP.md docs/development/tinymce-setup.md
    echo "? Moved: TINYMCE-SETUP.md ? docs/development/tinymce-setup.md"
fi

# Deployment docs
if [ -f "DEPLOYMENT-GUIDE.md" ]; then
    git mv DEPLOYMENT-GUIDE.md docs/deployment/deployment-guide.md
    echo "? Moved: DEPLOYMENT-GUIDE.md ? docs/deployment/deployment-guide.md"
fi

if [ -f "HTTPS-SETUP-GUIDE.md" ]; then
    git mv HTTPS-SETUP-GUIDE.md docs/deployment/https-setup-guide.md
    echo "? Moved: HTTPS-SETUP-GUIDE.md ? docs/deployment/https-setup-guide.md"
fi

if [ -f "HTTPS-SETUP-CHECKLIST.md" ]; then
    git mv HTTPS-SETUP-CHECKLIST.md docs/deployment/https-setup-checklist.md
    echo "? Moved: HTTPS-SETUP-CHECKLIST.md ? docs/deployment/https-setup-checklist.md"
fi

if [ -f "POST-SSL-CHECKLIST.md" ]; then
    git mv POST-SSL-CHECKLIST.md docs/deployment/post-ssl-checklist.md
    echo "? Moved: POST-SSL-CHECKLIST.md ? docs/deployment/post-ssl-checklist.md"
fi

if [ -f "QUICK-REFERENCE.md" ]; then
    git mv QUICK-REFERENCE.md docs/deployment/quick-reference.md
    echo "? Moved: QUICK-REFERENCE.md ? docs/deployment/quick-reference.md"
fi

# Security docs
if [ -f "ENCRYPTED-CONFIGURATION.md" ]; then
    git mv ENCRYPTED-CONFIGURATION.md docs/security/encrypted-configuration.md
    echo "? Moved: ENCRYPTED-CONFIGURATION.md ? docs/security/encrypted-configuration.md"
fi

if [ -f "HTTPS-CONFIGURATION.md" ]; then
    git mv HTTPS-CONFIGURATION.md docs/security/https-configuration.md
    echo "? Moved: HTTPS-CONFIGURATION.md ? docs/security/https-configuration.md"
fi

echo ""
echo "? Documentation reorganization complete!"
echo ""
echo "Next steps:"
echo "  1. Review the changes: git status"
echo "  2. Commit: git commit -m 'docs: Reorganize documentation into /docs folder'"
echo "  3. Push: git push origin main"
echo ""
