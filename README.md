# Team Task Management

A full-stack team and task management application built with **.NET 8 Web API, React, SQL Server, Entity Framework Core, JWT Authentication, and Docker**.

The application provides role-based access for **Admin, Manager, and User** with team management, task assignment, task status tracking, comments, notifications, dashboard reporting, and secure authentication.

---

## 1. Features

### Authentication & Authorization

* User registration
* User login
* JWT access token authentication
* Refresh token support
* Role-based authorization
* Protected API endpoints
* Secure password hashing
* Token expiration handling

### Role-Based Access Control

The application supports three roles:

| Role    | Capabilities                                           |
| ------- | ------------------------------------------------------ |
| Admin   | Manage users, teams, tasks and system-level operations |
| Manager | Manage teams and assigned team tasks                   |
| User    | View and manage assigned tasks                         |

Additional security rules:

* Public registration always creates a **User**
* Only authenticated Admin users can create Admin, Manager, or User accounts
* Multiple Admin users are supported
* The application prevents deletion/demotion of the last Admin
* Users can only access operations permitted by their role

---

## 2. Task Management

Users can work with tasks using the following statuses:

* To Do
* In Progress
* Done

Task functionality includes:

* Create tasks
* Assign tasks to team members
* Update task status
* Set task priority
* Set due dates
* View task details
* Add comments
* Track task progress
* View assigned tasks
* Filter tasks by status and priority

---

## 3. Notifications

The application provides notifications for important task activities.

Examples:

* Task assignment
* Task status changes
* Task-related activity

Users can:

* View their notifications
* Mark notifications as read

---

## 4. Dashboard

The dashboard provides an overview of workspace activity.

It includes:

* Total tasks
* To Do tasks
* In Progress tasks
* Completed tasks
* Overdue tasks
* Task completion percentage
* My Tasks
* Recent Activity
* Notifications
* Task progress

The dashboard is role-aware and displays functionality according to the logged-in user's role.

---

## 5. Technology Stack

### Backend

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* C#
* JWT Authentication
* Swagger / OpenAPI
* REST APIs

### Frontend

* React
* Vite
* JavaScript
* Axios
* React Router
* CSS

### Testing

* xUnit
* Moq
* WebApplicationFactory

### DevOps / Deployment

* Docker
* Docker Compose
* Nginx

---

## 6. Architecture

The backend follows a **Clean Architecture** approach.

```text
TeamTaskManagement
│
├── API
├── Application
├── Domain
├── Infrastructure
├── API.Tests
├── Application.Tests
├── Infrastructure.Tests
│
└── Frontend
    └── team-task-management-ui
```

### Domain

Contains the core business entities and domain models.

Examples:

* User
* Team
* TeamMember
* Task
* Comment
* Notification
* RefreshToken

### Application

Contains application business logic.

Examples:

* Services
* DTOs
* Interfaces
* Business rules

### Infrastructure

Contains external implementation details.

Examples:

* Entity Framework Core
* SQL Server
* Repository implementations
* Database migrations

### API

Contains:

* Controllers
* Authentication configuration
* Dependency injection
* Middleware
* Swagger configuration
* Application startup

---

## 7. Database

The application uses **Microsoft SQL Server** with **Entity Framework Core Code First**.

Database:

```text
TeamTaskManagementDB
```

Main tables:

```text
Users
Teams
TeamMembers
Tasks
Comments
Notifications
RefreshTokens
__EFMigrationsHistory
```

Entity Framework Core migrations are stored under:

```text
Infrastructure/Migrations
```

To apply migrations:

```powershell
dotnet ef database update --project Infrastructure --startup-project TeamTaskManagement
```

---

## 8. API Documentation

Swagger/OpenAPI is available when the API is running.

Local API:

```text
http://localhost:5298
```

Swagger:

```text
http://localhost:5298/swagger
```

Swagger provides an interactive interface for testing the REST APIs.

---

## 9. Frontend

The React frontend is located at:

```text
Frontend/team-task-management-ui
```

The frontend communicates with the backend through REST APIs.

API base URL:

```text
http://localhost:5298/api
```

Authentication tokens are stored in browser local storage:

```text
accessToken
refreshToken
expiresAt
```

Axios automatically attaches the JWT access token to authenticated API requests.

---

## 10. Running the Application Locally

### Backend

Navigate to the project:

```powershell
cd TeamTaskManagement
```

Run the API:

```powershell
dotnet run
```

The API runs on:

```text
http://localhost:5298
```

### Frontend

Open another terminal:

```powershell
cd Frontend/team-task-management-ui
```

Install dependencies:

```powershell
npm install
```

Run the frontend:

```powershell
npm run dev
```

The frontend is available at:

```text
http://localhost:5173
```

---

# 11. Running with Docker

The project includes Docker Compose configuration for the complete application.

The Docker environment contains:

```text
React Frontend
      │
      ▼
.NET 8 API
      │
      ▼
SQL Server
```

### Start all containers

From the project root:

```powershell
docker compose up -d
```

### Check running containers

```powershell
docker compose ps
```

Expected services:

```text
teamtaskmanagement-api
teamtaskmanagement-frontend
teamtaskmanagement-sqlserver
```

### Application URLs

Frontend:

```text
http://localhost:5173
```

API:

```text
http://localhost:5298
```

Swagger:

```text
http://localhost:5298/swagger
```

### Stop containers

```powershell
docker compose down
```

### Stop containers and remove database volume

> Warning: this removes the Docker SQL Server data.

```powershell
docker compose down -v
```

