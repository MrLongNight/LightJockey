import { JulesTaskOrchestrator, JulesConfig, JulesTaskParams } from '@clduab11/gemini-flow';
import { Octokit } from '@octokit/rest';

// This script is intended to be run from a GitHub Action
// It requires the following environment variables to be set:
// - GITHUB_TOKEN: A token with permissions to read workflow logs and write to PRs.
// - GEMINI_API_KEY: Your Gemini API key.
// - GITHUB_REPOSITORY: The owner and repository name (e.g., "my-org/my-repo").
// - FAILED_WORKFLOW_RUN_ID: The ID of the workflow run that failed.

async function main() {
  const {
    GITHUB_TOKEN,
    GEMINI_API_KEY,
    GITHUB_REPOSITORY,
    FAILED_WORKFLOW_RUN_ID,
  } = process.env;

  if (!GITHUB_TOKEN || !GEMINI_API_KEY || !GITHUB_REPOSITORY || !FAILED_WORKFLOW_RUN_ID) {
    console.error("Missing required environment variables.");
    process.exit(1);
  }

  const [owner, repo] = GITHUB_REPOSITORY.split('/');
  const runId = parseInt(FAILED_WORKFLOW_RUN_ID, 10);

  const octokit = new Octokit({ auth: GITHUB_TOKEN });

  // 1. Get failed workflow logs
  console.log(`Fetching logs for workflow run ${runId}...`);
  const jobs = await octokit.actions.listJobsForWorkflowRun({
    owner,
    repo,
    run_id: runId,
  });

  const failedJob = jobs.data.jobs.find(job => job.conclusion === 'failure');
  if (!failedJob) {
    console.log("No failed job found in the workflow run. Exiting.");
    return;
  }

  const logResponse = await octokit.actions.downloadJobLogsForWorkflowRun({
    owner,
    repo,
    job_id: failedJob.id,
  });

  const logs = await streamToString(logResponse.data as any);

  // 2. Find the associated Pull Request
  const workflowRun = await octokit.actions.getWorkflowRun({
      owner,
      repo,
      run_id: runId,
  });
  const branchName = workflowRun.data.head_branch;
  if (!branchName) {
      console.error("Could not determine branch name from workflow run.");
      process.exit(1);
  }

  const prs = await octokit.pulls.list({
      owner,
      repo,
      head: `${owner}:${branchName}`,
      state: 'open',
  });

  const pr = prs.data[0];
  if (!pr) {
      console.error(`No open PR found for branch ${branchName}.`);
      process.exit(1);
  }

  console.log(`Found PR #${pr.number} for branch ${branchName}.`);

  // 3. Prepare and orchestrate the self-healing task
  const julesConfig: JulesConfig = {
    apiKey: GEMINI_API_KEY,
    githubToken: GITHUB_TOKEN,
    githubRepository: GITHUB_REPOSITORY,
  };

  const orchestrator = new JulesTaskOrchestrator(julesConfig, {
    enableConsensus: true, // Use multiple agents for a better chance of success
    consensusThreshold: 0.51,
  });

  await orchestrator.initialize();

  const taskParams: JulesTaskParams = {
    title: `[AUTO-FIX] Attempt to fix failed tests for PR #${pr.number}`,
    description: `The CI workflow failed. This is an automated attempt to fix the issue.

**Failed Job:** ${failedJob.name}
**PR:** ${pr.html_url}

**Error Logs:**
\`\`\`
${logs.substring(0, 5000)}...
\`\`\`
    `,
    type: 'bug',
    priority: 'high',
    branch: branchName,
    baseBranch: pr.base.ref,
    files: [], // Let Jules determine the files to change based on the logs
  };

  console.log("Dispatching Jules to fix the failed tests...");
  const result = await orchestrator.orchestrateTask(taskParams);

  // 4. Monitor the task
  console.log("Monitoring task progress...");
  const finalTask = await orchestrator.monitorTask(result.task.id, (status) => {
    console.log(`  -> Task status update: ${status}`);
  });

  if (finalTask.status === 'completed' && finalTask.result?.pullRequest) {
    console.log(`Jules has updated the PR with a potential fix: ${finalTask.result.pullRequest.url}`);
    // The updated PR will automatically trigger a new CI run.
  } else {
    console.error("Jules was unable to complete the task.", finalTask);
    // Here you could add a notification step to alert the user for manual intervention
    process.exit(1);
  }
}

async function streamToString(stream: any): Promise<string> {
    const chunks: Buffer[] = [];
    return new Promise((resolve, reject) => {
        stream.on('data', (chunk: Buffer) => chunks.push(Buffer.from(chunk)));
        stream.on('error', (err: Error) => reject(err));
        stream.on('end', () => resolve(Buffer.concat(chunks).toString('utf8')));
    });
}

main().catch(console.error);
