#!/bin/bash

set -eo pipefail
trap 'echo "❌ Error line $LINENO"; exit 1' ERR

echo "📝 Generating CHANGELOG.md..."
echo "📍 Working directory: $(pwd)"
echo "📍 Date: $(date)"
echo "📍 Files:"
ls -lA

CHANGELOG_FILE="CHANGELOG.md"
TEMP_FILE="CHANGELOG_TEMP.md"

REPO_URL="https://github.com/piqueterron/OtherMediator"

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
TODAY=$(date +"%Y-%m-%d")

if [ -z "$LAST_TAG" ]; then
    echo "ℹ️ Not found tag. Extract all commits."
    COMMITS=$(git log --pretty=format:"%h|%an|%s" || true)
    VERSION="Unreleased"
else
    COMMITS=$(git log "$LAST_TAG"..HEAD --pretty=format:"%h|%an|%s" || true)
    VERSION=$(git describe --tags --abbrev=0)-next
fi

if [ -z "$COMMITS" ]; then
    echo "⚠️ No commits found to generate changelog."
    exit 0
fi

count_type() {
    local type="$1"
    echo "$COMMITS" | grep -cE "^[^|]+\|[^|]+\|${type}(\([^)]+\))?: " || echo "0"
}

FEAT_COUNT=$(count_type "feat")
FIX_COUNT=$(count_type "fix")
DOCS_COUNT=$(count_type "docs")
REFACTOR_COUNT=$(count_type "refactor")
TEST_COUNT=$(count_type "test")
CHORE_COUNT=$(count_type "chore")
STYLE_COUNT=$(count_type "style")
PERF_COUNT=$(count_type "perf")

{
    echo "## [$VERSION] - $TODAY"
    echo
    echo "**Change Summary:**"
    echo
    echo "- 🚀 Features: $FEAT_COUNT"
    echo "- 🐛 Bug Fixes: $FIX_COUNT"
    echo "- 📚 Documentation: $DOCS_COUNT"
    echo "- 🛠️ Refactoring: $REFACTOR_COUNT"
    echo "- 🧪 Tests: $TEST_COUNT"
    echo "- 🔧 Chores: $CHORE_COUNT"
    echo "- 🎨 Code Style: $STYLE_COUNT"
    echo "- ⚡ Performance: $PERF_COUNT"
    echo
    echo "---"
    echo
} > "$TEMP_FILE"

append_commits() {
    TYPE="$1"
    HEADING="$2"
    EMOJI="$3"

    local heading_printed=false
    local commits_array=()
    mapfile -t commits_array <<< "$COMMITS"

    echo "Processing $TYPE commits..."
    for line in "${commits_array[@]}"; do
        echo "Line: $line"
        if echo "$line" | grep -qE "^\w+\|[^|]+\|$TYPE(\([^)]+\))?: .+"; then
            if [ "$heading_printed" = false ]; then
                echo "### $EMOJI $HEADING" >> "$TEMP_FILE"
                heading_printed=true
            fi

            COMMIT_HASH=$(echo "$line" | cut -d'|' -f1)
            AUTHOR=$(echo "$line" | cut -d'|' -f2)
            MESSAGE=$(echo "$line" | cut -d'|' -f3-)

            CLEAN_MESSAGE=$(echo "$MESSAGE" | sed -E "s|^$TYPE(\([^)]+\))?: ||")
            LINKED_MESSAGE=$(echo "$CLEAN_MESSAGE" | sed -E "s|#([0-9]+)|[#\1]($REPO_URL/issues/\1)|g")

            echo "- $LINKED_MESSAGE" >> "$TEMP_FILE"
            echo "  \`[$COMMIT_HASH]($REPO_URL/commit/$COMMIT_HASH)\` _por $AUTHOR_" >> "$TEMP_FILE"
            echo >> "$TEMP_FILE"
        fi
    done
}

append_commits "feat" "Features" "🚀"
append_commits "fix" "Bug Fixes" "🐛"
append_commits "docs" "Documentation" "📚"
append_commits "refactor" "Refactoring" "🛠️"
append_commits "test" "Tests" "🧪"
append_commits "chore" "Chores" "🔧"
append_commits "style" "Code Style" "🎨"
append_commits "perf" "Performance" "⚡"

BREAKING=$(git log ${LAST_TAG:+$LAST_TAG..HEAD} --pretty=format:"%b" | grep -i "BREAKING CHANGE:" || true)
if [ -n "$BREAKING" ]; then
    echo "### ⚠️ Breaking Changes" >> "$TEMP_FILE"
    echo "$BREAKING" | sed -E "s|.*BREAKING CHANGE: (.+)|- \1|" >> "$TEMP_FILE"
    echo >> "$TEMP_FILE"
fi

OTHERS=$(echo "$COMMITS" | grep -Ev "^\w+\|[^|]+\|(feat|fix|docs|refactor|test|chore|style|perf)(\([^)]+\))?: .+" || true)
if [ -n "$OTHERS" ]; then
    echo "### Other changes" >> "$TEMP_FILE"
    echo "$OTHERS" | while IFS= read -r line; do
        COMMIT_HASH=$(echo "$line" | cut -d'|' -f1)
        AUTHOR=$(echo "$line" | cut -d'|' -f2)
        MESSAGE=$(echo "$line" | cut -d'|' -f3-)
        
        echo "- $MESSAGE" >> "$TEMP_FILE"
        echo "  \`[$COMMIT_HASH]($REPO_URL/commit/$COMMIT_HASH)\` _por $AUTHOR_" >> "$TEMP_FILE"
    done
    echo >> "$TEMP_FILE"
fi

if [ -f "$CHANGELOG_FILE" ]; then
    cat "$CHANGELOG_FILE" >> "$TEMP_FILE"
fi

mv "$TEMP_FILE" "$CHANGELOG_FILE"

echo "✅ CHANGELOG.md update version: [$VERSION]"