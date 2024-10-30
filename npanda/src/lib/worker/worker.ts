import { expose } from 'comlink';

// fzstd is slower, but WASM based implementations cannot do streaming decompression
import * as fzstd from 'fzstd';

import { Page } from '$lib/memorypack/models/Page';
import { SadPandaIdToken } from '$lib/memorypack/models/SadPandaIdToken';
import { Tag } from '$lib/memorypack/models/Tag';
import { Title } from '$lib/memorypack/models/Title';
import { Images } from '$lib/memorypack/models/Images';
import { Book } from '$lib/memorypack/models/Book';
import { MemoryPackReader } from '$lib/memorypack/models/MemoryPackReader';

const types = {
    Page: Page,
    SadPandaIdToken: SadPandaIdToken,
    Tag: Tag,
    Title: Title,
    Images: Images,
    Book: Book,

    BookMap: class {
        static deserialize(buffer: ArrayBuffer) {
            const reader = new MemoryPackReader(buffer);
            return reader.readMap(
                reader => reader.readUint32(),
                reader => Book.deserializeCore(reader)!
            )!
        }
    },

    NHentaiMapping: class {
        static deserialize(buffer: ArrayBuffer) {
            const reader = new MemoryPackReader(buffer);
            return reader.readMap(
                reader => reader.readUint32(),
                reader => SadPandaIdToken.deserializeCore(reader)!
            )!
        }
    },

    NaGalleries: class {
        static deserialize(buffer: ArrayBuffer) {
            const reader = new MemoryPackReader(buffer);
            return reader.readMap(
                reader => reader.readUint32(),
                reader => Title.deserializeCore(reader)!
            )!
        }
    },

    ErroredGalleries: class {
        static deserialize(buffer: ArrayBuffer) {
            const reader = new MemoryPackReader(buffer);
            return reader.readArray(reader => reader.readUint32())!;
        }
    },
}

export class Api {
    async getMPack<T extends keyof typeof types>(path: string, fns: keyof typeof types & T): Promise<ReturnType<typeof types[T]['deserialize']>> {
        const fetched = await fetch(path, {
            cache: 'force-cache'
        });

        const result = fzstd.decompress(new Uint8Array(await fetched.arrayBuffer()));
        const decode = types[fns].deserialize(result) as ReturnType<typeof types[T]['deserialize']>;

        return decode;
    }
}

expose(new Api());
