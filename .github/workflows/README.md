# GitHub Actions Workflows

This directory contains automated CI/CD workflows for the LightJockey project.

⚠️ **Important Note on Automation**: The workflow creates issues/PRs automatically but **GitHub Copilot Workspace must be manually activated** to implement tasks. See [AUTO_TASK_TROUBLESHOOTING.md](../docs/AUTO_TASK_TROUBLESHOOTING.md) for details.

## Workflow Organization Structure

All workflows are organized using GitHub's **reusable workflows** pattern for better maintainability:

- **Caller workflows**: Small workflow files that define triggers and call reusable workflows
- **Reusable workflows**: Prefixed with `TASK-A.00_Reusable-*`, these contain the actual workflow logic that can be called from multiple places

This structure provides:
- ✅ Better organization and separation of concerns
- ✅ Easier maintenance (logic in one place)
- ✅ Workflows visible in GitHub Actions UI
- ✅ Code reusability across different triggers

## 🆕 Jules API Workflows (RECOMMENDED)

The new **Jules API integration** provides near-complete automation (~98%) by leveraging Google Jules API to process tasks programmatically. See [JULES_API_INTEGRATION.md](../../docs/JULES_API_INTEGRATION.md) for complete documentation.

**Structure**: Each Jules workflow consists of a caller workflow (e.g., `TASK-A.01_Start-Task_on-Issue-Label_AUTO.yml`) that calls a reusable workflow (e.g., `TASK-A.00_Reusable-Start-Task_on-Workflow-Call_AUTO.yml`).

### TASK-A.01: Start Task on Issue Label
**Caller File**: `TASK-A.01_Start-Task_on-Issue-Label_AUTO.yml`  
**Reusable Workflow**: `TASK-A.00_Reusable-Start-Task_on-Workflow-Call_AUTO.yml`

**Purpose**: Automatically submits tasks to Jules AI agent via API.

**Triggers**:
- Automatic on push to `main` branch
- Manual workflow dispatch

**What it does**:
- Finds next uncompleted task from development plan
- Creates Jules session via API with detailed prompt
- Creates GitHub tracking issue
- No manual activation required!

**Configuration**: Requires `JULES_AUTOMATION_ENABLED` variable and `JulesAPIKey` secret.

### TASK-A.02: Monitor PR on PR Open
**Caller File**: `TASK-A.02_Monitor-PR_on-PR-Open_AUTO.yml`  
**Reusable Workflow**: `TASK-A.00_Reusable-Monitor-PR_on-Workflow-Call_AUTO.yml`

**Purpose**: Monitors Jules sessions and triggers Copilot review when PR is created.

**Triggers**:
- Scheduled (every 15 minutes via cron)
- Manual workflow dispatch

**What it does**:
- Monitors active Jules sessions via API
- Detects when Jules creates a PR
- Updates tracking issue with PR information
- Triggers Copilot Agent for code review
- Adds appropriate labels

**Configuration**: Requires `JULES_AUTOMATION_ENABLED` variable and `JulesAPIKey` secret.

### TASK-A.03: Merge PR on PR Review
**Caller File**: `TASK-A.03_Merge-PR_on-PR-Review_AUTO.yml`  
**Reusable Workflow**: `TASK-A.00_Reusable-Merge-PR_on-Workflow-Call_AUTO.yml`

**Purpose**: Automatically merges PRs after Copilot review and successful CI tests.

**Triggers**:
- Automatic on pull request review submitted
- Automatic on check suite completed
- Manual workflow dispatch

**What it does**:
- Verifies all CI checks passed
- Verifies Copilot review completed
- Converts draft PR to ready if needed
- Merges PR automatically
- Closes tracking issue
- Cycle continues with next task

**Configuration**: Requires `JULES_AUTOMATION_ENABLED` variable and `JulesAPIKey` secret.

## 🆕 User Issue Workflows (NEW)

These workflows handle bug reports and feature requests submitted by users through GitHub issue templates. See [USER_ISSUE_AUTOMATION.md](../../docs/USER_ISSUE_AUTOMATION.md) for complete documentation.

