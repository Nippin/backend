FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 81
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "Endpoint.dll"]
