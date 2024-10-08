// Code generated by protoc-gen-ts_proto. DO NOT EDIT.
// versions:
//   protoc-gen-ts_proto  v2.2.0
//   protoc               v5.27.1
// source: protobuf.proto

/* eslint-disable */
import { BinaryReader, BinaryWriter } from "@bufbuild/protobuf/wire";
import { Timestamp } from "./google/protobuf/timestamp";

export const protobufPackage = "HentaiMapGen.Proto";

export interface NHentai {
}

export enum NHentai_PageType {
  JPG = 0,
  PNG = 1,
  GIF = 2,
  INVALID1 = 3,
  INVALID2 = 4,
  INVALID3 = 5,
  UNRECOGNIZED = -1,
}

export interface NHentai_Title {
  english?: string | undefined;
  japanese?: string | undefined;
  pretty?: string | undefined;
}

export interface NHentai_Page {
  /** t */
  type: NHentai_PageType;
  /** w */
  width: number;
  /** h */
  height: number;
}

export interface NHentai_Images {
  pages: NHentai_Page[];
  cover: NHentai_Page | undefined;
  thumbnail: NHentai_Page | undefined;
}

export interface NHentai_Tag {
  id: number;
  type: string;
  name: string;
  url: string;
  count: number;
}

export interface NHentai_Book {
  mediaId: number;
  title: NHentai_Title | undefined;
  images: NHentai_Images | undefined;
  scanlator: string;
  uploadDate: Date | undefined;
  tags: NHentai_Tag[];
  numPages: number;
  numFavorites: number;
}

export interface NHentai_BookOrError {
  id: number;
  error?: string | undefined;
  book?: NHentai_Book | undefined;
}

export interface NHentai_BookList {
  books: NHentai_BookOrError[];
}

export interface NHentai_BookMap {
  books: { [key: number]: NHentai_BookOrError };
}

export interface NHentai_BookMap_BooksEntry {
  key: number;
  value: NHentai_BookOrError | undefined;
}

export interface NHentaiMapping {
  mapping: { [key: number]: NHentaiMapping_SadPandaUrlParts };
}

export interface NHentaiMapping_SadPandaUrlParts {
  Gid: number;
  Token: bigint;
}

export interface NHentaiMapping_UnmatchedGalleries {
  mapping: { [key: number]: NHentai_Title };
}

export interface NHentaiMapping_UnmatchedGalleries_MappingEntry {
  key: number;
  value: NHentai_Title | undefined;
}

export interface NHentaiMapping_ErroredGalleries {
  ids: number[];
}

export interface NHentaiMapping_MappingEntry {
  key: number;
  value: NHentaiMapping_SadPandaUrlParts | undefined;
}

function createBaseNHentai(): NHentai {
  return {};
}