### USER-ISSUE.01: Process Bug Report
**File**: `USER-ISSUE.01_Process-Bug-Report_AUTO.yml`

**Purpose**: Automatically processes bug reports submitted by users - fully automated.

**Triggers**:
- Automatic when issue with label `jules-auto-process` is opened

**What it does**:
- Creates Jules session immediately (no approval needed)
- Jules analyzes and fixes the bug
- Creates pull request automatically
- Runs tests and auto-merges if successful
- Notifies user of progress

**User Experience**: Bug is automatically fixed within minutes to hours.

### USER-ISSUE.02: Process Feature Request
**File**: `USER-ISSUE.02_Process-Feature-Request_AUTO.yml`

**Purpose**: Processes feature requests with manual approval step.

**Triggers**:
- Automatic when issue with label `needs-approval` is opened (requests approval)
- Automatic when label `approved-for-jules` is added (starts Jules)

**What it does**:
- Assigns issue to repository owner for review
- Waits for maintainer approval
- When approved: Creates Jules session
- Jules implements the feature
- Creates pull request automatically
- Runs tests and auto-merges if successful
- Notifies user at each step

**User Experience**: 
- User submits feature request
- Waits for approval (1-7 days)
- Feature is automatically implemented after approval

### USER-ISSUE.03: Notify Test Failures
**File**: `USER-ISSUE.03_Notify-Test-Failures_AUTO.yml`

**Purpose**: Notifies users when unit tests fail on their issues, requiring manual review.

**Triggers**:
- Automatic when CI workflow fails

**What it does**:
- Detects failed tests on user-submitted issue PRs
- Identifies the related original issue
- Notifies user about test failures
- Explains that manual review is needed
- Adds labels: `tests-failed`, `needs-manual-review`
- Mentions maintainer for manual intervention

**User Experience**: User is kept informed when delays occur due to test failures.

**Configuration**: Works automatically with existing CI workflow.

## Workflow Structure

### Auto-Task Workflows

#### TASK-B.01: Start Task on Manual
**File**: `TASK-B.01_Start-Task_on-Manual_MAN.yml`

**Purpose**: Automatically creates issues and PRs for uncompleted tasks (legacy workflow for GitHub Copilot Workspace).

**Triggers**:
- Automatic on push to `main` branch
- Manual workflow dispatch

**What it does**:
- Parses development plan for next uncompleted task
- Creates feature branch
- Creates GitHub issue with task details
- Creates draft pull request
- Links issue to PR

**Configuration**: Requires `AUTOMATION_ENABLED` repository variable set to `true`.

**Note**: This is the older workflow method requiring manual Copilot Workspace activation. Consider using TASK-A workflows for better automation.

#### TASK-B.02: Notify Agent on PR Open
**File**: `TASK-B.02_Notify-Agent_on-PR-Open_AUTO.yml`

**Purpose**: Adds instructions to auto-generated PRs for Copilot activation.

**Triggers**:
- Automatic when PR is opened

**What it does**:
- Comments on PR with activation instructions
- Updates PR description
- Assigns PR to repository owner

**Configuration**: Works with PRs that have the `autogenerated` label.

#### TASK-B.03: Merge PR on PR Label
**File**: `TASK-B.03_Merge-PR_on-PR-Label_AUTO.yml`

**Purpose**: Automatically merges PRs after successful CI (legacy workflow for GitHub Copilot Workspace).

**Triggers**:
- Automatic after CI workflow (`CI.01_Build-Test_on-Push-PR_AUTO`) completes

**What it does**:
- Checks if automation is enabled
- Converts draft PR to ready for review
- Adds automerge label
- Merges PR if tests passed

**Configuration**: Requires `AUTOMATION_ENABLED` repository variable set to `true`.

### Agent Workflow

#### AGENT.01: Assign Issue on Issue Event
**File**: `AGENT.01_Assign-Issue_on-Issue-Event_AUTO.yml`

