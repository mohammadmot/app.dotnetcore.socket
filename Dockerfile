FROM registry.aranuma.com:5000/mcr.microsoft.com/dotnet/sdk:3.1 AS base  

RUN mkdir app
WORKDIR /app

COPY . /app

RUN apt update && apt install -y nginx
COPY nginx.conf /etc/nginx/sites-enabled/default

ADD ./nuget.config  ~/.nuget/NuGet/NuGet.Config

COPY . .

RUN dotnet restore "/app/src/ara.influxdb.webapi.tool.csproj" --configfile=~/.nuget/NuGet/NuGet.Config

RUN cd src

RUN dotnet publish -c Release -o /app/publish

# install entity framework tool in the container
#RUN dotnet tool install --global dotnet-ef
#ENV PATH=$PATH:/root/.dotnet/tools

ENV TZ=Asia/Tehran
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

RUN echo $(ls )
#RUN dotnet dev-certs https
RUN chmod +x entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]
