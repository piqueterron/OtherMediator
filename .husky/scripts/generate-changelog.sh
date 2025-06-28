#!/bin/sh

set -euo pipefail
trap 'echo "❌ Error on line $LINENO"; exit 1' ERR

CHANGELOG_FILE="CHANGELOG.md"
TEMP_FILE="CHANGELOG_TEMP.md"
REPO_URL="https://github.com/piqueterron/OtherMediator"
TODAY=$(date +"%Y-%m-%d")

echo "📝 Generating CHANGELOG.md..."
echo "📍 Working directory: $(pwd)"
echo "📍 Date: $TODAY"

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "❌ Error: This script must be run inside a Git repository"
    exit 1
fi

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
if [ -z "$LAST_TAG" ]; then
    echo "ℹ️ No tags found. Using all commits."
    COMMITS=$(git log --pretty=format:"%h|%an|%s|%b")
    VERSION="Unreleased"
else
    COMMITS=$(git log "$LAST_TAG"..HEAD --pretty=format:"%h|%an|%s|%b")
    VERSION="$LAST_TAG-next"
fi

if [ -z "$COMMITS" ]; then
    echo "⚠️ No commits to include."
    exit 0
fi

declare FEATS FIXES DOCS REFACTORS TESTS CHORES STYLES PERFS BREAKINGS OTHERS
FEAT_COUNT=0; FIX_COUNT=0; DOCS_COUNT=0; REFACTOR_COUNT=0; TEST_COUNT=0; CHORE_COUNT=0; STYLE_COUNT=0; PERF_COUNT=0; BREAKING_COUNT=0

# Procesa todos los commits una sola vez
echo "$COMMITS" | awk -F'|' -v repo_url="$REPO_URL" '
function print_commit(arr, ctype, emoji, heading) {
    if (length(arr) > 0) {
        print "### " emoji " " heading "\n" >> ENVIRON["TEMP_FILE"]
        for (i = 1; i <= length(arr); i++) {
            print arr[i] >> ENVIRON["TEMP_FILE"]
        }
        print "" >> ENVIRON["TEMP_FILE"]
    }
}
{
    hash=$1; author=$2; subject=$3; body=$4
    # Quita tipo y paréntesis del subject
    match(subject, /^([a-zA-Z]+)(\([^\)]+\))?:[ ]?/, m)
    type = m[1]
    msg = subject
    gsub(/^([a-zA-Z]+)(\([^\)]+\))?:[ ]?/, "", msg)
    # Enlaza #123 a issues
    gsub(/#([0-9]+)/, "[#\\1]("repo_url"/issues/\\1)", msg)
    line="- " msg "\n  \`[" hash "]\` by " author
    # Clasifica por tipo
    if (type == "feat")      { feats[++fc]=line; FEAT_COUNT++ }
    else if (type == "fix")  { fixes[++fic]=line; FIX_COUNT++ }
    else if (type == "docs") { docs[++dc]=line; DOCS_COUNT++ }
    else if (type == "refactor") { refactors[++rc]=line; REFACTOR_COUNT++ }
    else if (type == "test") { tests[++tc]=line; TEST_COUNT++ }
    else if (type == "chore") { chores[++cc]=line; CHORE_COUNT++ }
    else if (type == "style") { styles[++sc]=line; STYLE_COUNT++ }
    else if (type == "perf")  { perfs[++pc]=line; PERF_COUNT++ }
    else                      { others[++oc]=line }
    # Breaking changes (en body)
    if (tolower(body) ~ /breaking change:/) {
        match(body, /[Bb][Rr][Ee][Aa][Kk][Ii][Nn][Gg] [Cc][Hh][Aa][Nn][Gg][Ee]:[ ]*(.*)/, b)
        if (b[1] != "") breakings[++bc]="- " b[1]
    }
}
END {
    # Output resumen
    print "## [" ENVIRON["VERSION"] "] - " ENVIRON["TODAY"] "\n" > ENVIRON["TEMP_FILE"]
    print "**Change Summary:**\n" >> ENVIRON["TEMP_FILE"]
    print "- 🚀 Features: " FEAT_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 🐛 Bug Fixes: " FIX_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 📚 Documentation: " DOCS_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 🛠️ Refactoring: " REFACTOR_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 🧪 Tests: " TEST_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 🔧 Chores: " CHORE_COUNT >> ENVIRON["TEMP_FILE"]
    print "- 🎨 Code Style: " STYLE_COUNT >> ENVIRON["TEMP_FILE"]
    print "- ⚡ Performance: " PERF_COUNT "\n" >> ENVIRON["TEMP_FILE"]
    print "---\n" >> ENVIRON["TEMP_FILE"]
    # Por tipo
    print_commit(feats, "feat", "🚀", "Features")
    print_commit(fixes, "fix", "🐛", "Bug Fixes")
    print_commit(docs, "docs", "📚", "Documentation")
    print_commit(refactors, "refactor", "🛠️", "Refactoring")
    print_commit(tests, "test", "🧪", "Tests")
    print_commit(chores, "chore", "🔧", "Chores")
    print_commit(styles, "style", "🎨", "Code Style")
    print_commit(perfs, "perf", "⚡", "Performance")
    # Breaking changes
    if (length(breakings) > 0) {
        print "### ⚠️ Breaking Changes\n" >> ENVIRON["TEMP_FILE"]
        for (i = 1; i <= length(breakings); i++) {
            print breakings[i] >> ENVIRON["TEMP_FILE"]
        }
        print "" >> ENVIRON["TEMP_FILE"]
    }
    # Otros
    if (length(others) > 0) {
        print "### Other Changes\n" >> ENVIRON["TEMP_FILE"]
        for (i = 1; i <= length(others); i++) {
            print others[i] >> ENVIRON["TEMP_FILE"]
        }
        print "" >> ENVIRON["TEMP_FILE"]
    }
}
'

if [ -f "$CHANGELOG_FILE" ]; then
    if ! grep -q "## \[$VERSION\] - $TODAY" "$CHANGELOG_FILE"; then
        echo "🔗 Appending to existing CHANGELOG..."
        { echo ""; cat "$CHANGELOG_FILE"; } >> "$TEMP_FILE"
    else
        echo "ℹ️ Version $VERSION already exists in CHANGELOG.md, skipping duplicate."
    fi
fi

mv "$TEMP_FILE" "$CHANGELOG_FILE"
echo "✅ CHANGELOG.md successfully updated for version: [$VERSION]"