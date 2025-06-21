#!/bin/bash

set -e

CHANGELOG_FILE="CHANGELOG.md"
TEMP_FILE="CHANGELOG_TEMP.md"

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")

TODAY=$(date +"%Y-%m-%d")

if [ -z "$LAST_TAG" ]; then
    COMMITS=$(git log --pretty=format:"%s")
    VERSION="Unreleased"
else
    COMMITS=$(git log "$LAST_TAG"..HEAD --pretty=format:"%s")
    VERSION=$(git describe --tags --abbrev=0)-next
fi

{
    echo "## [$VERSION] - $TODAY"
    echo
} > "$TEMP_FILE"

append_commits() {
    TYPE="$1"
    HEADING="$2"

    MATCHED=$(echo "$COMMITS" | grep -E "^$TYPE(\(.+\))?: " || true)
    if [ -n "$MATCHED" ]; then
        echo "### $HEADING" >> "$TEMP_FILE"
        echo "$MATCHED" | sed -E "s/^$TYPE(\(.+\))?: (.+)/- \2/" >> "$TEMP_FILE"
        echo >> "$TEMP_FILE"
    fi
}

append_commits "feat" "Features"
append_commits "fix" "Bug Fixes"
append_commits "docs" "Documentation"
append_commits "refactor" "Refactoring"
append_commits "test" "Tests"
append_commits "chore" "Chores"
append_commits "style" "Code Style"
append_commits "perf" "Performance"

if [ -f "$CHANGELOG_FILE" ]; then
    cat "$CHANGELOG_FILE" >> "$TEMP_FILE"
fi

mv "$TEMP_FILE" "$CHANGELOG_FILE"

echo "✅ CHANGELOG.md update version: [$VERSION]"
