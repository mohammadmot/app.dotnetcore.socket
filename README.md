# project
- .net core console application for test socket

# run in docker with docker compose file
## if config file changed only
- docker-compose -f code.socket-compose.yml up -d --force--recreate
## if source code changed then --build
- docker-compose -f code.socket-compose.yml up -d --build

# goto running container logs
- docker logs 746

# goto executed image (runing container) shell
- docker exec -it 746 sh

# stop container
- docker stop 746

# kill container => don't show in [docker ps], but image is exist in [docker images]
- docker kill 746

# run 1 instance from docker images; if exit shell make container kill
docker run -it 4e6 sh

# run 1 instance from docker images; infinit
docker run -d 4e6