dotnet clean
dotnet publish -c release --self-contained -r linux-x64
docker build -t consul-demo-gw .\bin\release\netcoreapp3.1\linux-x64\publish\