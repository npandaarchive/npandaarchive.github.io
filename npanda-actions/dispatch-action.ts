import fs from 'node:fs/promises';
import osPath from 'node:path';
import { context, getOctokit } from '@actions/github';

const octokit = getOctokit(process.env.GITHUB_TOKEN!);

await octokit.rest.actions.createWorkflowDispatch({
    owner: context.repo.owner,
    repo: context.repo.repo,
    workflow_id: process.argv[2],
    ref: 'master',
    headers: {
      'X-GitHub-Api-Version': '2022-11-28'
    }
})