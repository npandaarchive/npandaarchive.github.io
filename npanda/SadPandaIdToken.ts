import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class SadPandaIdToken {
    gid: number;
    token: bigint;

    constructor() {
        this.gid = 0;
        this.token = 0n;

    }

    static serialize(value: SadPandaIdToken | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: SadPandaIdToken | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(2);
        writer.writeUint32(value.gid);
        writer.writeUint64(value.token);

    }

    static serializeArray(value: (SadPandaIdToken | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (SadPandaIdToken | null)[] | null): void {
        writer.writeArray(value, (writer, x) => SadPandaIdToken.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): SadPandaIdToken | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): SadPandaIdToken | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new SadPandaIdToken();
        if (count == 2) {
            value.gid = reader.readUint32();
            value.token = reader.readUint64();

        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.gid = reader.readUint32(); if (count == 1) return value;
            value.token = reader.readUint64(); if (count == 2) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (SadPandaIdToken | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (SadPandaIdToken | null)[] | null {
        return reader.readArray(reader => SadPandaIdToken.deserializeCore(reader));
    }
}
