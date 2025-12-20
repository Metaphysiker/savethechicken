#!/bin/bash

docker compose --file docker-compose.build.yml build

docker save savethechicken-production-webapi | bzip2 | pv | ssh deploy@84.234.19.192 docker load

docker save savethechicken-production-web | bzip2 | pv | ssh deploy@84.234.19.192 docker load

scp docker-compose.remote.yml deploy@84.234.19.192:/home/deploy/savethechicken

scp .env deploy@84.234.19.192:/home/deploy/savethechicken

ssh deploy@84.234.19.192 << EOF
    cd /home/deploy/savethechicken
    docker compose --file docker-compose.remote.yml down
    docker compose --file docker-compose.remote.yml up -d
    docker system prune -f
EOF
