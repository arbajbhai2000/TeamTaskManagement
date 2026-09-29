# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["TeamTaskManagement/TeamTaskManagementAPI.csproj", "TeamTaskManagement/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TeamTaskManagement/TeamTaskManagementAPI.csproj"

# Copy source code
COPY TeamTaskManagement/ TeamTaskManagement/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/

# Build
WORKDIR /src/TeamTaskManagement
RUN dotnet build "TeamTaskManagementAPI.csproj" -c Release -o /app/build --no-restore

# =========================
# Publish stage
# =========================
FROM build AS publish

RUN dotnet publish "TeamTaskManagementAPI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TeamTaskManagementAPI.dll"]