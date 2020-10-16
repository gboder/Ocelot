Remove-Item -Force -Recurse published
dotnet publish -c release -o published --self-contained -r linux-x64 
docker build -t consul-demo-app published