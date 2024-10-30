import { MemoryPackWriter } from "./MemoryPackWriter";
import { MemoryPackReader } from "./MemoryPackReader";
import { Title } from "./Title";
import { Images } from "./Images";
import { Tag } from "./Tag";

export class Book {
    error: string | null;
    id: number | null;
    mediaId: number | null;
    title: Title | null;
    images: Images | null;
    scanlator: string | null;
    uploadDate: Date | null;
    tags: (Tag | null)[] | null;
    numPages: number | null;
    numFavorites: number | null;

    constructor() {
        this.error = null;
        this.id = null;
        this.mediaId = null;
        this.title = null;
        this.images = null;
        this.scanlator = null;
        this.uploadDate = null;
        this.tags = null;
        this.numPages = null;
        this.numFavorites = null;

    }

    static serialize(value: Book | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Book | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(10);
        writer.writeString(value.error);
        writer.writeNullableUint32(value.id);
        writer.writeNullableUint32(value.mediaId);
        Title.serializeCore(writer, value.title);
        Images.serializeCore(writer, value.images);
        writer.writeString(value.scanlator);
        writer.writeNullableDate(value.uploadDate);
        writer.writeArray(value.tags, (writer, x) => Tag.serializeCore(writer, x));
        writer.writeNullableUint32(value.numPages);
        writer.writeNullableUint32(value.numFavorites);

    }

    static serializeArray(value: (Book | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Book | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Book.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Book | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Book | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Book();
        if (count == 10) {
            value.error = reader.readString();
            value.id = reader.readNullableUint32();
            value.mediaId = reader.readNullableUint32();
            value.title = Title.deserializeCore(reader);
            value.images = Images.deserializeCore(reader);
            value.scanlator = reader.readString();
            value.uploadDate = reader.readNullableDate();
            value.tags = reader.readArray(reader => Tag.deserializeCore(reader));
            value.numPages = reader.readNullableUint32();
            value.numFavorites = reader.readNullableUint32();

        }
        else if (count > 10) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.error = reader.readString(); if (count == 1) return value;
            value.id = reader.readNullableUint32(); if (count == 2) return value;
            value.mediaId = reader.readNullableUint32(); if (count == 3) return value;
            value.title = Title.deserializeCore(reader); if (count == 4) return value;
            value.images = Images.deserializeCore(reader); if (count == 5) return value;
            value.scanlator = reader.readString(); if (count == 6) return value;
            value.uploadDate = reader.readNullableDate(); if (count == 7) return value;
            value.tags = reader.readArray(reader => Tag.deserializeCore(reader)); if (count == 8) return value;
            value.numPages = reader.readNullableUint32(); if (count == 9) return value;
            value.numFavorites = reader.readNullableUint32(); if (count == 10) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Book | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Book | null)[] | null {
        return reader.readArray(reader => Book.deserializeCore(reader));
    }
}
