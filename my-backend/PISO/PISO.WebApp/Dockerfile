FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PISO.sln", "./"]
COPY ["PISO.Contracts/PISO.Contracts.csproj", "PISO.Contracts/"]
COPY ["PISO.Entities/PISO.Entities.csproj", "PISO.Entities/"]
COPY ["PISO.LoggerService/PISO.LoggerService.csproj", "PISO.LoggerService/"]
COPY ["PISO.Presentation/PISO.Presentation.csproj", "PISO.Presentation/"]
COPY ["PISO.Repository/PISO.Repository.csproj", "PISO.Repository/"]
COPY ["PISO.Service/PISO.Service.csproj", "PISO.Service/"]
COPY ["PISO.Service.Contracts/PISO.Service.Contracts.csproj", "PISO.Service.Contracts/"]
COPY ["PISO.Shared/PISO.Shared.csproj", "PISO.Shared/"]
COPY ["PISO.RedisService/PISO.RedisService.csproj", "PISO.RedisService/"]
COPY ["PISO.HangfireService/PISO.HangfireService.csproj", "PISO.HangfireService/"]
COPY ["PISO.GoogleMapService/PISO.GoogleMapService.csproj", "PISO.GoogleMapService/"]
COPY ["PISO.WebApp/PISO.WebApp.csproj", "PISO.WebApp/"]
RUN dotnet restore "PISO.sln"
COPY . .
WORKDIR "/src/PISO.WebApp"
RUN dotnet build "./PISO.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PISO.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV DOTNET_GCHeapHardLimit=0x12800000
ENV DOTNET_GCHighMemPercent=50
ENV DOTNET_EnableDiagnostics=0
ENV COMPlus_ThreadPool_ForceMinWorkerThreads=2

ENTRYPOINT ["dotnet", "PISO.WebApp.dll"]
