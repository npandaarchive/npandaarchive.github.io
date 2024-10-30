import { MemoryPackWriter } from "./MemoryPackWriter";
import { MemoryPackReader } from "./MemoryPackReader";

export class Title {
    english: string | null;
    japanese: string | null;
    pretty: string | null;

    constructor() {
        this.english = null;
        this.japanese = null;
        this.pretty = null;

    }

    static serialize(value: Title | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Title | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(3);
        writer.writeString(value.english);
        writer.writeString(value.japanese);
        writer.writeString(value.pretty);

    }

    static serializeArray(value: (Title | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Title | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Title.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Title | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Title | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Title();
        if (count == 3) {
            value.english = reader.readString();
            value.japanese = reader.readString();
            value.pretty = reader.readString();

        }
        else if (count > 3) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.english = reader.readString(); if (count == 1) return value;
            value.japanese = reader.readString(); if (count == 2) return value;
            value.pretty = reader.readString(); if (count == 3) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Title | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Title | null)[] | null {
        return reader.readArray(reader => Title.deserializeCore(reader));
    }
}
