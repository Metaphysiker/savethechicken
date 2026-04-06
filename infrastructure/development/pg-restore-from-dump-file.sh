#!/bin/bash

path_to_dump="$1"

# copy file to docker container
docker cp $path_to_dump savethechicken-development-postgres-1:/dump.dump

# restore dump
# -U: username
# -c: clean (drop) database objects before recreating them
# -d: database name
# --no-owner: ignore ownership information in the backup
# --role: role name to be used when restoring objects
docker exec -it savethechicken-development-postgres-1 pg_restore -U savethechicken -c -d savethechicken --no-owner --role=savethechicken /dump.dump

