FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src

FROM base AS dev
COPY . .
WORKDIR /src/src/OpenDXP.Api
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080", "--no-launch-profile"]

FROM base AS build
COPY . .
WORKDIR /src/src/OpenDXP.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpenDXP.Api.dll"]
