import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class Tag {
    id: number;
    type: string | null;
    name: string | null;
    url: string | null;
    count: number;

    constructor() {
        this.id = 0;
        this.type = null;
        this.name = null;
        this.url = null;
        this.count = 0;

    }

    static serialize(value: Tag | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Tag | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(5);
        writer.writeUint32(value.id);
        writer.writeString(value.type);
        writer.writeString(value.name);
        writer.writeString(value.url);
        writer.writeUint32(value.count);

    }

    static serializeArray(value: (Tag | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Tag | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Tag.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Tag | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Tag | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Tag();
        if (count == 5) {
            value.id = reader.readUint32();
            value.type = reader.readString();
            value.name = reader.readString();
            value.url = reader.readString();
            value.count = reader.readUint32();

        }
        else if (count > 5) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.id = reader.readUint32(); if (count == 1) return value;
            value.type = reader.readString(); if (count == 2) return value;
            value.name = reader.readString(); if (count == 3) return value;
            value.url = reader.readString(); if (count == 4) return value;
            value.count = reader.readUint32(); if (count == 5) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Tag | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Tag | null)[] | null {
        return reader.readArray(reader => Tag.deserializeCore(reader));
    }
}
