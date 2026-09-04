#!/bin/sh
# Copy frontend build artifacts into the shared volume
cp -r /opt/frontend-dist/. /usr/share/nginx/html/

# Start nginx (user appuser directive handles worker privileges)
exec nginx -g 'daemon off;'
