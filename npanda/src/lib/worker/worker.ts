import { NHentai, NHentai_Book, NHentai_BookList, NHentai_BookMap, NHentai_BookMap_BooksEntry, NHentai_BookOrError, NHentai_Images, NHentai_Page, NHentai_Tag, NHentai_Title, NHentaiMapping, NHentaiMapping_ErroredGalleries, NHentaiMapping_MappingEntry, NHentaiMapping_SadPandaUrlParts, NHentaiMapping_UnmatchedGalleries, NHentaiMapping_UnmatchedGalleries_MappingEntry, type MessageFns } from '$lib/protobuf';

import { expose } from 'comlink';

// fzstd is slower, but WASM based implementations cannot do streaming decompression
import * as fzstd from 'fzstd';

const types = {
    NHentai: NHentai,
    NHentai_Title: NHentai_Title,
    NHentai_Page: NHentai_Page,
    NHentai_Images: NHentai_Images,
    NHentai_Tag: NHentai_Tag,
    NHentai_Book: NHentai_Book,
    NHentai_BookOrError: NHentai_BookOrError,
    NHentai_BookList: NHentai_BookList,
    NHentai_BookMap: NHentai_BookMap,
    NHentai_BookMap_BooksEntry: NHentai_BookMap_BooksEntry,
    NHentaiMapping: NHentaiMapping,
    NHentaiMapping_SadPandaUrlParts: NHentaiMapping_SadPandaUrlParts,
    NHentaiMapping_UnmatchedGalleries: NHentaiMapping_UnmatchedGalleries,
    NHentaiMapping_UnmatchedGalleries_MappingEntry: NHentaiMapping_UnmatchedGalleries_MappingEntry,
    NHentaiMapping_ErroredGalleries: NHentaiMapping_ErroredGalleries,
    NHentaiMapping_MappingEntry: NHentaiMapping_MappingEntry,
}

export class Api {
    async getProto<T extends keyof typeof types>(path: string, fns: keyof typeof types & T): Promise<ReturnType<typeof types[T]['decode']>> {
        const fetched = await fetch(path, {
            cache: 'force-cache'
        });

        const result = fzstd.decompress(new Uint8Array(await fetched.arrayBuffer()));
        const decode = types[fns].decode(result) as ReturnType<typeof types[T]['decode']>;

        return decode;
    }
}

expose(new Api());
