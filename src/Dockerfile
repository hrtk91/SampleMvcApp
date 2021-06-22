# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ENV PATH="$PATH:~/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT="Staging"
RUN dotnet tool install --global dotnet-ef --version 5.0.7

WORKDIR /source
COPY . ./aspnetapp/
RUN dotnet build ./aspnetapp/SampleMvcApp.csproj

WORKDIR /source/aspnetapp
RUN dotnet publish -c release -o /app --no-restore SampleMvcApp.csproj

# final stage/image
# FROM mcr.microsoft.com/dotnet/aspnet:5.0
# FROM mcr.microsoft.com/dotnet/sdk:5.0
# COPY --from=build "/source/aspnetapp/." "/source/aspnetapp/"
# COPY --from=build "/app" "/"
# WORKDIR /app
WORKDIR /app
CMD /source/aspnetapp/entrypoint.sh
