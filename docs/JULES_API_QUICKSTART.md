# Jules API Workflows - Quick Start Guide

This guide will help you set up and use the Jules API automation workflows for the LightJockey project.

## ⚡ Quick Setup (5 Minutes)

### Step 1: Install Jules GitHub App

1. Visit [jules.google.com](https://jules.google.com)
2. Sign in with your Google account
3. Click "Install GitHub App"
4. Select your repository: `MrLongNight/LightJockey`
5. Authorize the app

### Step 2: Get Jules API Key

1. Go to [jules.google.com/settings#api](https://jules.google.com/settings#api)
2. Click "Create API Key"
3. Copy the generated key (keep it safe!)

### Step 3: Add API Key to GitHub

1. Go to your repository on GitHub
2. Navigate to `Settings` → `Secrets and variables` → `Actions`
3. Click "New repository secret"
4. Name: `JULES_API_KEY`
5. Value: Paste your Jules API key
6. Click "Add secret"

### Step 4: Enable Jules Automation

1. In the same page, go to "Variables" tab
2. Click "New repository variable"
3. Name: `JULES_AUTOMATION_ENABLED`
4. Value: `true`
5. Click "Add variable"

### Step 5: Test the Workflow

```bash
# Trigger the workflow manually
gh workflow run flow-jules_01-submit-task.yml

# Or via GitHub web interface:
# Actions → Flow Jules 01: Submit Task to Jules API → Run workflow
```

## ✅ That's It!

The automation is now active and will:

1. 🤖 **Automatically** find next task after each merge
2. 🚀 **Submit** task to Jules via API
3. 👀 **Monitor** Jules session every 15 minutes
4. 📝 **Review** PR with Copilot Agent when Jules is done
5. ✅ **Merge** PR after tests pass
6. 🔄 **Repeat** for next task

## 📊 Monitoring

### Check Jules Session Status

```bash
# Using the helper script
export JULES_API_KEY="your-api-key"
./scripts/jules_api_helper.py list-sessions
```

### Check GitHub Tracking Issues

All Jules tasks have tracking issues with label `jules-task`. You can filter issues:

```bash
gh issue list --label jules-task
```

### Check Workflow Runs

```bash
gh run list --workflow=flow-jules_01-submit-task.yml
```

## 🎯 Manual Operations

### Submit Specific Task

Edit the development plan to mark tasks as incomplete, then:

```bash
gh workflow run flow-jules_01-submit-task.yml
```

### Monitor Specific Session

```bash
./scripts/jules_api_helper.py monitor SESSION_ID
```

### Force PR Review

```bash
gh workflow run flow-jules_02-monitor-and-review.yml
```

### Force PR Merge

```bash
gh workflow run flow-jules_03-auto-merge.yml -f pr_number=123
```

## 🔍 Troubleshooting

### "Repository not found in Jules sources"

**Fix**: Install Jules GitHub App for your repository at [jules.google.com](https://jules.google.com)

### "JULES_API_KEY secret is not set"

**Fix**: Add your API key as a repository secret (see Step 3 above)

### "Jules automation is disabled"

**Fix**: Set `JULES_AUTOMATION_ENABLED` variable to `true` (see Step 4 above)

### Session Stuck or Not Progressing

1. Check the session in Jules web UI: `https://jules.google.com/sessions/SESSION_ID`
2. View activities: `./scripts/jules_api_helper.py list-activities SESSION_ID`
3. Send message to Jules if needed
4. Fall back to manual implementation if Jules is blocked

## 📚 More Information

- **Complete Guide**: [JULES_API_INTEGRATION.md](JULES_API_INTEGRATION.md)
- **Workflow Reference**: [.github/workflows/README.md](../.github/workflows/README.md)
- **Jules API Docs**: [developers.google.com/jules/api](https://developers.google.com/jules/api)
- **Development Plan**: [LIGHTJOCKEY_Entwicklungsplan.md](../LIGHTJOCKEY_Entwicklungsplan.md)

## 🎉 Success Metrics

With Jules API automation active, you should see:

- ⏱️ **Time per Task**: 15-30 minutes (down from 2-4 hours)
- 🤝 **Manual Effort**: ~2% (down from ~35%)
- 🎯 **Automation**: ~98% (up from ~65%)
- ✨ **Quality**: Very high (AI + Reviews + CI)

## 🔄 Workflow Comparison

| Feature | Copilot Agent | Jules API |
|---------|--------------|-----------|
| Activation | Manual click | Fully automatic |
| Monitoring | GitHub UI | API + Workflows |
| Review | Integrated | Copilot reviews Jules PR |
| Automation | ~95% | ~98% |

## 🚀 Next Steps

1. ✅ Complete setup above
2. 🧪 Test with a small task
3. 📊 Monitor first run
4. 🎯 Enable for all tasks
5. 🎉 Enjoy automated development!

---

**Need Help?** Open an issue or check the complete documentation in [JULES_API_INTEGRATION.md](JULES_API_INTEGRATION.md)
