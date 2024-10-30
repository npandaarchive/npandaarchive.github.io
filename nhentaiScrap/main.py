import json
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
import sys

sleep = 0
error = 0

def my_encode(obj):
    #return sqlite3.Binary(lz4.frame.compress(msgpack.packb(obj)))
    return json.dumps(obj)

def my_decode(obj: str):
    #return msgpack.unpackb(lz4.frame.decompress(bytes(obj)))
    return json.loads(obj.removeprefix('json:'))


# 3 hrs
# 334261 to 342159
# (528626 - 334261) / 2632 = 73.8468844985
with (
    SqliteDict(sys.argv[1], autocommit=True, encode=my_encode, decode=my_decode, tablename="galleries_kv") as db,
    open("errors.txt", 'a') as fe
):
    for i in range(528626, 530360+1):
        if str(i) in db:
            print(f'{i} skipped')
            continue
        try:
            #with open(f'galleries/{i}.json', 'w') as fo:
            res = requests.get(f"https://nhentai.net/api/gallery/{i}")
            if res.status_code != 404:
                res.raise_for_status()

            value = json.loads(res.text)
            db[str(i)] = value
            print(i)
        except BaseException as ex:
            if isinstance(ex, KeyboardInterrupt):
                raise

            print(f"#{i} failed")
            traceback.print_exc()
            if error >= 1000:
                print("Hit 1000 errors. Exiting.")
                break
            error+=1
            fe.write(f"{i}\n")
        if sleep >= 100:
            time.sleep(10)
            sleep = 0
        else:
            sleep += 1

print(f"Total errors: {error}")
