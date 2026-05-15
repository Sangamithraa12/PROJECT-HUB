# 🚀 ProjectHub

A full-stack enterprise-grade Project Management, Task Collaboration, and Employee Learning platform built using Angular and ASP.NET Core.

ProjectHub combines project tracking, real-time communication, employee collaboration, and LMS functionality into a single scalable platform designed with clean architecture principles and enterprise-level backend architecture.

---

# ✨ Features

## 📁 Project Management
- Create, update, and manage projects
- Assign managers and employees
- Track project progress and status
- Monitor project workflows efficiently

---

## ✅ Kanban Task Board
- Drag-and-drop task management
- Task prioritization and status tracking
- Employee task assignment
- Real-time task updates
- Agile workflow management

---

## 💬 Real-time Chat (SignalR)
- Instant messaging between users
- Real-time communication without refresh
- Live chat updates using SignalR hubs
- Faster team collaboration

---

## 🔔 Live Notifications
- Instant task assignment alerts
- Real-time status notifications
- SignalR-powered notification delivery
- Live activity updates

---

## 📊 Leaderboard
- Employee productivity tracking
- Performance-based ranking system
- Dynamic leaderboard updates
- Task completion analytics

---

## 🎓 Course Management (LMS)
- Upload and manage learning resources
- Employee course enrollment
- Training progress tracking
- Centralized learning platform

---

# 📊 Dashboards

## 👑 Admin Dashboard
- Manage users and roles
- Monitor projects and tasks
- Manage courses and LMS resources
- Track employee productivity
- View real-time notifications and updates

### Purpose
Provides complete control and monitoring over the entire platform.

---

## 👨‍💼 Manager Dashboard
- Create and manage projects
- Assign tasks to employees
- Monitor team performance
- Track project progress
- Access real-time chat and notifications

### Purpose
Helps managers efficiently coordinate teams and workflows.

---

## 👩‍💻 Employee Dashboard
- View assigned tasks
- Update task status and progress
- Access Kanban board
- Receive live notifications
- Access LMS learning resources

### Purpose
Helps employees manage daily work, collaboration, and learning activities.

---

# 🔐 Role-Based Access Control

### 👑 Admin
- Full system access
- Manage users, projects, tasks, and courses

### 👨‍💼 Manager
- Create and manage projects/tasks
- Assign work to employees
- Monitor team progress

### 👩‍💻 Employee
- View assigned tasks
- Update task status
- Access learning materials

---

# 🏛️ Architecture

```text
Angular Frontend
        ↓
JWT Authentication
        ↓
ASP.NET Core Controllers
        ↓
MediatR (CQRS)
        ↓
Service Layer
        ↓
Repository Layer
        ↓
Entity Framework Core
        ↓
SQL Server
```

---

# ⚙️ Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 17+ |
| Backend | ASP.NET Core Web API |
| Database | SQL Server Express |
| ORM | Entity Framework Core |
| Authentication | JWT Bearer Tokens |
| Real-time Communication | SignalR |
| Validation | FluentValidation |
| Mapping | Mapster |
| Mediator Pattern | MediatR |
| Testing | xUnit + Moq |

---

# 🏗️ Backend Architecture

The backend follows modern enterprise architecture patterns:

- Clean Architecture
- CQRS (Command Query Responsibility Segregation)
- Repository Pattern
- Service-Oriented Design
- Dependency Injection
- Global Exception Handling Middleware

### Benefits
- Scalable architecture
- Better maintainability
- Clean separation of concerns
- Easier testing and debugging

---

# 📂 Project Structure

```text
ProjectHub/
│
├── FRONTEND/
│   └── projecthub-ui/
│       │
│       ├── src/
│       │   ├── app/
│       │   │   ├── components/       → Reusable UI Components
│       │   │   ├── pages/            → Application Pages
│       │   │   ├── services/         → API Communication Services
│       │   │   ├── guards/           → Route Protection
│       │   │   ├── interceptors/     → JWT Token Interceptors
│       │   │   ├── models/           → Frontend Models & Interfaces
│       │   │   ├── shared/           → Shared Modules & Utilities
│       │   │   └── app.routes.ts     → Angular Routing
│       │   │
│       │   ├── assets/               → Images & Static Files
│       │   └── environments/         → Environment Configuration
│       │
│       ├── angular.json              → Angular Configuration
│       ├── package.json              → Dependencies & Scripts
│       └── tsconfig.json             → TypeScript Configuration
│
└── BACKEND/
    └── ProjectHubAPI/
        │
        ├── Controllers/              → API Endpoints
        ├── Features/                 → CQRS Commands & Queries
        ├── Repositories/             → Database Access Layer
        ├── Services/                 → Business Logic
        ├── DTOs/                     → Data Transfer Objects
        ├── Models/                   → Entity Models
        ├── Middleware/               → Global Exception Handling
        ├── Validators/               → FluentValidation Rules
        ├── Hubs/                     → SignalR Real-time Hubs
        ├── Mapping/                  → Mapster Configuration
        ├── Data/                     → DbContext & Migrations
        ├── Interfaces/               → Service & Repository Contracts
        ├── Authentication/           → JWT Authentication Logic
        └── Program.cs                → Startup Configuration
```

---

# 🔄 Application Workflow

```text
User Action
    ↓
Angular Frontend
    ↓
API Request with JWT Token
    ↓
ASP.NET Core Controllers
    ↓
MediatR (CQRS)
    ↓
Service Layer
    ↓
Repository Layer
    ↓
Entity Framework Core
    ↓
SQL Server Database
```

---

# 🔐 Authentication Flow

```text
1. User logs in
2. Backend validates credentials
3. JWT token generated
4. Angular stores token
5. Interceptor attaches token to requests
6. Backend validates token
7. Authorized APIs become accessible
```

---

# 📡 Real-time Communication

SignalR is used for:

- Real-time chat
- Live notifications
- Instant UI updates
- Task assignment alerts
- Dynamic updates without refresh

---

# 🛡️ Security Features

- JWT Stateless Authentication
- BCrypt Password Hashing
- Role-based Authorization
- Global Exception Middleware
- Secure API Endpoints

---

# 📊 Database

```text
Database: ProjectHubDB
Server: localhost\SQLEXPRESS
ORM: Entity Framework Core
Approach: Code-First Migrations
```

---

# 🚀 How to Run

## Backend

```bash
Open ProjectHubAPI in Visual Studio
Press F5
```

Backend runs on:

```text
https://localhost:10001
```

Swagger:

```text
https://localhost:10001/swagger
```

---

## Frontend

```bash
cd FRONTEND/projecthub-ui
npm install
npm start
```

Frontend runs on:

```text
http://localhost:4200
```

---

# 🧪 Run Unit Tests

```bash
dotnet test
```

---

# 🌟 Key Technical Highlights

- Enterprise-style layered architecture
- CQRS with MediatR
- Real-time SignalR communication
- JWT-based stateless authentication
- Repository pattern implementation
- Global exception handling middleware
- Unit testing with xUnit and Moq
- FluentValidation-based request validation
- Angular standalone component architecture

---

# 📌 Future Enhancements

- Docker containerization
- Redis caching
- Email notifications
- Azure deployment
- Microservices architecture
- AI-powered productivity analytics

---

# 👩‍💻 Developed By

### Sangamithra P

Full Stack Developer | ASP.NET Core | Angular | SQL Server | Real-time Applications