**Purpose**: Automatically assigns GitHub Copilot Coding Agent to auto-generated issues.

**Triggers**:
- Automatic when issue is opened
- Automatic when issue is labeled

**What it does**:
- Checks if issue has both `autogenerated` and `copilot-task` labels
- Adds comment with Copilot agent activation instructions
- Provides guidance on how to activate the Copilot Coding Agent

**Configuration**: Requires `AUTOMATION_ENABLED` repository variable set to `true`.

**Note**: Based on GitHub Copilot's agentic workflows pattern.

### Continuous Integration

### 1. Build & Test (Automatic)
**File**: `CI.01_Build-Test_on-Push-PR_AUTO.yml`  
**Status**: [![Build & Test](https://github.com/MrLongNight/LightJockey/actions/workflows/CI.01_Build-Test_on-Push-PR_AUTO.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/CI.01_Build-Test_on-Push-PR_AUTO.yml)

**Purpose**: Run unit tests automatically and upload coverage to Codecov.

**Triggers**:
- ✅ Automatic on push to `main` or `develop`
- ✅ Automatic on pull requests to `main` or `develop`

**What it does**:
- Builds the solution
- Runs all unit tests (179+ tests)
- Collects code coverage
- Uploads coverage to Codecov

### 2. Release MSI Package (Manual)
**File**: `RELEASE.01_Create-MSI_on-Push_AUTO.yml`  
**Status**: [![Release MSI](https://github.com/MrLongNight/LightJockey/actions/workflows/RELEASE.01_Create-MSI_on-Push_AUTO.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/RELEASE.01_Create-MSI_on-Push_AUTO.yml)

**Purpose**: Create MSI installer package for distribution.

**Triggers**:
- 🔧 Manual workflow dispatch (with optional version input)
- ✅ Automatic on version tags (e.g., `v1.0.0`)

**What it does**:
- Builds the solution
- Runs tests to ensure quality
- Publishes the application
- Creates MSI installer using WiX Toolset v4
- Uploads MSI artifact (90-day retention)
- Creates GitHub Release (when triggered by tag)

**Note**: This is the main workflow for creating MSI releases. Use this workflow when you need to build and publish installer packages.

### 3. Release MSI Package (Manual - Pre-Release)
**File**: `RELEASE.01_Create-MSI_on-Push_MAN.yml`  
**Status**: [![Release MSI Manual](https://github.com/MrLongNight/LightJockey/actions/workflows/RELEASE.01_Create-MSI_on-Push_MAN.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/RELEASE.01_Create-MSI_on-Push_MAN.yml)

**Purpose**: Create MSI installer package for pre-release testing.

**Triggers**:
- 🔧 Manual workflow dispatch only (with optional version input)

**What it does**:
- Builds the solution
- Runs tests to ensure quality
- Publishes the application
- Creates MSI installer using WiX Toolset v4
- Uploads MSI artifact (90-day retention)
- Creates GitHub Pre-Release with tag `v{version}-pre`

**Note**: This workflow is specifically for creating pre-release builds for testing purposes. The release is always marked as a pre-release and uses a different tag naming convention.

## Usage

### Running Tests (Automatic)
Tests run automatically on every push and PR. No action needed.

### Building (Automatic)
Builds run automatically on every push and PR. No action needed.

### Creating a Release Package (Manual)

**Option 1: Manual trigger with custom version**
1. Go to **Actions** → **RELEASE.01: Create MSI on Push**
2. Click **Run workflow**
3. Enter version number (e.g., `1.0.0`) or leave default
4. Click **Run workflow**

**Option 2: Tag-based release**
```bash
git tag v1.0.0
git push origin v1.0.0
```
This will:
- Trigger MSI package creation
- Create a GitHub Release
- Attach the MSI installer to the release

### Creating a Pre-Release Package (Manual)

For testing purposes, you can create a pre-release build:

1. Go to **Actions** → **RELEASE.01_Create-MSI_on-Push_MAN**
2. Click **Run workflow**
3. Enter version number (e.g., `1.0.0-beta`) or leave default
4. Click **Run workflow**

This will:
- Build and test the MSI package
- Create a GitHub Pre-Release with tag `v{version}-pre`
- Mark the release as pre-release for testing

## Workflow Requirements

### Secrets
- `CODECOV_TOKEN` - For code coverage uploads (optional but recommended)
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions

### CI Environment
- Windows runner (windows-latest)
- .NET 9.0 SDK
- WiX Toolset v4 (installed automatically)

## Local Development

```powershell
# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release

# Publish (for MSI packaging)
dotnet publish src/LightJockey/LightJockey.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output publish

# Install WiX (one-time)
dotnet tool install --global wix

# Build MSI (requires WiX source file)
# See docs/tasks/Task10_CICD_MSI.md for details
```

## Troubleshooting

### Build Failures
- Verify .NET 9.0 SDK is installed
- Check NuGet package availability
- Review build logs in Actions tab

### Test Failures
- Tests require Windows environment
- Check test logs for specific failures

### MSI Package Failures
- Ensure WiX Toolset installation succeeded
- Verify publish directory contains all required files
- Check WiX source file (.wxs) syntax

## Documentation

For detailed information:
- [Task 10: CI/CD & MSI Packaging](../../docs/tasks/Task10_CICD_MSI.md)
- [User Issue Automation](../../docs/USER_ISSUE_AUTOMATION.md)

## Summary

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| **CI/CD** |
| CI.01_Build-Test_on-Push-PR_AUTO | Automatic (push/PR) | Run tests & coverage |
| RELEASE.01_Create-MSI_on-Push_AUTO | Manual / Tags | Create installer package |
| RELEASE.01_Create-MSI_on-Push_MAN | Manual only | Create pre-release installer |
| **User Issue Workflows (NEW)** |
| USER-ISSUE.01_Process-Bug-Report_AUTO | Issue opened (bug) | Auto-process bug reports |
| USER-ISSUE.02_Process-Feature-Request_AUTO | Issue opened/labeled (feature) | Process feature requests with approval |
| USER-ISSUE.03_Notify-Test-Failures_AUTO | CI failure | Notify users of test failures |
| **Jules API Workflows (Recommended)** |
| TASK-A.00_Reusable-Start-Task_on-Workflow-Call_AUTO | Workflow call | Reusable: Start Jules task |
| TASK-A.00_Reusable-Monitor-PR_on-Workflow-Call_AUTO | Workflow call | Reusable: Monitor Jules PR |
| TASK-A.00_Reusable-Merge-PR_on-Workflow-Call_AUTO | Workflow call | Reusable: Merge Jules PR |
| TASK-A.01_Start-Task_on-Issue-Label_AUTO | Push to main / Manual | Submit task to Jules API |
| TASK-A.02_Monitor-PR_on-PR-Open_AUTO | Scheduled (15 min) / Manual | Monitor Jules sessions |
| TASK-A.03_Merge-PR_on-PR-Review_AUTO | PR review / Check suite | Auto-merge after review |
| **Legacy Copilot Workspace Workflows** |
| TASK-B.01_Start-Task_on-Manual_MAN | Push to main / Manual | Create issues & PRs |
| TASK-B.02_Notify-Agent_on-PR-Open_AUTO | PR opened | Add Copilot instructions |
| TASK-B.03_Merge-PR_on-PR-Label_AUTO | After CI completes | Auto-merge PRs |
| **Agent Workflows** |
| AGENT.01_Assign-Issue_on-Issue-Event_AUTO | Issue opened/labeled | Assign Copilot agent |

This structure provides:
- ✅ Automatic quality checks on every change
- ✅ Manual control over release packaging
- ✅ Clear separation of concerns
- ✅ Multiple automation paths (Jules API or Copilot Workspace)
- ✅ User-friendly issue templates with automated processing
- ✅ Approval workflow for feature requests
- ✅ Automatic notifications on test failures
