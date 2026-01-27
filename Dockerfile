# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY src/DataTouch.Domain/*.csproj ./src/DataTouch.Domain/
COPY src/DataTouch.Infrastructure/*.csproj ./src/DataTouch.Infrastructure/
COPY src/DataTouch.Web/*.csproj ./src/DataTouch.Web/
COPY src/DataTouch.Api/*.csproj ./src/DataTouch.Api/
COPY tests/DataTouch.Tests/*.csproj ./tests/DataTouch.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/DataTouch.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "DataTouch.Web.dll"]
