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

# Add untracked files, skipping Unity generated folders
echo "Adding new files..."
EXCLUDE_PATTERN='^\?\s+(Library|Logs|Temp|UserSettings|obj)(/|$)'
svn status | grep '^\?' | grep -v -E "$EXCLUDE_PATTERN" | awk '{print $2}' | while IFS= read -r f; do
    svn add --parents -q "$f" 2>/dev/null
done

# Check for changes
svn status --ignore-externals | grep -v -E '^\?\s+(Library|Logs|Temp|UserSettings|obj)(/|$)'

# Commit
echo ""
echo "Committing..."
svn commit -m "$COMMIT_MSG"

echo ""
echo "Done."
