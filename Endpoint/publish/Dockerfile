FROM microsoft/dotnet:1.1-runtime
MAINTAINER slawomir@siudek.net

WORKDIR /dotnetapp

COPY . .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Endpoint.dll"]

