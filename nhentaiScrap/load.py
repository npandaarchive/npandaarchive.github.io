import json
import os
import pickle
import traceback
from typing import cast
import zlib
import requests
import time
import sqlite3
from sqlitedict import SqliteDict
import msgpack
import lz4.frame

def my_encode(obj):
    #return sqlite3.Binary(lz4.frame.compress(msgpack.packb(obj)))
    return 'json:' + json.dumps(obj)

def my_decode(obj: str):
    #return msgpack.unpackb(lz4.frame.decompress(bytes(obj)))
    return json.loads(obj.removeprefix('json:'))

error = 0

# 3 hrs
# 334261 to 342159
# (528626 - 334261) / 2632 = 73.8468844985
with (
    SqliteDict("manga.db", encode=my_encode, decode=my_decode) as db,
    open("errors.txt", 'a') as fe
):
    for f in os.listdir('./galleries'):
        n, ex = os.path.splitext(os.path.basename(f))

        if n not in db:
            with open(f'./galleries/{f}', 'r') as fo:
                try:
                    j = json.load(fo)
                except json.decoder.JSONDecodeError:
                    print(f"#{n} failed")
                    traceback.print_exc()
                    fe.write(f"{n}\n")
                    continue

            db[n] = j

            if int(n) % 1000 == 0:
                print(n)
                db.commit()
                print('commit')
        else:
            if int(n) % 1000 == 0:
                print(f'{n} skip')
    db.commit()
    print('commit done')
