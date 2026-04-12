# Multi-stage build for .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore first for better caching
COPY ["BugPredictionBackend.csproj", "./"]
RUN dotnet restore "./BugPredictionBackend.csproj"

# Copy rest and publish
COPY . .
RUN dotnet publish "BugPredictionBackend.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# At runtime use the PORT env var provided by Render. If not set, default to 5000.
# We use a shell entrypoint so the env var is evaluated when the container starts.
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-5000} dotnet BugPredictionBackend.dll"]
