# 🚀 RukuServiceApi

A robust, secure, and scalable ASP.NET Core Web API for service management with comprehensive authentication, authorization, monitoring, and production-ready features.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Security](#security)
- [Monitoring & Health Checks](#monitoring--health-checks)
- [Database Management](#database-management)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

RukuServiceApi is a comprehensive service management backend API built with ASP.NET Core 8.0, designed to handle service management, user authentication, file uploads, and more. The API follows enterprise-grade patterns and includes production-ready features like health checks, structured logging, input validation, and security best practices.

### Key Capabilities
- **Service Management** - Create, read, update, and delete services with pricing plans
- **User Authentication** - JWT-based authentication with role-based authorization
- **File Upload** - Secure file upload with validation and security measures
- **Email Integration** - Contact form handling with SMTP integration
- **Health Monitoring** - Comprehensive health checks and system monitoring
- **Database Management** - Entity Framework migrations and data seeding

## ✨ Features

### 🔐 Security Features
- **JWT Authentication** - Secure token-based authentication
- **Role-based Authorization** - Admin, Owner, and Subscriber roles
- **Input Validation** - Comprehensive validation using FluentValidation
- **Secure File Upload** - File type validation, size limits, and path security
- **Security Headers** - Protection against common web vulnerabilities
- **Environment-based Configuration** - Secure secrets management

### 📊 Monitoring & Observability
- **Health Checks** - Database, email service, file system, and memory monitoring
- **Structured Logging** - Serilog with file and console output
- **Request/Response Logging** - Full request tracking with correlation IDs
- **Performance Metrics** - System information and performance monitoring
- **Error Handling** - Global exception handling with consistent responses

### 🗄️ Database Features
- **Entity Framework Core** - Modern ORM with MySQL support
- **Database Migrations** - Version-controlled schema management
- **Data Seeding** - Automatic initial data population
- **Connection Pooling** - Optimized database connections

### 🚀 Production Features
- **Docker Support** - Containerized deployment ready
- **Environment Configuration** - Development, staging, and production configs
- **CORS Configuration** - Proper cross-origin resource sharing
- **API Documentation** - Swagger/OpenAPI documentation
- **Migration Scripts** - Automated database management

## 🏗️ Architecture

### Project Structure
```
RukuServiceApi/
├── Controllers/           # API Controllers
│   ├── AuthController.cs         # Authentication endpoints
│   ├── ServicesController.cs     # Service management
│   ├── UsersController.cs        # User management
│   ├── EmailController.cs        # Email functionality
│   ├── UploadImageController.cs  # File upload
│   ├── PublicServicesController.cs # Public service access
│   ├── MonitoringController.cs   # System monitoring
│   └── BaseController.cs         # Base controller with CRUD operations
├── Models/               # Data Models
│   ├── User.cs                   # User entity
│   ├── Service.cs                # Service entity
│   ├── RukuService.cs           # Ruku service entity
│   ├── Availability.cs          # Availability entity
│   ├── Schedule.cs              # Schedule entity
│   ├── Contact.cs               # Contact entity
│   ├── EmailSettings.cs         # Email configuration
│   ├── JwtSettings.cs          # JWT configuration
│   ├── FileUploadSettings.cs   # File upload configuration
│   └── ValidationModels.cs     # Request/response DTOs
├── Services/             # Business Logic Services
│   ├── AuthService.cs           # Authentication service
│   ├── FileUploadService.cs     # File upload service
│   └── DatabaseSeeder.cs        # Database seeding service
├── Middleware/           # Custom Middleware
│   ├── GlobalExceptionMiddleware.cs    # Global error handling
│   ├── ValidationMiddleware.cs         # Request validation
│   ├── RequestLoggingMiddleware.cs     # Request logging
│   └── SecurityHeadersMiddleware.cs   # Security headers
├── HealthChecks/         # Health Check Implementations
│   └── CustomHealthChecks.cs    # Custom health checks
├── Validators/           # Input Validation
│   └── Validators.cs            # FluentValidation validators
├── Context/              # Database Context
│   └── ApplicationDbContext.cs # EF Core context
└── Program.cs            # Application startup
```

### Technology Stack
- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - Object-relational mapping
- **MySQL** - Database with Pomelo.EntityFrameworkCore.MySql
- **JWT Bearer Authentication** - Token-based authentication
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- MySQL 8.0+
- Docker (optional, for containerized deployment)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/jjkst/RukuServiceApi.git
   cd RukuServiceApi
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure environment variables**
   ```bash
   cp env.template .env.local
   cp docker.env.template .env.docker   # optional: for Docker Compose
   ```
   - Update the files with your own values (never commit them).
   - **Important**: `ALLOWED_HOSTS` must use semicolons (`;`) to separate hosts, not commas.
   - Export the variables: `export $(grep -v '^#' .env.local | xargs)`.
   - For the first run, create your development database in MySQL (`CREATE DATABASE RukuITServicesTest CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`).

4. **Run database migrations**
   ```bash
   # Make migration tool executable (one-time setup)
   chmod +x migrate
   
   # Create initial migration
   ./migrate create InitialCreate
   
   # Update database
   ./migrate update Development

   # The database already has tables, likely from a previous migration or manual SQL. Use the reset command to drop and recreate it:
   ./migrate reset Development
   ```
5. **Run the application**
   ```bash
   dotnet run --project RukuServiceApi
   ```

6. **Access the API**
   - API: `http://localhost:5002`
   - Swagger UI: `http://localhost:5002/swagger`
   - Health Check: `http://localhost:5002/health`

### Run with Docker

1. **Prepare environment files**
   ```bash
   cp env.template .env.local
   cp docker.env.template .env.docker
   ```
   - Populate both files with your credentials. `.env.local` is used for local development; `.env.docker` is read by Docker Compose.
   - **Important**: For Docker Compose, update `.env.docker`:
     - **Development**: Set `CONNECTIONSTRING=Server=mysql-dev;Port=3306;Database=RukuITServicesTest;User=root;Password=dev-password;`
     - **Production**: Set `CONNECTIONSTRING=Server=mysql;Port=3306;Database=RukuITServicesProd;User=root;Password=your-password;`
   - `ALLOWED_HOSTS` must use semicolons (`;`) to separate hosts: `localhost;127.0.0.1;your-domain.com`
   - Set `MYSQL_ROOT_PASSWORD` in `.env.docker` to match your connection string password.

2. **Run database migrations in Docker**
   ```bash
   # Option 1: Run migrations inside the container (recommended)
   docker-compose -f docker-compose.dev.yml up -d mysql-dev
   docker-compose exec ruku-service-api-dev dotnet ef database update
   
   # Option 2: Run from host (requires connection string pointing to localhost:3307)
   export CONNECTIONSTRING="Server=127.0.0.1;Port=3307;Database=RukuITServicesTest;User=root;Password=dev-password;"
   ./migrate.sh update Development
   ```

3. **Bring up the full stack**
   ```bash
   docker-compose -f docker-compose.dev.yml up --build
   ```
   - The API waits for MySQL to be healthy before starting (healthcheck enabled).
   - API is exposed on `http://localhost:5002`.
   - MySQL runs on port `3307` (dev) or `3306` (prod) with credentials from `.env.docker`.
   - Logs are stored in `./logs`, uploaded files in `./uploads` (both mounted as volumes).

4. **Tear the stack down**
   ```bash
   # Stop containers
   docker-compose -f docker-compose.dev.yml down
   
   # Stop and remove volumes (clean slate)
   docker-compose -f docker-compose.dev.yml down -v
   ```

5. **Run just the API container (optional)**
   ```bash
   docker build -t ruku-service-api .
   export $(grep -v '^#' .env.local | xargs)
   docker run -p 5000:80 \
     --env-file .env.local \
     -v "$(pwd)/logs:/var/log/ruku-service-api" \
     -v "$(pwd)/uploads:/app/uploads" \
     ruku-service-api
   ```
   Point the container's `CONNECTIONSTRING` at an existing database or MySQL instance.


#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "uid": "unique-user-id",
  "displayName": "John Doe",
  "emailVerified": true,
  "provider": "Google"
}
```

#### Login User
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "uid": "unique-user-id"
}
```

### Service Management Endpoints

#### Get All Services (Public)
```http
GET /api/publicservices
```

#### Create Service (Admin/Owner)
```http
POST /api/services
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Web Development",
  "description": "Professional web development services",
  "features": ["Responsive Design", "SEO Optimization"],
  "pricingPlans": [
    {
      "name": "Basic",
      "initialSetupFee": "$500",
      "monthlySubscription": "$200",
      "features": ["Basic Website", "Mobile Responsive"]
    }
  ]
}
```

#### Update Service (Admin/Owner)
```http
PUT /api/services/{id}
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Updated Service Title",
  "description": "Updated description"
}
```

### User Management Endpoints

#### Update User Role (Admin Only)
```http
PUT /api/users/{id}/role
Authorization: Bearer <jwt-token>
Content-Type: application/json

"Admin"
```

### File Upload Endpoints

#### Upload File (Admin/Owner)
```http
POST /api/uploadimage
Authorization: Bearer <jwt-token>
Content-Type: multipart/form-data

file: <file>
folder: "images"
```

### Health Check Endpoints

#### Complete Health Check
```http
GET /health
```

#### Readiness Probe
```http
GET /health/ready
```

#### Liveness Probe
```http
GET /health/live
```

### Monitoring Endpoints (Admin Only)

#### System Information
```http
GET /api/monitoring/system-info
Authorization: Bearer <jwt-token>
```

#### Performance Metrics
```http
GET /api/monitoring/performance
Authorization: Bearer <jwt-token>
```

## 🔒 Security

### Authentication & Authorization
- **JWT Tokens** - Secure token-based authentication
- **Role-based Access** - Admin, Owner, and Subscriber roles
- **Protected Endpoints** - Sensitive operations require authentication
- **Token Validation** - Comprehensive JWT validation

### Input Validation
- **FluentValidation** - Server-side validation
- **Data Annotations** - Model validation
- **Sanitization** - Input sanitization and validation
- **File Upload Security** - File type and size validation

### Security Headers
- **X-Content-Type-Options** - Prevent MIME type sniffing
- **X-Frame-Options** - Prevent clickjacking
- **X-XSS-Protection** - XSS protection
- **Referrer-Policy** - Control referrer information
- **Strict-Transport-Security** - HTTPS enforcement

### Environment Security
- **Secrets Management** - All sensitive data stored in environment variables
- **Configuration Separation** - Environment-specific configurations with secure defaults
- **No Hardcoded Secrets** - Database passwords, API keys, and JWT secrets externalized
- **CORS Configuration** - Proper cross-origin resource sharing
- **Production Hardening** - Swagger disabled in production, restrictive logging

## 📊 Monitoring & Health Checks

### Health Check Components
- **Database Health** - Connection and query validation
- **Email Service Health** - SMTP configuration validation
- **File System Health** - Upload directory accessibility
- **Memory Health** - Memory usage monitoring
- **Entity Framework Health** - Database context validation

### Logging
- **Structured Logging** - JSON-formatted logs with Serilog
- **Request Tracking** - Correlation IDs for request tracing
- **Performance Logging** - Request duration and performance metrics
- **Error Logging** - Comprehensive error tracking
- **File Rotation** - Automatic log file rotation

### Monitoring Endpoints
- **System Information** - Application and system details
- **Performance Metrics** - Memory, CPU, and GC statistics
- **Log Access** - Recent application logs
- **Garbage Collection** - Manual GC trigger for testing

## 🗄️ Database Management

### Migrations
The project includes a C# migration tool for easy database management:

```bash
# Using the wrapper script (recommended)
./migrate create AddNewFeature
./migrate update Development
./migrate reset Development
./migrate script InitialCreate migration.sql
./migrate rollback PreviousMigration
./migrate drop Development
./migrate status

# Or directly with dotnet
dotnet run --project MigrateTool -- create AddNewFeature
dotnet run --project MigrateTool -- update Development
```

**Available Commands:**
- `create <name>` - Create a new migration
- `update [env]` - Update database (Development/Production)
- `script [from] [output]` - Generate SQL script
- `rollback <name>` - Rollback to specific migration
- `remove` - Remove last migration (not applied)
- `status` - Show migration status
- `drop [env]` - Drop database for environment
- `reset [env]` - Drop then apply migrations for environment

The tool automatically loads environment variables from `.env.local` if present.

### Data Seeding
Automatic data seeding includes:
- **Admin User** - Default administrator account
- **Owner User** - Default business owner account
- **Sample Services** - Web development and mobile app services
- **Pricing Plans** - Sample pricing structures

### Database Schema
- **Users** - User accounts with roles and authentication
- **Services** - Service offerings with features and pricing
- **RukuServices** - Specialized service offerings
- **Availabilities** - Service availability schedules
- **Schedules** - Customer appointment scheduling

## 🚀 Deployment

### Docker Compose

**Development:**
```bash
# Start the stack
docker-compose -f docker-compose.dev.yml up --build

# Run migrations (if needed)
docker-compose exec ruku-service-api-dev dotnet ef database update

# Check status
docker-compose -f docker-compose.dev.yml ps

# View logs
docker-compose -f docker-compose.dev.yml logs -f
```

**Production:**
```bash
# Start the stack
docker-compose -f docker-compose.prod.yml up --build

# Run migrations (if needed)
docker-compose exec ruku-service-api dotnet ef database update

# Check status
docker-compose -f docker-compose.prod.yml ps
```

**Key Points:**
- Use `.env.docker` for Docker Compose deployments (configured via `env_file`).
- Development: API on port `5002`, MySQL on port `3307`
- Production: API on port `5000`, MySQL on port `3306`
- Logs are stored in `./logs` and uploads in `./uploads` (both mounted as volumes).
- MySQL healthchecks ensure the API waits for the database to be ready.
- Connection strings must use service names: `mysql-dev` (dev) or `mysql` (prod).

### Manual Docker Run

```bash
docker build -t ruku-service-api .
export $(grep -v '^#' .env.local | xargs)
docker run -p 5000:80 \
  --env-file .env.local \
  -v "$(pwd)/logs:/var/log/ruku-service-api" \
  -v "$(pwd)/uploads:/app/uploads" \
  ruku-service-api
```

### Production Deployment Checklist
- [ ] Copy `docker.env.template` to `.env.docker` and populate with production values
- [ ] Set `CONNECTIONSTRING` with `Server=mysql` (Docker service name)
- [ ] Set `ALLOWED_HOSTS` with semicolons: `yourdomain.com;www.yourdomain.com`
- [ ] Set `MYSQL_ROOT_PASSWORD` to match your connection string password
- [ ] Configure production secrets in your hosting platform or orchestrator
- [ ] Provision the production MySQL database and run migrations:
  ```bash
  docker-compose -f docker-compose.prod.yml exec ruku-service-api dotnet ef database update
  ```
- [ ] Enable HTTPS (certificates, reverse proxy, or load balancer)
- [ ] Configure log aggregation, backups, and monitoring alerts
- [ ] Verify `/health`, `/health/ready`, and `/health/live` after deployment
- [ ] Automate deployments via CI/CD (e.g., GitHub Actions)

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RukuServiceApi.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```
