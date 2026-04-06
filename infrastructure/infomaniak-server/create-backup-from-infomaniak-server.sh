#!/bin/bash

path_to_save_dump="$1"

now=$(date +"%Y-%m-%d_%H_%M_%S")
backup_name="dump_$now.dump"

ssh deploy@84.234.19.192 << EOF
    cd /home/deploy/savethechicken
    docker exec savethechicken-production-postgres-1 bash -c 'pg_dump -Fc -U savethechicken savethechicken > db.dump'
    docker cp savethechicken-production-postgres-1:/db.dump $backup_name
    mkdir -p /home/deploy/backups/savethechicken
    mv $backup_name /home/deploy/backups/savethechicken
EOF

scp deploy@84.234.19.192:/home/deploy/backups/savethechicken/$backup_name $path_to_save_dump
