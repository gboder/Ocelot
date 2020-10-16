# escape=`

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY published/ ./
ENTRYPOINT ["dotnet", "gw.dll"]