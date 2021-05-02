FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY  "POC.OpenTelemetry.sln" "POC.OpenTelemetry.sln"

COPY "POC.OpenTelemetry.API/POC.OpenTelemetry.API.csproj", "POC.OpenTelemetry.API/POC.OpenTelemetry.API.csproj"
COPY "POC.OpenTelemetry.Worker/POC.OpenTelemetry.Worker.csproj" "POC.OpenTelemetry.Worker/POC.OpenTelemetry.Worker.csproj"
COPY "POC.OpenTelemetry.Grpc/POC.OpenTelemetry.Grpc.csproj", "POC.OpenTelemetry.Grpc/POC.OpenTelemetry.Grpc.csproj"

COPY "docker-compose.dcproj" "docker-compose.dcproj"

RUN dotnet restore "POC.OpenTelemetry.sln"

COPY . .
WORKDIR "/POC.OpenTelemetry.Grpc"
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC.OpenTelemetry.Grpc.dll"]
