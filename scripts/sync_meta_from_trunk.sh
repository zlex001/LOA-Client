#!/usr/bin/env bash
# Copy .meta files from trunk/Assets to LOA-Client-git/Assets for paths that exist in both.
# Git-only paths (e.g. Assets/Game) keep their existing .meta; only overlapping paths are overwritten.
BASE="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd | tr -d '\r')"
TRUNK_ASSETS="${BASE}/../trunk/Assets"
GIT_ASSETS="${BASE}/Assets"
if [ ! -d "$TRUNK_ASSETS" ]; then
  echo "Error: trunk Assets not found at $TRUNK_ASSETS"
  exit 1
fi
if [ ! -d "$GIT_ASSETS" ]; then
  echo "Error: git Assets not found at $GIT_ASSETS"
  exit 1
fi
sync_count=$(find "$TRUNK_ASSETS" -name "*.meta" | while read -r meta; do
  rel="${meta#$TRUNK_ASSETS/}"
  rel="${rel#/}"
  dest="${GIT_ASSETS}/${rel}"
  if [ -f "$dest" ]; then
    cp "$meta" "$dest"
    echo 1
  fi
done | wc -l | tr -d ' ')
echo "Synced ${sync_count} .meta files from trunk to LOA-Client-git/Assets."
