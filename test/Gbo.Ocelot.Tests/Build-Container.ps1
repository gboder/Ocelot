Remove-Item -Force -Recurse gw/published
dotnet publish -c release -o gw/published --self-contained -r linux-x64
docker build -t consul-demo-gw gw/