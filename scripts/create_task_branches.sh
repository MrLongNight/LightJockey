#!/bin/bash
# Script to create branches for Tasks 13-21
# This script will create feature branches for all uncompleted tasks

set -e

echo "Creating branches for Tasks 13-21..."

# Get current branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "Current branch: $CURRENT_BRANCH"

# Switch to main to create branches from there
echo "Switching to main branch..."
git fetch origin main
git checkout main
git pull origin main

# Array of tasks to create
declare -a TASKS=(
  "13:Preset-Sharing-Cloud:Task13_PresetImportExport"
  "14:Security-Verschluesselung:Task14_SecureKeys"
  "15:Advanced-Logging-Metrics:Task15_AdvancedLoggingMetrics"
  "16:Theme-Enhancements:Task16_ThemeEnhancements"
  "17:Hue-Bridge-MultiSupport:Task17_Hue-Bridge-MultiSupport"
  "18:WebSocketAPI:Task18_WebSocketAPI"
  "19:2D-LightMappingManager:Task19_2D-LightMappingManager"
  "20:CICD-StoreDeployment:Task20_CICD_StoreDeployment"
  "21:QA-Documentation:Task21_QA_Documentation"
)

# Create branches
for task in "${TASKS[@]}"; do
  IFS=':' read -r task_num task_name pr_name <<< "$task"
  
  BRANCH_NAME="feature/task-${task_num}-${task_name}"
  
  echo ""
  echo "Creating branch: $BRANCH_NAME"
  
  # Check if branch already exists locally
  if git show-ref --verify --quiet "refs/heads/$BRANCH_NAME"; then
    echo "  Branch $BRANCH_NAME already exists locally, skipping..."
    continue
  fi
  
  # Check if branch exists remotely
  if git ls-remote --heads origin "$BRANCH_NAME" | grep -q "$BRANCH_NAME"; then
    echo "  Branch $BRANCH_NAME already exists remotely, skipping..."
    continue
  fi
  
  # Create and push branch
  git checkout -b "$BRANCH_NAME"
  git push -u origin "$BRANCH_NAME"
  echo "  ✓ Created and pushed $BRANCH_NAME"
  
  # Go back to main for next iteration
  git checkout main
done

echo ""
echo "✓ All branches created successfully!"
echo ""
echo "Next steps:"
echo "1. The automation workflow should detect these branches and create PRs"
echo "2. Or manually create PRs for each branch using the GitHub UI"
echo "3. Or run the create_task_issues.py script to create issues via GitHub API"
echo ""
echo "Switching back to original branch: $CURRENT_BRANCH"
git checkout "$CURRENT_BRANCH"
