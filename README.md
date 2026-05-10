# 🚀 ProjectHub

> A premium task management and collaboration platform built for seamless team coordination — featuring real-time communication, role-based dashboards, and an integrated LMS.

---

## ✨ Features

### 🔐 Secure Authentication
- JWT-based authentication with BCrypt password hashing
- Role-Based Access Control (RBAC) — Admin, Manager, Employee
- Legacy password auto-migration to secure format

### 📊 Role-Based Dashboards

| Role | Capabilities |
|------|-------------|
| 👨‍💼 **Admin** | System overview, user management, platform insights |
| 🧑‍💻 **Manager** | Assign & monitor tasks, team performance tracking |
| 👨‍💻 **Employee** | View assigned tasks, update progress, track deadlines |

### 💬 Collaboration Hub
- WhatsApp-style chat for task discussions
- Real-time activity alerts via SignalR
- Task assignment notifications with grouping
- File sharing support

### 🎓 LMS Integration
- Course management system
- Progress tracking
- Automatic certificate generation

### 💎 UI/UX
- Glassmorphism components with micro-animations
- Dynamic light/dark theme switching
- Responsive design across devices

---

## 🏗️ Architecture

```
PROJECT-HUB/
├── BACKEND/
│   └── ProjectHubAPI/
│       ├── Controllers/       # API endpoints
│       ├── DTOs/              # Data Transfer Objects
│       ├── Data/              # DbContext & migrations
│       ├── Hubs/              # SignalR real-time hubs
│       ├── Mapping/           # Mapster profiles
│       ├── Middleware/        # Custom middleware
│       ├── Models/            # Domain entities
│       ├── Services/          # Business logic
│       ├── Validators/        # FluentValidation
│       └── wwwroot/uploads/   # File storage
│
├── FRONTEND/
│   └── projecthub-ui/
│       └── src/app/
│           ├── components/    # UI components
│           ├── guards/        # Route guards
│           ├── interceptors/  # HTTP interceptors (JWT)
│           ├── models/        # TypeScript interfaces
│           ├── pipes/         # Custom pipes
│           ├── services/      # API services
│           └── shared/        # Shared modules
│
└── README.md
```

---

## ⚙️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular, TypeScript, CSS Animations |
| Backend | ASP.NET Core Web API |
| Auth | JWT + BCrypt + RBAC |
| Real-time | SignalR |
| ORM | Entity Framework Core |
| Mapping | Mapster |
| Validation | FluentValidation |
| Testing | xUnit (ProjectHubAPI.Tests) |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- SQL Server (or update connection string for your DB)

### Backend Setup

```bash
cd BACKEND/ProjectHubAPI
dotnet restore
dotnet ef database update
dotnet run
```

API runs at: `https://localhost:7001`

### Frontend Setup

```bash
cd FRONTEND/projecthub-ui
npm install
ng serve
```

App runs at: `http://localhost:4200`

---

## 🧪 Running Tests

```bash
cd BACKEND/ProjectHubAPI.Tests
dotnet test
```

---

## 👩‍💻 Author

**Sangamithra P**

---

