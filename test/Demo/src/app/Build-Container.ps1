dotnet clean
dotnet publish -c debug --self-contained -r linux-x64 -o published
docker build -t consul-demo-app .