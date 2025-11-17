podman run -d \
  --name mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=mypass \
  -v mongodb_data:/data/db \
  mongo:latest

sudo firewall-cmd --add-port=27017/tcp


podman logs mariadb

podman logs rabbitmq
