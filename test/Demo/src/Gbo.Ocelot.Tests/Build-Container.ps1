dotnet clean
dotnet publish -c debug --self-contained -r linux-x64
docker build -t consul-demo-gw .\bin\debug\netcoreapp3.1\linux-x64\publish\