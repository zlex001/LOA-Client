#!/bin/bash

# SVN Auto Commit Script for Mac
# This script reads commit message from commit_msg.txt and commits changes

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
COMMIT_MSG_FILE="$SCRIPT_DIR/commit_msg.txt"

cd "$PROJECT_DIR" || exit 1

# Check if commit message file exists
if [ ! -f "$COMMIT_MSG_FILE" ]; then
    echo "Error: commit_msg.txt not found"
    exit 1
fi

# Read commit message
COMMIT_MSG=$(cat "$COMMIT_MSG_FILE")

if [ -z "$COMMIT_MSG" ]; then
    echo "Error: commit message is empty"
    exit 1
fi

echo "Project directory: $PROJECT_DIR"
echo "Commit message: $COMMIT_MSG"
echo ""

# Add all new files (excluding Unity generated folders)
echo "Adding new files..."
svn add --force . --auto-props --parents --depth infinity -q 2>/dev/null

# Revert Unity generated folders that should not be committed
svn revert --depth infinity Library/ 2>/dev/null
svn revert --depth infinity Logs/ 2>/dev/null
svn revert --depth infinity Temp/ 2>/dev/null
svn revert --depth infinity UserSettings/ 2>/dev/null
svn revert --depth infinity obj/ 2>/dev/null

# Check for changes
svn status

# Commit
echo ""
echo "Committing..."
svn commit -m "$COMMIT_MSG"

echo ""
echo "Done."
