#!/bin/sh
docker-compose.exe down && sh .\\command\\docker\\dockerRemoveImages.sh && docker-compose up -d --build