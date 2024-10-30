import fs from 'node:fs/promises';
import osPath from 'node:path';
import { context, getOctokit } from '@actions/github';

const octokit = getOctokit(process.env.GITHUB_TOKEN!);

let path = process.argv[2];

let getReleaseResponse;
try {
    getReleaseResponse = await octokit.rest.repos.listReleases({
        per_page: 2,
        // page: 1,
        owner: context.repo.owner,
        repo: context.repo.repo,
    });
} catch (err) {
    console.error(`getReleaseResponse ${err}`);
    process.exit(0);
}

if (getReleaseResponse.status != 200) {
    console.log(`getReleaseResponse status ${getReleaseResponse.status}`);
    process.exit(0);
}

const release = getReleaseResponse.data.find(e => e.assets.find(e => e.name == osPath.basename(path))?.browser_download_url)!;
const downloadUrl = release.assets.find(e => e.name == osPath.basename(path))?.browser_download_url;
if (!downloadUrl) {
    console.log(`no downloadUrl for ${release.name}`);
    process.exit(0);
}

const buf = await fetch(downloadUrl).then(e => e.ok ? e.arrayBuffer() : undefined);
if (buf) {
    await fs.writeFile(path, Buffer.from(buf));
    console.log(`downloaded database from ${release.name}}`);
} else {
    console.log('not ok');
}