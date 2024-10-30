import { expose } from 'comlink';

// fzstd is slower, but WASM based implementations cannot do streaming decompression
import * as fzstd from 'fzstd';

import { Unpackr } from 'msgpackr/unpack';
// import { decode as msgpackDecode } from 'notepack.io';
// import { decode as msgpackDecode } from '@msgpack/msgpack';

export const enum ImageType {
    Jpg,
    Png,
    Gif,
    Invalid1,
    Invalid2,
    Invalid3
}

const types = {
    Page: class {
        constructor(
            public readonly t: ImageType,
            public readonly w: number,
            public readonly h: number,
        ) {}

        static deserialize(data: any[] | null) {
            return data == null ? null : new this(...data);
        }
    },
    SadPandaIdToken: class {
        constructor(
            public readonly gid: number,
            public readonly token: bigint,
        ) {}

        static deserialize(data: any[] | null) {
            return data == null ? null : new this(...data);
        }
    },
    Tag: class {
        constructor(
            public readonly id: number,
            public readonly type: string,
            public readonly name: string,
            public readonly url: string,
            public readonly count: number,
        ) {}

        static deserialize(data: any[] | null) {
            return data == null ? null : new this(...data);
        }
    },
    Title: class {
        constructor(
            public readonly english: string,
            public readonly japanese: string,
            public readonly pretty: string,
        ) {}

        static deserialize(data: any[] | null) {
            return data == null ? null : new this(data[0], data[1], data[2]);
        }
    },
    Images: class {
        constructor(
            public pages: InstanceType<typeof types.Page>[],
            public cover: InstanceType<typeof types.Page>,
            public Thumbnail: InstanceType<typeof types.Page>,
        ){}

        static deserialize(data: any[] | null) {
            return data == null ? null : new this(data[0], data[1], data[2]);
        }
    },
    Book: class {
        constructor(
            public id: number,
            public title: InstanceType<typeof types.Title>,

            public error?: string,
            public mediaId?: number,
            public images?: InstanceType<typeof types.Images>,
            public scanlator?: string,
            public uploadDate?: Date,
            public tags?: InstanceType<typeof types.Tag>[],
            public numPages?: number,
            public numFavorites?: number,
        ) {}

        static deserialize(data: any[]) {
            return new this(
                data[1],
                types.Title.deserialize(data[3]),

                data[0],
                data[2],
                types.Images.deserialize(data[4]),
                data[5],
                data[6],
                data[7]?.map((e: any[]) => types.Tag.deserialize(e)),
                data[8],
                data[9],
            );
        }
    },

    BookMap: class {
        static deserialize(data: Record<string, any>) {
            return new Map(
                Object.entries(data)
                    .map(([k, v]) => [Number(k), types.Book.deserialize(v)])
            );
        }
    },

    NHentaiMapping: class {
        static deserialize(data: any[]) {
            return new Map(
                Object.entries(data)
                    .map(([k, v]) => [Number(k), types.SadPandaIdToken.deserialize(v)])
            );
        }
    },

    NaGalleries: class {
        static deserialize(data: any[]) {
            return new Map(
                Object.entries(data)
                    .map(([k, v]) => [Number(k), types.Title.deserialize(v)])
            );
        }
    },

    ErroredGalleries: class {
        static deserialize(data: any[]) {
            return data as number[];
        }
    },
};

export type Book = InstanceType<typeof types.Book>;

const unpackr = new Unpackr({ useRecords: false });

export class Api {
    async getMPack<T extends keyof typeof types>(path: string, fns: keyof typeof types & T): Promise<ReturnType<typeof types[T]['deserialize']>> {
        const fetched = await fetch(path, {
            cache: 'force-cache'
        });

        const result = fzstd.decompress(new Uint8Array(await fetched.arrayBuffer()));
        const decode = types[fns].deserialize(unpackr.decode(new Uint8Array(result)) as any[]) as ReturnType<typeof types[T]['deserialize']>;

        return decode;
    }
}

expose(new Api());
