import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Page } from "./Page.js";

export class Images {
    pages: (Page | null)[] | null;
    cover: Page | null;
    thumbnail: Page | null;

    constructor() {
        this.pages = null;
        this.cover = null;
        this.thumbnail = null;

    }

    static serialize(value: Images | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Images | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(3);
        writer.writeArray(value.pages, (writer, x) => Page.serializeCore(writer, x));
        Page.serializeCore(writer, value.cover);
        Page.serializeCore(writer, value.thumbnail);

    }

    static serializeArray(value: (Images | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Images | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Images.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Images | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Images | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Images();
        if (count == 3) {
            value.pages = reader.readArray(reader => Page.deserializeCore(reader));
            value.cover = Page.deserializeCore(reader);
            value.thumbnail = Page.deserializeCore(reader);

        }
        else if (count > 3) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.pages = reader.readArray(reader => Page.deserializeCore(reader)); if (count == 1) return value;
            value.cover = Page.deserializeCore(reader); if (count == 2) return value;
            value.thumbnail = Page.deserializeCore(reader); if (count == 3) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Images | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Images | null)[] | null {
        return reader.readArray(reader => Images.deserializeCore(reader));
    }
}
