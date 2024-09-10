/*!
https://github.com/SophiaH67/e-hentai-ts

MIT License

Copyright (c) 2021 Ryan Sonshine

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
export interface IGDataReq {
    method: 'gdata';
    gidlist: [id: number, token: string][];
    namespace: 1;
}

export interface IGDataRes {
    gmetadata: (IGMetadata | IGError)[];
}

export interface IGError {
    gid: number;
    error: string;
}

export interface IGMetadata {
    gid: number;
    token: string;
    archiver_key: string;
    title: string;
    title_jpn: string;
    category: string;
    thumb: string;
    uploader: string;
    posted: string;
    filecount: string;
    filesize: number;
    expunged: boolean;
    rating: string;
    torrentcount: string;
    torrents: ITorrent[];
    tags: string[];
    parent_gid?: string;
    parent_key?: string;
    first_gid: string;
    first_key: string;
}

export interface ITorrent {
    hash: string;
    added: string;
    name: string;
    tsize: string;
    fsize: string;
}