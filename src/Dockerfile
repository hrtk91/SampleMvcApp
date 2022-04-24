# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
ENV PATH="$PATH:~/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT="Staging"
COPY src /src

FROM base as build
RUN dotnet build "/src/SampleMvcApp.csproj"

FROM build AS migration
RUN dotnet tool install --global dotnet-ef --version 6.0.4
RUN dotnet ef migrations add Initial --project /src/SampleMvcApp.csproj
ENTRYPOINT [ "dotnet", "ef", "database", "update", "--project", "/src/SampleMvcApp.csproj", "--verbose"]

FROM build AS publish
# RUN dotnet publish -c release -o /app /src/SampleMvcApp.csproj
RUN dotnet publish -o /app /src/SampleMvcApp.csproj
WORKDIR /app
RUN dotnet dev-certs https
ENTRYPOINT [ "dotnet", "/app/SampleMvcApp.dll" ]
