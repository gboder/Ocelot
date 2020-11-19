# escape=`

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime

WORKDIR /consul
RUN apt-get update && apt-get install zip wget supervisor -y && wget https://releases.hashicorp.com/consul/1.8.5/consul_1.8.5_linux_amd64.zip && unzip consul_1.8.5_linux_amd64.zip && ln -s consul /usr/bin/consul
RUN mkdir -p /var/log/supervisor

WORKDIR /app
COPY published/ ./

#ENTRYPOINT ["dotnet", "app.dll"]
CMD ["/usr/bin/supervisord"]