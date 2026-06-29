# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish api-sgc.csproj -c Release -o /app/publish --no-restore

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser
EXPOSE 5070

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
  CMD curl -sf http://localhost:5070/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "api-sgc.dll"]