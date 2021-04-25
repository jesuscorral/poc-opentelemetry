#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["POC.OpenTelemetry.Grpc/POC.OpenTelemetry.Grpc.csproj", "POC.OpenTelemetry.Grpc/"]
RUN dotnet restore "POC.OpenTelemetry.Grpc/POC.OpenTelemetry.Grpc.csproj"
COPY . .
WORKDIR "/src/POC.OpenTelemetry.Grpc"
RUN dotnet build "POC.OpenTelemetry.Grpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "POC.OpenTelemetry.Grpc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC.OpenTelemetry.Grpc.dll"]
