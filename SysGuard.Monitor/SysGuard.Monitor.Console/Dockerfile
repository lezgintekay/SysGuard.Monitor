FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SysGuard.Monitor.Console/SysGuard.Monitor.Console.csproj", "SysGuard.Monitor.Console/"]
RUN dotnet restore "SysGuard.Monitor.Console/SysGuard.Monitor.Console.csproj"
COPY . .
WORKDIR "/src/SysGuard.Monitor.Console"
RUN dotnet build "./SysGuard.Monitor.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SysGuard.Monitor.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SysGuard.Monitor.Console.dll"]
