# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies as a separate layer to leverage Docker's build cache.
COPY RukuServiceApi/*.csproj ./RukuServiceApi/
COPY RukuServiceApi.Tests/*.csproj ./RukuServiceApi.Tests/
COPY MigrateTool/*.csproj ./MigrateTool/
RUN dotnet restore "RukuServiceApi/RukuServiceApi.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application for release, excluding the test project
WORKDIR "/src/RukuServiceApi"
RUN dotnet publish "RukuServiceApi.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80 for the Kestrel web server inside the container
EXPOSE 80

ENTRYPOINT ["dotnet", "RukuServiceApi.dll"]