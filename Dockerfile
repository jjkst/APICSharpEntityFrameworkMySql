# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY RukuServiceApi/*.csproj ./RukuServiceApi/
COPY RukuServiceApi.Tests/*.csproj ./RukuServiceApi.Tests/
COPY MigrateTool/*.csproj ./MigrateTool/
RUN dotnet restore "RukuServiceApi/RukuServiceApi.csproj"

COPY . .
WORKDIR "/src/RukuServiceApi"
RUN dotnet publish "RukuServiceApi.csproj" -c Release -o /app/publish --no-restore

# Build EF Core migration bundle (standalone executable, no SDK needed at runtime)
WORKDIR /src
RUN dotnet tool install --global dotnet-ef --version 8.0.*
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project RukuServiceApi/RukuServiceApi.csproj \
    --startup-project RukuServiceApi/RukuServiceApi.csproj \
    -o /app/efbundle --force

# Stage 2: Migration runner (runs once, applies pending migrations, exits)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS migrate
WORKDIR /app
COPY --from=build /app/efbundle ./efbundle
COPY --from=build /src/RukuServiceApi/appsettings.json ./
RUN chmod +x ./efbundle
ENTRYPOINT ["./efbundle"]

# Stage 3: API runtime (~100MB smaller than Debian)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS api
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "RukuServiceApi.dll"]