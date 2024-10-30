import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { ImageType } from "./ImageType.js";

export class Page {
    type: ImageType;
    width: number;
    height: number;

    constructor() {
        this.type = 0;
        this.width = 0;
        this.height = 0;

    }

    static serialize(value: Page | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Page | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(3);
        writer.writeUint8(value.type);
        writer.writeUint32(value.width);
        writer.writeUint32(value.height);

    }

    static serializeArray(value: (Page | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Page | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Page.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Page | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Page | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Page();
        if (count == 3) {
            value.type = reader.readUint8();
            value.width = reader.readUint32();
            value.height = reader.readUint32();

        }
        else if (count > 3) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.type = reader.readUint8(); if (count == 1) return value;
            value.width = reader.readUint32(); if (count == 2) return value;
            value.height = reader.readUint32(); if (count == 3) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Page | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Page | null)[] | null {
        return reader.readArray(reader => Page.deserializeCore(reader));
    }
}
