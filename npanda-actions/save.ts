import fs from 'node:fs/promises';
import osPath from 'node:path';
import { context, getOctokit } from '@actions/github';

const octokit = getOctokit(process.env.GITHUB_TOKEN!);

let path = process.argv[2];

const date = new Date();

const createReleaseResponse = await octokit.rest.repos.createRelease({
    owner: context.repo.owner,
    repo: context.repo.repo,
    tag_name: 'database-' + date.toISOString().replace(/:/g, '-'),
    name: `Persisting database at ${date.toUTCString()}`,
    draft: false,
    prerelease: true
});

await octokit.request<'POST {origin}/repos/{owner}/{repo}/releases/{release_id}/assets{?name,label}'>({
    method: "POST",
    url: createReleaseResponse.data.upload_url,
    headers: {
        "content-type": "application/zip",
        'X-GitHub-Api-Version': '2022-11-28'
    },
    data: await fs.readFile(path),
    name: osPath.basename(path),
    label: osPath.basename(path),
});