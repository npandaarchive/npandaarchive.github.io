import fs from 'node:fs/promises';
import osPath from 'node:path';
import { context, getOctokit } from '@actions/github';

const octokit = getOctokit(process.env.GITHUB_TOKEN!);

let path = process.argv[2];

let getReleaseResponse;
try {
    getReleaseResponse = await octokit.rest.repos.listReleases({
        per_page: 1,
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

const downloadUrl = getReleaseResponse.data[0].assets.find(e => e.name == osPath.basename(path))?.browser_download_url
if (!downloadUrl) {
    console.log(`no downloadUrl for ${getReleaseResponse.data[0].name}`);
    process.exit(0);
}

const buf = await fetch(downloadUrl).then(e => e.ok ? e.arrayBuffer() : undefined);
if (buf) {
    await fs.writeFile(path, Buffer.from(buf));
    console.log(`downloaded database from ${getReleaseResponse.data[0].name}}`);
} else {
    console.log('not ok');
}