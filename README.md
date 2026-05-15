# 🚀 ProjectHub

A full-stack enterprise-grade Project Management, Task Collaboration, and Employee Learning platform built using Angular and ASP.NET Core.

ProjectHub combines project tracking, real-time communication, employee collaboration, and LMS functionality into a single scalable platform designed with clean architecture principles and enterprise-level backend architecture.

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

### Workflow Explanation

### 1️⃣ User Interaction
The user interacts with the Angular frontend by:
- Creating projects
- Updating tasks
- Sending chat messages
- Accessing LMS resources

---

### 2️⃣ Frontend Request
Angular services send HTTP requests to the ASP.NET Core backend API.

Example:
```ts
this.http.get('/api/projects');
```

---

### 3️⃣ JWT Authentication
The Angular interceptor automatically attaches the JWT token to every protected request.

```text
Authorization: Bearer <token>
```

---

### 4️⃣ Controller Layer
ASP.NET Core Controllers receive incoming requests and route them to the appropriate handlers.

---

### 5️⃣ CQRS with MediatR
MediatR separates operations into:

```text
Commands → Create / Update / Delete Operations
Queries  → Read Operations
```

### Benefits
- Better maintainability
- Clean architecture separation
- Scalable backend structure

---

### 6️⃣ Service Layer
The Service Layer handles:
- Business logic
- Validation
- Authorization checks
- Application rules

---

### 7️⃣ Repository Layer
Repositories interact with the database using Entity Framework Core.

### Benefits
- Loose coupling
- Easier testing
- Centralized data access

---

### 8️⃣ Database Operations
Entity Framework Core converts LINQ queries into SQL queries and communicates with SQL Server.

---

### 9️⃣ Response Returned
The processed response is returned back to Angular UI and updated instantly.

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

### Workflow
```text
To Do → In Progress → Testing → Completed
```

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

### Features
- Manage users and roles
- Monitor projects and tasks
- Manage LMS resources
- Track employee productivity
- View system-wide analytics
- Receive real-time notifications

### Purpose
Provides complete control and monitoring over the entire platform.

---

## 👨‍💼 Manager Dashboard

### Features
- Create and manage projects
- Assign tasks to employees
- Track project completion
- Monitor team productivity
- Access live chat and notifications

### Purpose
Helps managers coordinate teams and manage workflows efficiently.

---

## 👩‍💻 Employee Dashboard

### Features
- View assigned tasks
- Update task status and progress
- Access Kanban task board
- Receive real-time notifications
- Access learning resources

### Purpose
Helps employees manage daily tasks and learning activities.

---

# 📄 Pages & Modules Overview

## 🔐 Login Page (`/`)
### Purpose
Secure login page for Admins, Managers, and Employees.

### Features
- JWT Authentication
- Role-based access control
- Secure route protection

---

## 📊 Dashboard Page (`/dashboard`)
### Features
- Total projects overview
- Completed and pending task statistics
- Employee leaderboard
- Productivity analytics
- Real-time activity monitoring

---

## 📁 Projects Page (`/projects`)
### Features
- Create and manage projects
- Assign employees and managers
- Monitor project status
- Track project progress

---

## 📄 Project Details Page (`/projects/:id`)
### Features
- Detailed project information
- Team member tracking
- Project workflow monitoring
- Related task management

---

## ✅ Tasks Page / Kanban Board (`/tasks`)
### Features
- Drag-and-drop task updates
- Real-time synchronization
- Task prioritization
- Workflow tracking

---

## 👥 Users Page (`/users`)
### Features
- User management
- Role assignment
- Employee access control
- User information management

---

## 🎓 Courses Page (`/courses`)
### Features
- Browse available courses
- Course enrollment
- Learning resource access
- Employee training platform

---

## 📚 Course Details Page (`/courses/:id`)
### Features
- Video tutorials
- Study materials
- Learning modules
- Course progress tracking

---

## 📖 My Courses Page (`/my-courses`)
### Features
- View enrolled courses
- Track learning progress
- Continue training modules
- Completion percentage tracking

---

## 🏆 Certificates Page (`/certificates`)
### Features
- View earned certificates
- Achievement tracking
- Course completion rewards
- Employee recognition system

---

## 💬 Chat System
### Features
- One-to-one messaging
- Team communication
- Real-time live chat
- Instant message delivery

---

## 🔔 Notification System
### Features
- Task assignment alerts
- Deadline reminders
- Status change notifications
- Real-time activity updates

---

# 🔐 Role-Based Access Control

## 👑 Admin
- Full system access
- Manage users, projects, tasks, and courses

## 👨‍💼 Manager
- Create and manage projects/tasks
- Assign work to employees
- Monitor team progress

## 👩‍💻 Employee
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
