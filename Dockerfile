# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RukuServiceApi/RukuServiceApi.csproj", "RukuServiceApi/"]
RUN dotnet restore "RukuServiceApi/RukuServiceApi.csproj"
COPY . .
WORKDIR "/src/RukuServiceApi"
RUN dotnet build "RukuServiceApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RukuServiceApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create log directory
RUN mkdir -p /var/log/ruku-service-api

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Environment variables for configuration (can be overridden at runtime)
ENV CONNECTIONSTRING=""
ENV JWT_SECRET_KEY=""
ENV JWT_ISSUER="RukuServiceApi"
ENV JWT_AUDIENCE="RukuServiceApiUsers"
ENV JWT_EXPIRATION_MINUTES="60"
ENV SMTP_SERVER=""
ENV SMTP_PORT="587"
ENV SMTP_USERNAME=""
ENV SMTP_PASSWORD=""
ENV ENABLE_SSL="true"
ENV ALLOWED_HOSTS="*"

# Create uploads directory
RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "RukuServiceApi.dll"]
