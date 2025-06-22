#!/bin/sh

set -eo pipefail
trap 'echo "❌ Error on line $LINENO"; exit 1' ERR

CHANGELOG_FILE="CHANGELOG.md"
TEMP_FILE="CHANGELOG_TEMP.md"
REPO_URL="https://github.com/piqueterron/OtherMediator"
TODAY=$(date +"%Y-%m-%d")

echo "📝 Generating CHANGELOG.md..."
echo "📍 Working directory: $(pwd)"
echo "📍 Date: $TODAY"
echo "📍 Files in directory:"
ls -lA

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "❌ Error: This script must be run inside a Git repository"
    exit 1
fi

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
if [ -z "$LAST_TAG" ]; then
    echo "ℹ️ No tags found in repository. Using all commits."
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
    type="$1"
    echo "$COMMITS" | grep -cE "^[^|]+\|[^|]+\|${type}(\([^)]+\))?: " || echo ""
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
    heading_printed=false

    if [ "$(count_type "$TYPE")" -eq 0 ]; then
        return
    fi

    echo "$COMMITS" | while IFS= read -r line; do
        if echo "$line" | grep -qE "^[^|]+\|[^|]+\|${TYPE}(\([^)]+\))?: "; then
            if [ "$heading_printed" = false ]; then
                echo "### $EMOJI $HEADING" >> "$TEMP_FILE"
                heading_printed=true
            fi

            commit_hash=$(echo "$line" | cut -d'|' -f1)
            author=$(echo "$line" | cut -d'|' -f2)
            message=$(echo "$line" | cut -d'|' -f3- | sed -E "s/^${TYPE}(\([^)]+\))?: //")
            
            linked_message=$(echo "$message" | sed -E "s|#([0-9]+)|[#\1]($REPO_URL/issues/\1)|g")

            echo "- $linked_message" >> "$TEMP_FILE"
            echo "  \`[$commit_hash]\` by $author" >> "$TEMP_FILE"
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

OTHERS=$(echo "$COMMITS" | grep -Ev "^[^|]+\|[^|]+\|(feat|fix|docs|refactor|test|chore|style|perf)(\([^)]+\))?: " || true)
if [ -n "$OTHERS" ]; then
    echo "### Other Changes" >> "$TEMP_FILE"
    echo "$OTHERS" | while IFS= read -r line; do
        commit_hash=$(echo "$line" | cut -d'|' -f1)
        author=$(echo "$line" | cut -d'|' -f2)
        message=$(echo "$line" | cut -d'|' -f3-)
        
        echo "- $message" >> "$TEMP_FILE"
        echo "  \`[$commit_hash]\` by $author" >> "$TEMP_FILE"
    done
    echo >> "$TEMP_FILE"
fi

if [ -f "$CHANGELOG_FILE" ]; then
    if ! grep -q "## \[$VERSION\] - $TODAY" "$CHANGELOG_FILE"; then
        echo "🔗 Appending to existing CHANGELOG..."
        {
            echo ""
            cat "$CHANGELOG_FILE"
        } >> "$TEMP_FILE"
    else
        echo "ℹ️ Version $VERSION already exists in CHANGELOG.md, skipping duplicate."
    fi
fi

mv "$TEMP_FILE" "$CHANGELOG_FILE"
echo "✅ CHANGELOG.md successfully updated for version: [$VERSION]"