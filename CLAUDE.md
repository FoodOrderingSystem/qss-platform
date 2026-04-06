# Claude Instructions — QSS Platform (Quality Management System)

## 🧠 Project Overview
This is a **production-ready MVP** for a **Dental Quality Management System (QSS Platform)** built with:

- .NET 8 Clean Architecture
- ASP.NET Core Web API + Razor Pages
- Entity Framework Core
- SignalR (real-time)
- SQLite (dev) → SQL Server/PostgreSQL (prod)

The system includes:
- RBAC (Superadmin, Admin, Dentist, Assistant, Trainee)
- Task & Process Management
- Device & Inventory Tracking
- Medications & Materials
- Chat + Notifications (SignalR)
- Learning System
- Reports & Dashboard (QSS Score)

---

# ⚠️ CRITICAL RULES (DO NOT BREAK)

## 1. NEVER break existing functionality
- Do NOT remove or refactor existing logic unless explicitly asked
- Do NOT rename existing public APIs, DTOs, or database fields
- All changes must be **backwards compatible**

## 2. FOLLOW CLEAN ARCHITECTURE STRICTLY

Layer responsibilities:

### Domain (`QSS.Domain`)
- Entities, Enums, Value Objects
- NO dependencies on other layers
- No business logic that depends on infrastructure

### Application (`QSS.Application`)
- DTOs
- Interfaces (Services)
- Business logic
- Validation

### Infrastructure (`QSS.Infrastructure`)
- EF Core implementation
- External services
- Repositories

### API (`QSS.API`)
- Controllers only
- No business logic
- Calls Application layer

### Web (`QSS.Web`)
- Razor Pages UI
- Alpine.js for interactivity
- Tailwind for styling

❗ NEVER mix responsibilities between layers

---

## 3. DATABASE RULES (EF CORE)

- Always use migrations for schema changes
- Do NOT drop or recreate database
- Respect existing relationships
- Use soft-delete pattern if deleting data
- Maintain compatibility with SQLite AND SQL Server

---

## 4. CODE QUALITY

- Write **production-ready code only**
- No pseudo code
- No TODO comments unless explicitly needed
- No “example” implementations
- Use proper async/await everywhere
- Follow existing naming conventions

---

## 5. SECURITY

- Respect RBAC roles
- Do NOT expose sensitive data
- All endpoints must validate authorization
- JWT authentication must remain intact

---

## 6. SIGNALR

- Do NOT break existing hubs:
  - `/hubs/chat`
  - `/hubs/notifications`
- Ensure real-time updates still function

---

## 7. DO NOT MODIFY WITHOUT ASKING

- Authentication system
- JWT configuration
- Docker setup
- Seeder logic
- Core architecture structure

---

# 🛠️ HOW TO IMPLEMENT FEATURES

When asked to implement a feature:

## Step 1 — Understand scope
- Identify affected modules
- Identify entities involved

## Step 2 — Backend first

### Domain
- Add/update entities if needed

### Application
- Create DTOs
- Define service interfaces
- Add business logic

### Infrastructure
- Implement services
- Update DbContext
- Add migrations (if needed)

### API
- Add controller endpoints

---

## Step 3 — Frontend (Razor Pages)

- Add UI in `QSS.Web`
- Use Alpine.js for interactivity
- Use Tailwind for styling
- Keep UI clean and minimal

---

## Step 4 — Integration

- Ensure API + UI work together
- Ensure SignalR updates if needed
- Ensure role-based access

---

# 📦 PR CREATION RULES

When creating a PR:

## PR Title
Clear and structured:
[Feature] Add device maintenance scheduling
[Fix] Resolve task status update bug
[Improvement] Optimize dashboard queries


## PR Description must include:
- What was implemented
- Affected modules
- Database changes (if any)
- How to test

---

# 🧪 TESTING

Before finalizing:

- Ensure API endpoints work
- Ensure UI renders correctly
- Ensure no runtime errors
- Ensure migrations run cleanly

---


---

# 🧠 SMART BEHAVIOR

- Prefer minimal, clean solutions over complex ones
- Reuse existing patterns in the project
- Follow existing structure strictly
- Think like a **senior .NET architect**

---

# ❌ WHAT NOT TO DO

- Do NOT rewrite large parts of the system
- Do NOT introduce new frameworks
- Do NOT change architecture
- Do NOT break API contracts
- Do NOT over-engineer

---

# ✅ GOAL

Claude should act as:
- Senior backend engineer
- Clean architecture enforcer
- Safe PR generator

---

# 💡 FINAL NOTE

This is a **real production SaaS system**, not a demo.

All code must be:
- Stable
- Maintainable
- Scalable
- Safe to merge
