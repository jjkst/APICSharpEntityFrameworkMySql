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

# Stage 2: Alpine runtime (~100MB smaller than Debian)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "RukuServiceApi.dll"]