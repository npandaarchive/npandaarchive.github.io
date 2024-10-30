<script lang="ts">
    import type { Api } from '$lib/worker/worker';
    import { wrap } from 'comlink';
    import { Badge, Button, Col, Container, Input, InputGroup, InputGroupText, Label, Row, Spinner, Card } from "@sveltestrap/sveltestrap";
    import type { IGDataReq, IGDataRes, IGError, IGMetadata } from '$lib/e-hentai-types';
    import dayjs from 'dayjs';
    import relativeTime from 'dayjs/plugin/relativeTime';
    import { base } from '$app/paths';
    import type { Book } from '$lib/memorypack/models/Book';
    import { ImageType } from '$lib/memorypack/models/ImageType';

    const worker = wrap<Api>(
        new Worker(new URL("$lib/worker/worker.ts", import.meta.url), {
            type: "module",
        }),
    ) as Api;

    // const errored = worker.getProto(`${base}/data/errored.mpack.zst`, 'ErroredGalleries');
    const mappings = worker.getMPack(`${base}/data/mappings.mpack.zst`, 'NHentaiMapping');
    const unmatched = worker.getMPack(`${base}/data/unmatched.mpack.zst`, 'NaGalleries');

    function tokenToHex(token: bigint): string {
        return token.toString(16).padStart(10, '0');
    }

    interface MatchResult {
        status: 'panda' | 'no-panda' | 'errored' | 'invalid-gallery' | 'not-started' | 'waiting',
        nhentaiGallery?: Book,
        nhentaiError?: string,
        sadpandaId?: [gid: number, token: bigint];
        sadpandaGallery?: IGMetadata | IGError,
    }

    async function findMatch(nhId: number) {
        const [amappings, aunmatched, ngalleries] = await Promise.all([
            // errored,
            mappings,
            unmatched,
            worker.getMPack(`${base}/data/galleries/${nhId % 1024}.bin.zst`, 'BookMap')
        ]);

        console.log(amappings, aunmatched, ngalleries);

        const result: MatchResult = {status: 'invalid-gallery'};

        if (ngalleries.has(nhId)) {
            const bookOrError = ngalleries.get(nhId)!;
            if (bookOrError.error) {
                result.nhentaiError = bookOrError.error;
                result.status = 'errored';
            } else {
                result.nhentaiGallery = bookOrError;
            }
        }

        if (amappings.has(nhId)) {
            const { gid, token } = amappings.get(nhId)!;

            result.sadpandaId = [gid, token];

            const response: IGDataRes = await fetch('https://api.e-hentai.org/api.php', {
                cache: 'force-cache',
                method: 'POST',
                body: JSON.stringify({
                    "method": "gdata",
                    "gidlist": [
                        [gid, tokenToHex(token)]
                    ],
                    "namespace": 1
                } satisfies IGDataReq)
            }).then(e => e.json());

            result.sadpandaGallery = response?.gmetadata?.[0];
            result.status = 'panda';
        } else if (aunmatched.has(nhId)) {
            result.status = 'no-panda';
        }

        console.log(result);
        return result;
    }

    function debounce<T extends (...args: TA) => void, TA extends any[]>(callback: T, wait = 300) {
        let timeout: ReturnType<typeof setTimeout>;

        return (...args: TA) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => callback(...args), wait);
        };
    };

    let currentId: number = -1;
    let currentResult: MatchResult = {status: 'not-started'};
    const debounced = debounce(async (nhId: string) => {
        if (+nhId != currentId) {
            currentResult = {status: 'waiting'};
            currentResult = await findMatch(+nhId);
            currentId = +nhId;
        }
    }, 1000);

    function fileType(type: ImageType) {
        switch (type) {
            case ImageType.Jpg: return 'jpg';
            case ImageType.Png: return 'png';
            case ImageType.Gif: return 'gif';
            case ImageType.Invalid1: return 'jpg';
            case ImageType.Invalid2: return 'png';
            case ImageType.Invalid3: return 'gif';
        }
    }

    // https://stackoverflow.com/q/42136098
    class Group<T> {
        constructor(public readonly key: string | number | symbol, public readonly members: T[] = []) {
            this.key = key;
        }
    }

    function groupBy<T>(list:T[], func:(x:T)=>string|number|symbol): Group<T>[] {
        const groups: Record<string | number | symbol, T[]> = {};
        for (const t of list) {
            const k = func(t);
            if (k in groups) {
                groups[k].push(t);
            } else {
                groups[k] = [t];
            }
        }

        const result: Group<T>[] = [];
        for (const groupKey in groups) {
            if (Object.hasOwn(groups, groupKey)) {
                result.push(new Group<T>(groupKey, groups[groupKey]));
            }
        }
        return result;
    }

    dayjs.extend(relativeTime);

    function elapsedSince(date: Date) {
        return dayjs(date).fromNow();
    }

    // https://stackoverflow.com/a/18650828
    function formatBytes(bytes: number, decimals = 2) {
        if (!+bytes) return '0 Bytes'

        const k = 1024
        const dm = decimals < 0 ? 0 : decimals
        const sizes = ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB']

        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`
    }

</script>

<svelte:head>
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css">
</svelte:head>

<Container>
    <h1>NHentai Metadata Browser</h1>

    <p>
        <InputGroup>
            <InputGroupText>#</InputGroupText>
            <Input
                type="number"
                placeholder="NHentai Gallery #"
                on:change={e => debounced(e.currentTarget.value)}
                on:keyup={e => debounced(e.currentTarget.value)}/>
        </InputGroup>
    </p>

    {#if currentResult}
    <!--
        status: 'panda' | 'no-panda' | 'errored' | 'invalid-gallery',
        nhentaiGallery?: NHentai_Book,
        nhentaiError?: string,
        sadpandaId?: [gid: number, token: bigint];
        sadpandaGallery?: IGMetadata | IGError,
    -->

        {#if currentResult.status == 'not-started'}
            <p>
                Type an NHentai gallery number above.
            </p>
            <p>
                Some notes:
                <ul>
                    <li>Some galleries will not be available because they were removed from NHentai before the crawler could get to it.</li>
                    <li>Some galleries were removed but are known, so they will only show metadata from ExHentai.</li>
                    <li>If the site isn't working, try a more recent browser.</li>
                    <li>
                        Some NHentai galleries could not be matched to ExHentai galleries due to small title differences.
                        In such case, use the "Search on ExHentai" / "Search on E-Hentai" link to find them yourself.
                    </li>
                    <li>
                        Very recent galleries were not present in the database dump that was used to correlate NHentai galleries with ExHentai galleries.
                        In such case, use the "Search on ExHentai" / "Search on E-Hentai" link to find them yourself.
                    </li>
                </ul>
            <footer>
                <p>
                    Crawled database:
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/releases/download/dbfiles/manga.7z">Download (82.3 MB, Sqlite3 in LZMA2, 2 GB uncompressed)</a>
                    <br>
                    Database in protobuf format:
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/releases/download/dbfiles/galleries.bin.zst">Download (87.1 MB in Zstandard, 600 MB uncompressed)</a>,
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/raw/master/HentaiMapGen/protobuf.proto">schema</a> (deserialize as <code>NHentai.BookList</code>)
                    <br>
                    Galleries missed by crawler (deleted, etc):
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/releases/download/dbfiles/erroredGalleries.json">Download (78.6 KB, JSON)</a>
                    <br>
                    NHentai galleries matched to ExHentai galleries:
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/releases/download/dbfiles/nhentaiMapping.json">Download (32.3 MB, JSON)</a>
                    <br>
                    NHentai galleries that could not be matched to ExHentai galleries:
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io/releases/download/dbfiles/naGalleries.json">Download (8.81 MB, JSON)</a>
                </p>
                <p>
                    <a href="https://github.com/npandaarchive/npandaarchive.github.io">Crawler, website, everything else source code</a>
                    <br>
                    Website & all source code is provided without warranty.
                    All rights to this work are waived under <a href="https://creativecommons.org/public-domain/cc0/">CC0</a> to the fullest extent possible by law.
                </p>
            </footer>
        {:else if currentResult.status == 'waiting'}
            <Spinner type="border" color="primary" />
        {:else if currentResult.status == 'invalid-gallery'}
            Gallery #{currentId} not found.
        {:else}
            {#if currentResult.status == 'errored'}
                Gallery #{currentId} had an error during archival (most likely it was removed and we could not archive it):
                <pre>
                    {currentResult.nhentaiError?.trim()}
                </pre>
            {:else}
                    <Row>
                        {#if currentResult.nhentaiGallery}
                        {@const nGallery = currentResult.nhentaiGallery}
                        <Col>
                            {#if nGallery.title?.english}
                            <h4>{nGallery.title?.english}</h4>
                            {/if}
                            {#if nGallery.title?.japanese}
                            <h5>{nGallery.title?.japanese}</h5>
                            {/if}
                            <p>
                                <a href="https://nhentai.net/g/{currentId}/">#{currentId}</a>
                                {#if !currentResult.sadpandaGallery || 'error' in currentResult.sadpandaGallery}
                                {@const anyTitle = nGallery.title?.pretty ?? nGallery.title?.english ?? nGallery.title?.japanese ?? ''}
                                <br><a href="https://e-hentai.org/?f_search={encodeURIComponent(anyTitle).replace(/%20/g, '+')}">Search on ExHentai</a>
                                <br><a href="https://exhentai.org/?f_search={encodeURIComponent(anyTitle).replace(/%20/g, '+')}">Search on E-Hentai</a>
                                {/if}
                            </p>

                            {#each groupBy(nGallery.tags ?? [], e => e.type) as group (group.key)}
                                <p><b>{String(group.key)[0].toUpperCase()}{String(group.key).slice(1)}:</b>
                                    {#each group.members as tag (tag?.id)}
                                    <a href="https://nhentai.net{tag?.url}"><Badge color="secondary" pill={true}>{tag?.name} ({tag?.count})</Badge></a>
                                    {/each}
                                </p>
                            {/each}

                            {#if nGallery.images}
                            <p>
                                <b>Pages:</b> <Badge color="secondary" pill={true}>{nGallery.images?.pages?.length}</Badge>
                            </p>
                            {/if}

                            {#if nGallery.uploadDate}
                            <p>Uploaded: {elapsedSince(nGallery.uploadDate)}</p>
                            {/if}
                        </Col>
                        {/if}
                        {#if currentResult.sadpandaGallery}
                            {@const pGallery = currentResult.sadpandaGallery}
                            {#if !('error' in pGallery)}
                                <Col>
                                    <h4>{pGallery.title}</h4>
                                    <h5>{pGallery.title_jpn}</h5>
                                    <p><a href="https://e-hentai.org/g/{pGallery.gid}/{pGallery.token}/">
                                        e-hentai.org/g/{pGallery.gid}/{pGallery.token}/
                                    </a></p>
                                    <p><a href="https://exhentai.org/g/{pGallery.gid}/{pGallery.token}/">
                                        exhentai.org/g/{pGallery.gid}/{pGallery.token}/
                                    </a></p>
                                    <Container>
                                        <Row>
                                            <Col>
                                                <img src={pGallery.thumb} alt="E-Hentai thumbnail for gallery">
                                            </Col>
                                            <Col>
                                                <p><b>Category:</b> {pGallery.category}</p>
                                                <!--<p><b>parent_gid:</b> {pGallery.parent_gid}</p>
                                                <p><b>parent_key:</b> {pGallery.parent_key}</p>-->
                                                <p><b>Uploader:</b> <a href="https://e-hentai.org/uploader/{pGallery.uploader}">{pGallery.uploader}</a></p>
                                                <p><b>Rating:</b> {pGallery.rating}</p>
                                                <p><b>Length:</b> {pGallery.filecount} pages</p>
                                                <p><b>File Size:</b> {formatBytes(pGallery.filesize)}</p>


                                                {#each groupBy(pGallery.tags.map(e => e.split(':')), e => e[0]) as group (group.key)}
                                                    <p><b>{String(group.key)[0].toUpperCase()}{String(group.key).slice(1)}:</b>
                                                        {#each group.members as tag (tag)}
                                                        <a href="https://exhentai.org/tag/{tag[0]}:{tag[1]}"><Badge color="secondary" pill={true}>{tag[1]}</Badge></a>
                                                        {/each}
                                                    </p>
                                                {/each}

                                                <p>Posted: {dayjs(new Date((+pGallery.posted)*1000)).fromNow()}</p>
                                            </Col>
                                        </Row>
                                    </Container>
                                </Col>
                            {:else}
                                <Col>
                                Error fetching ExHentai gallery info:
                                <pre>{pGallery.error}</pre>
                                </Col>
                            {/if}
                        {/if}
                    </Row>
            {/if}
        {/if}
    {/if}
</Container>
