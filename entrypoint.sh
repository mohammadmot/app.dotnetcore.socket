#!/bin/bash
set -e

# ENTITY_SRC_PATH=app.exnance.coin_board.Infrastructure
# WEB_SRC_PATH=Ara.Archive.Host.Rest

# service nginx start || true

# migrate

#cd $ENTITY_SRC_PATH
#dotnet ef database update

#cd ..

# run app
#cd $WEB_SRC_PATH/bin/Release/net5.0/

service nginx start || true

cd /app/publish/netcoreapp3.1
dotnet ara.influxdb.webapi.tool.dll

exec "$@"
