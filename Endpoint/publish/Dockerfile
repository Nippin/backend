FROM microsoft/dotnet:1.1-runtime
MAINTAINER slawomir@siudek.net

# libgdiplus is required to use System.Drawing used by current implementation of Selenium WebDriver
# I'm guessing it is necessary to make Screenshots
RUN apt-get update
RUN apt-get install -y libgdiplus

WORKDIR /dotnetapp

COPY . .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Endpoint.dll"]