export const NHentai: MessageFns<NHentai> = {
  encode(_: NHentai, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_Title(): NHentai_Title {
  return { english: undefined, japanese: undefined, pretty: undefined };
}

export const NHentai_Title: MessageFns<NHentai_Title> = {
  encode(message: NHentai_Title, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.english !== undefined) {
      writer.uint32(10).string(message.english);
    }
    if (message.japanese !== undefined) {
      writer.uint32(18).string(message.japanese);
    }
    if (message.pretty !== undefined) {
      writer.uint32(26).string(message.pretty);
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_Title {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_Title();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          message.english = reader.string();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.japanese = reader.string();
          continue;
        case 3:
          if (tag !== 26) {
            break;
          }

          message.pretty = reader.string();
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_Page(): NHentai_Page {
  return { type: 0, width: 0, height: 0 };
}

export const NHentai_Page: MessageFns<NHentai_Page> = {
  encode(message: NHentai_Page, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.type !== 0) {
      writer.uint32(8).int32(message.type);
    }
    if (message.width !== 0) {
      writer.uint32(16).uint32(message.width);
    }
    if (message.height !== 0) {
      writer.uint32(24).uint32(message.height);
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_Page {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_Page();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.type = reader.int32() as any;
          continue;
        case 2:
          if (tag !== 16) {
            break;
          }

          message.width = reader.uint32();
          continue;
        case 3:
          if (tag !== 24) {
            break;
          }

          message.height = reader.uint32();
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_Images(): NHentai_Images {
  return { pages: [], cover: undefined, thumbnail: undefined };
}

export const NHentai_Images: MessageFns<NHentai_Images> = {
  encode(message: NHentai_Images, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    for (const v of message.pages) {
      NHentai_Page.encode(v!, writer.uint32(10).fork()).join();
    }
    if (message.cover !== undefined) {
      NHentai_Page.encode(message.cover, writer.uint32(18).fork()).join();
    }
    if (message.thumbnail !== undefined) {
      NHentai_Page.encode(message.thumbnail, writer.uint32(26).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_Images {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_Images();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          message.pages.push(NHentai_Page.decode(reader, reader.uint32()));
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.cover = NHentai_Page.decode(reader, reader.uint32());
          continue;
        case 3:
          if (tag !== 26) {
            break;
          }

          message.thumbnail = NHentai_Page.decode(reader, reader.uint32());
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_Tag(): NHentai_Tag {
  return { id: 0, type: "", name: "", url: "", count: 0 };
}

export const NHentai_Tag: MessageFns<NHentai_Tag> = {
  encode(message: NHentai_Tag, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.id !== 0) {
      writer.uint32(8).uint32(message.id);
    }
    if (message.type !== "") {
      writer.uint32(18).string(message.type);
    }
    if (message.name !== "") {
      writer.uint32(26).string(message.name);
    }
    if (message.url !== "") {
      writer.uint32(34).string(message.url);
    }
    if (message.count !== 0) {
      writer.uint32(40).uint32(message.count);
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_Tag {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_Tag();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.id = reader.uint32();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.type = reader.string();
          continue;
        case 3:
          if (tag !== 26) {
            break;
          }

          message.name = reader.string();
          continue;
        case 4:
          if (tag !== 34) {
            break;
          }

          message.url = reader.string();
          continue;
        case 5:
          if (tag !== 40) {
            break;
          }

          message.count = reader.uint32();
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_Book(): NHentai_Book {
  return {
    mediaId: 0,
    title: undefined,
    images: undefined,
    scanlator: "",
    uploadDate: undefined,
    tags: [],
    numPages: 0,
    numFavorites: 0,
  };
}

export const NHentai_Book: MessageFns<NHentai_Book> = {
  encode(message: NHentai_Book, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.mediaId !== 0) {
      writer.uint32(16).uint32(message.mediaId);
    }
    if (message.title !== undefined) {
      NHentai_Title.encode(message.title, writer.uint32(26).fork()).join();
    }
    if (message.images !== undefined) {
      NHentai_Images.encode(message.images, writer.uint32(34).fork()).join();
    }
    if (message.scanlator !== "") {
      writer.uint32(42).string(message.scanlator);
    }
    if (message.uploadDate !== undefined) {
      Timestamp.encode(toTimestamp(message.uploadDate), writer.uint32(50).fork()).join();
    }
    for (const v of message.tags) {
      NHentai_Tag.encode(v!, writer.uint32(58).fork()).join();
    }
    if (message.numPages !== 0) {
      writer.uint32(64).uint32(message.numPages);
    }
    if (message.numFavorites !== 0) {
      writer.uint32(72).uint32(message.numFavorites);
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_Book {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_Book();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 2:
          if (tag !== 16) {
            break;
          }

          message.mediaId = reader.uint32();
          continue;
        case 3:
          if (tag !== 26) {
            break;
          }

          message.title = NHentai_Title.decode(reader, reader.uint32());
          continue;
        case 4:
          if (tag !== 34) {
            break;
          }

          message.images = NHentai_Images.decode(reader, reader.uint32());
          continue;
        case 5:
          if (tag !== 42) {
            break;
          }

          message.scanlator = reader.string();
          continue;
        case 6:
          if (tag !== 50) {
            break;
          }

          message.uploadDate = fromTimestamp(Timestamp.decode(reader, reader.uint32()));
          continue;
        case 7:
          if (tag !== 58) {
            break;
          }

          message.tags.push(NHentai_Tag.decode(reader, reader.uint32()));
          continue;
        case 8:
          if (tag !== 64) {
            break;
          }

          message.numPages = reader.uint32();
          continue;
        case 9:
          if (tag !== 72) {
            break;
          }

          message.numFavorites = reader.uint32();
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_BookOrError(): NHentai_BookOrError {
  return { id: 0, error: undefined, book: undefined };
}

export const NHentai_BookOrError: MessageFns<NHentai_BookOrError> = {
  encode(message: NHentai_BookOrError, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.id !== 0) {
      writer.uint32(8).uint32(message.id);
    }
    if (message.error !== undefined) {
      writer.uint32(18).string(message.error);
    }
    if (message.book !== undefined) {
      NHentai_Book.encode(message.book, writer.uint32(26).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_BookOrError {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_BookOrError();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.id = reader.uint32();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.error = reader.string();
          continue;
        case 3:
          if (tag !== 26) {
            break;
          }

          message.book = NHentai_Book.decode(reader, reader.uint32());
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_BookList(): NHentai_BookList {
  return { books: [] };
}

export const NHentai_BookList: MessageFns<NHentai_BookList> = {
  encode(message: NHentai_BookList, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    for (const v of message.books) {
      NHentai_BookOrError.encode(v!, writer.uint32(10).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_BookList {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_BookList();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          message.books.push(NHentai_BookOrError.decode(reader, reader.uint32()));
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_BookMap(): NHentai_BookMap {
  return { books: {} };
}

export const NHentai_BookMap: MessageFns<NHentai_BookMap> = {
  encode(message: NHentai_BookMap, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    Object.entries(message.books).forEach(([key, value]) => {
      NHentai_BookMap_BooksEntry.encode({ key: key as any, value }, writer.uint32(10).fork()).join();
    });
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_BookMap {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_BookMap();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          const entry1 = NHentai_BookMap_BooksEntry.decode(reader, reader.uint32());
          if (entry1.value !== undefined) {
            message.books[entry1.key] = entry1.value;
          }
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentai_BookMap_BooksEntry(): NHentai_BookMap_BooksEntry {
  return { key: 0, value: undefined };
}

export const NHentai_BookMap_BooksEntry: MessageFns<NHentai_BookMap_BooksEntry> = {
  encode(message: NHentai_BookMap_BooksEntry, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.key !== 0) {
      writer.uint32(8).uint32(message.key);
    }
    if (message.value !== undefined) {
      NHentai_BookOrError.encode(message.value, writer.uint32(18).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentai_BookMap_BooksEntry {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentai_BookMap_BooksEntry();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.key = reader.uint32();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.value = NHentai_BookOrError.decode(reader, reader.uint32());
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping(): NHentaiMapping {
  return { mapping: {} };
}

export const NHentaiMapping: MessageFns<NHentaiMapping> = {
  encode(message: NHentaiMapping, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    Object.entries(message.mapping).forEach(([key, value]) => {
      NHentaiMapping_MappingEntry.encode({ key: key as any, value }, writer.uint32(10).fork()).join();
    });
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          const entry1 = NHentaiMapping_MappingEntry.decode(reader, reader.uint32());
          if (entry1.value !== undefined) {
            message.mapping[entry1.key] = entry1.value;
          }
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping_SadPandaUrlParts(): NHentaiMapping_SadPandaUrlParts {
  return { Gid: 0, Token: 0n };
}

export const NHentaiMapping_SadPandaUrlParts: MessageFns<NHentaiMapping_SadPandaUrlParts> = {
  encode(message: NHentaiMapping_SadPandaUrlParts, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.Gid !== 0) {
      writer.uint32(8).uint32(message.Gid);
    }
    if (message.Token !== 0n) {
      if (BigInt.asUintN(64, message.Token) !== message.Token) {
        throw new globalThis.Error("value provided for field message.Token of type uint64 too large");
      }
      writer.uint32(16).uint64(message.Token);
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping_SadPandaUrlParts {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping_SadPandaUrlParts();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.Gid = reader.uint32();
          continue;
        case 2:
          if (tag !== 16) {
            break;
          }

          message.Token = reader.uint64() as bigint;
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping_UnmatchedGalleries(): NHentaiMapping_UnmatchedGalleries {
  return { mapping: {} };
}

export const NHentaiMapping_UnmatchedGalleries: MessageFns<NHentaiMapping_UnmatchedGalleries> = {
  encode(message: NHentaiMapping_UnmatchedGalleries, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    Object.entries(message.mapping).forEach(([key, value]) => {
      NHentaiMapping_UnmatchedGalleries_MappingEntry.encode({ key: key as any, value }, writer.uint32(10).fork())
        .join();
    });
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping_UnmatchedGalleries {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping_UnmatchedGalleries();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 10) {
            break;
          }

          const entry1 = NHentaiMapping_UnmatchedGalleries_MappingEntry.decode(reader, reader.uint32());
          if (entry1.value !== undefined) {
            message.mapping[entry1.key] = entry1.value;
          }
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping_UnmatchedGalleries_MappingEntry(): NHentaiMapping_UnmatchedGalleries_MappingEntry {
  return { key: 0, value: undefined };
}

export const NHentaiMapping_UnmatchedGalleries_MappingEntry: MessageFns<
  NHentaiMapping_UnmatchedGalleries_MappingEntry
> = {
  encode(
    message: NHentaiMapping_UnmatchedGalleries_MappingEntry,
    writer: BinaryWriter = new BinaryWriter(),
  ): BinaryWriter {
    if (message.key !== 0) {
      writer.uint32(8).uint32(message.key);
    }
    if (message.value !== undefined) {
      NHentai_Title.encode(message.value, writer.uint32(18).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping_UnmatchedGalleries_MappingEntry {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping_UnmatchedGalleries_MappingEntry();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.key = reader.uint32();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.value = NHentai_Title.decode(reader, reader.uint32());
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping_ErroredGalleries(): NHentaiMapping_ErroredGalleries {
  return { ids: [] };
}

export const NHentaiMapping_ErroredGalleries: MessageFns<NHentaiMapping_ErroredGalleries> = {
  encode(message: NHentaiMapping_ErroredGalleries, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    writer.uint32(10).fork();
    for (const v of message.ids) {
      writer.uint32(v);
    }
    writer.join();
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping_ErroredGalleries {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping_ErroredGalleries();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag === 8) {
            message.ids.push(reader.uint32());

            continue;
          }

          if (tag === 10) {
            const end2 = reader.uint32() + reader.pos;
            while (reader.pos < end2) {
              message.ids.push(reader.uint32());
            }

            continue;
          }

          break;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function createBaseNHentaiMapping_MappingEntry(): NHentaiMapping_MappingEntry {
  return { key: 0, value: undefined };
}

export const NHentaiMapping_MappingEntry: MessageFns<NHentaiMapping_MappingEntry> = {
  encode(message: NHentaiMapping_MappingEntry, writer: BinaryWriter = new BinaryWriter()): BinaryWriter {
    if (message.key !== 0) {
      writer.uint32(8).uint32(message.key);
    }
    if (message.value !== undefined) {
      NHentaiMapping_SadPandaUrlParts.encode(message.value, writer.uint32(18).fork()).join();
    }
    return writer;
  },

  decode(input: BinaryReader | Uint8Array, length?: number): NHentaiMapping_MappingEntry {
    const reader = input instanceof BinaryReader ? input : new BinaryReader(input);
    let end = length === undefined ? reader.len : reader.pos + length;
    const message = createBaseNHentaiMapping_MappingEntry();
    while (reader.pos < end) {
      const tag = reader.uint32();
      switch (tag >>> 3) {
        case 1:
          if (tag !== 8) {
            break;
          }

          message.key = reader.uint32();
          continue;
        case 2:
          if (tag !== 18) {
            break;
          }

          message.value = NHentaiMapping_SadPandaUrlParts.decode(reader, reader.uint32());
          continue;
      }
      if ((tag & 7) === 4 || tag === 0) {
        break;
      }
      reader.skip(tag & 7);
    }
    return message;
  },
};

function toTimestamp(date: Date): Timestamp {
  const seconds = BigInt(Math.trunc(date.getTime() / 1_000));
  const nanos = (date.getTime() % 1_000) * 1_000_000;
  return { seconds, nanos };
}

function fromTimestamp(t: Timestamp): Date {
  let millis = (globalThis.Number(t.seconds.toString()) || 0) * 1_000;
  millis += (t.nanos || 0) / 1_000_000;
  return new globalThis.Date(millis);
}

export interface MessageFns<T> {
  encode(message: T, writer?: BinaryWriter): BinaryWriter;
  decode(input: BinaryReader | Uint8Array, length?: number): T;
}