---

## 12. Docker Services

### SQL Server

Container:

```text
teamtaskmanagement-sqlserver
```

Port:

```text
1433
```

Database:

```text
TeamTaskManagementDB
```

### API

Container:

```text
teamtaskmanagement-api
```

Container port:

```text
8080
```

Host port:

```text
5298
```

### Frontend

Container:

```text
teamtaskmanagement-frontend
```

Container port:

```text
80
```

Host port:

```text
5173
```

---

## 13. Docker Database Persistence

SQL Server uses a Docker named volume:

```text
sqlserver_data
```

This allows database data to remain available when containers are restarted.

---

## 14. Authentication Flow

The authentication process works as follows:

```text
User
 │
 ├── Register
 │
 ▼
ASP.NET Core API
 │
 ▼
Password Hashing
 │
 ▼
SQL Server
```

For login:

```text
User Login
    │
    ▼
Auth API
    │
    ▼
Validate Email & Password
    │
    ▼
Generate JWT Access Token
    │
    ▼
Generate Refresh Token
    │
    ▼
React Frontend
    │
    ▼
Authenticated API Requests
```

The JWT contains user information including:

* User ID
* Name
* Email
* Role

---

## 15. Role-Based UI

The frontend uses the authenticated user's role to control available functionality.

### Admin

Can access:

* Dashboard
* My Tasks
* Teams
* Comments
* Notifications
* Settings

### Manager

Can access:

* Dashboard
* My Tasks
* Teams
* Comments
* Notifications
* Settings

### User

Can access:

* Dashboard
* My Tasks
* Comments
* Notifications
* Settings

Users do not receive Admin/Manager-only team management functionality.

Authorization is enforced on the backend as well, so hiding frontend controls is not the only security mechanism.

---

## 16. API Security

The application implements:

* JWT Bearer authentication
* Role-based authorization
* Password hashing
* Refresh tokens
* Token expiration
* Protected API endpoints
* Server-side authorization checks
* Input validation

The API should never rely only on frontend authorization checks.

---

## 17. Testing

The solution contains separate test projects:

```text
API.Tests
Application.Tests
Infrastructure.Tests
```

Testing technologies:

* xUnit
* Moq
* ASP.NET Core WebApplicationFactory

Tests cover application functionality including authentication, authorization, services, repositories, and API behavior.

Run the complete test suite with:

```powershell
dotnet test
```

---

## 18. Project Structure

```text
TeamTaskManagement
│
├── TeamTaskManagement
│   ├── Controllers
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Dockerfile
│   └── TeamTaskManagementAPI.csproj
│
├── Application
│   ├── DTOs
│   ├── Interfaces
│   └── Services
│
├── Domain
│   └── Entities
│
├── Infrastructure
│   ├── Data
│   ├── Repositories
│   └── Migrations
│
├── API.Tests
├── Application.Tests
├── Infrastructure.Tests
│
├── Frontend
│   └── team-task-management-ui
│       ├── src
│       ├── public
│       ├── Dockerfile
│       ├── package.json
│       └── vite.config.js
│
├── docker-compose.yml
├── .dockerignore
├── README.md
└── TeamTaskManagement.sln
```

---

## 19. Demo Account

For a fresh Docker environment, a user can register through:

```text
POST /api/Auth/register
```

Example:

```json
{
  "name": "Docker Test",
  "email": "dockertest@test.com",
  "password": "Test@12345"
}
```

Public registration creates the account as a **User**.

For Admin and Manager demonstrations, an Admin account can create users with the required roles through the application's authorized user-management functionality.

---

## 20. Environment Configuration

### Local SQL Server

The local environment uses the SQL Server instance configured in:

```text
TeamTaskManagement/appsettings.json
```

### Docker SQL Server

Docker Compose overrides the database connection using:

```text
ConnectionStrings__DefaultConnection
```

The Docker API connects to:

```text
sqlserver,1433
```

rather than:

```text
localhost
```

This is required because the API runs inside its own Docker container.

---

## 21. Useful Docker Commands

View all containers:

```powershell
docker compose ps
```

View API logs:

```powershell
docker compose logs api
```

View API logs continuously:

```powershell
docker compose logs -f api
```

View SQL Server logs:

```powershell
docker compose logs sqlserver
```

View frontend logs:

```powershell
docker compose logs frontend
```

Restart API:

```powershell
docker compose restart api
```

Restart all services:

```powershell
docker compose restart
```

Rebuild images:

```powershell
docker compose build
```

Rebuild and start:

```powershell
docker compose up -d --build
```

---

## 22. Development Workflow

Recommended development workflow:

```text
1. Modify code
      ↓
2. Build application
      ↓
3. Run tests
      ↓
4. Run database migrations if required
      ↓
5. Build Docker images
      ↓
6. Start Docker Compose
      ↓
7. Verify API through Swagger
      ↓
8. Verify frontend
```

---

## 23. Future Improvements

Potential future enhancements include:

* CI/CD pipeline
* Centralized production logging
* Redis caching
* Email notifications
* Advanced reporting
* File attachments
* Real-time notifications using SignalR
* Production secrets management
* Cloud deployment
* Automated database migration during deployment

---

## 24. Author

**Arbaj Mujawar**

.NET Full Stack Developer

Technologies:

```text
C#
.NET 8
ASP.NET Core Web API
Entity Framework Core
SQL Server
React
JavaScript
Docker
REST API
JWT
Clean Architecture
```

---

## 25. License

This project was created for assessment and demonstration purposes.
