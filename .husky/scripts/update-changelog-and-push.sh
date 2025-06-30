#!/bin/sh

set -e

echo "📝 Generate changelog..."
.husky/scripts/generate-changelog.sh

if git diff --quiet --exit-code CHANGELOG.md; then
  echo "✅ CHANGELOG.md not change."
  exit 0
fi

echo "📦 Detected changes in CHANGELOG.md. Adding to git..."
git add CHANGELOG.md