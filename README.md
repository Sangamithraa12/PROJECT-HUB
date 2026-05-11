# 🚀 ProjectHub

A full-stack enterprise-grade Project Management, Task Collaboration, and Employee Learning platform built using Angular and ASP.NET Core.

ProjectHub combines project tracking, real-time communication, employee collaboration, and LMS functionality into a single scalable platform designed with clean architecture principles and enterprise-grade backend patterns.

---

# ✨ Features

## 📁 Project Management
- Create, update, and manage projects
- Assign managers and employees
- Track project progress and status

---

## ✅ Kanban Task Board
- Drag-and-drop task management
- Task prioritization and status tracking
- Employee task assignment
- Real-time task updates

---

## 💬 Real-time Chat (SignalR)
- Instant messaging between users
- Real-time communication without refresh
- Live chat updates using SignalR hubs

---

## 🔔 Live Notifications
- Instant task assignment alerts
- Real-time status notifications
- SignalR-powered notification delivery

---

## 📊 Leaderboard
- Employee productivity tracking
- Performance-based ranking system
- Dynamic leaderboard updates

---

## 🎓 Course Management (LMS)
- Upload and manage learning resources
- Employee course enrollment
- Training progress tracking

---

## 🔐 Role-Based Access Control

### Admin
- Full system access
- Manage users, projects, tasks, and courses

### Manager
- Create and manage projects/tasks
- Assign work to employees

### Employee
- View assigned tasks
- Update task status
- Access learning materials

---

## 🧪 Unit Testing
- xUnit testing framework
- Moq for dependency mocking
- Service-layer unit testing

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
Repository Layer
        ↓
Entity Framework Core
        ↓
SQL Server
```

---

# ⚙️ Tech Stack

| Layer                   | Technology            |
| ----------------------- | --------------------- |
| Frontend                | Angular 17+           |
| Backend                 | ASP.NET Core Web API  |
| Database                | SQL Server Express    |
| ORM                     | Entity Framework Core |
| Authentication          | JWT Bearer Tokens     |
| Real-time Communication | SignalR               |
| Validation              | FluentValidation      |
| Mapping                 | Mapster               |
| Mediator Pattern        | MediatR               |
| Testing                 | xUnit + Moq           |

---

# 🏗️ Backend Architecture

The backend follows:

* Clean Architecture
* CQRS (Command Query Responsibility Segregation)
* Repository Pattern
* Service-Oriented Design
* Dependency Injection
* Global Exception Handling Middleware

---

# 📂 Project Structure

```text
BACKEND/
│
├── Controllers/        → API Endpoints
├── Features/           → CQRS Commands & Queries
├── Repositories/       → Database Access Layer
├── Services/           → Business Logic
├── DTOs/               → Data Transfer Objects
├── Models/             → Entity Models
├── Middleware/         → Global Exception Handling
├── Validators/         → FluentValidation Rules
├── Hubs/               → SignalR Real-time Hubs
├── Mapping/            → Mapster Configuration
├── Data/               → DbContext & Migrations
└── Program.cs          → Startup Configuration
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

* Real-time chat
* Live notifications
* Instant UI updates
* Task assignment alerts

---

# 🛡️ Security Features

* JWT Stateless Authentication
* BCrypt Password Hashing
* Role-based Authorization
* Global Exception Middleware
* Secure API Endpoints

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

* Enterprise-style layered architecture
* CQRS with MediatR
* Real-time SignalR communication
* JWT-based stateless authentication
* Global exception middleware
* Repository pattern implementation
* Unit testing with xUnit and Moq
* FluentValidation-based request validation
* Angular standalone component architecture

---

# 📌 Future Enhancements

* Docker containerization
* Redis caching
* Email notifications
* Azure deployment
* Microservices architecture
* AI-powered productivity analytics

---

# 👩‍💻 Developed By

### Sangamithra P

Full Stack Developer | ASP.NET Core | Angular | SQL Server | Real-time Applications
