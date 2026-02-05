# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build entire solution
dotnet build

# Run the web app (development with InMemory DB)
dotnet run --project src/DataTouch.Web

# Run with hot reload
dotnet watch run --project src/DataTouch.Web

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Dev credentials:** `admin@demo.com` / `admin123`

**Local URLs:** `https://localhost:5001` or `http://localhost:5000`

## Architecture Overview

DataTouch is a Blazor Server CRM for digital business cards with booking and quote management.

```
src/
├── DataTouch.Domain/           # Entities only (16 classes)
├── DataTouch.Infrastructure/   # EF Core DbContext
├── DataTouch.Web/              # Blazor Server app
│   ├── Components/Pages/       # Razor pages
│   ├── Components/Shared/      # Reusable components
│   ├── Services/               # Business logic (13 services)
│   └── Models/                 # View models, theme tokens
└── DataTouch.Api/              # Future REST API (placeholder)
```

**Key patterns:**
- Clean Architecture: Domain → Infrastructure → Web
- Service Layer pattern: All business logic in `Web/Services/`
- InMemory DB for development, MySQL 8 (Pomelo) for production
- Cookie-based authentication with `CustomAuthStateProvider`

## Key Services & Their Responsibilities

| Service | Purpose |
|---------|---------|
| `AppointmentService` | CRUD for appointments, slot calculation |
| `QuoteService` | Quote lifecycle (8 states), lead deduplication |
| `AvailabilityService` | Weekly rules, exceptions, slot generation |
| `DashboardService` | KPIs, analytics, chart data |
| `CardAnalyticsService` | Event tracking (page_view, cta_click, etc.) |

## State Machines

**QuoteStatus:** New → InReview → NeedsInfo → Quoted → Negotiation → Won/Lost → Archived

**AppointmentStatus:** Pending → Confirmed → Completed/Cancelled/NoShow

## Routes

**Public (EmptyLayout):**
- `/p/{org}/{slug}` - Public card view
- `/book/{org}/{slug}/{serviceId}` - Public booking
- `/login` - Login page

**Protected (MainLayout):**
- `/` - Dashboard
- `/cards/mine` - Card editor
- `/appointments` - Appointment CRM (3 tabs: Citas, Servicios, Disponibilidad)
- `/quotes` - Quote CRM
- `/leads`, `/leads/{id}` - Lead management

## MudBlazor Conventions

- Use `T="Type"` parameter on `MudSelect` to avoid `InvalidCastException`
- For nullable Guid selects: `T="Guid?"` with cast `(Guid?)value`
- Current version: MudBlazor 8.x (some `MUD0002` warnings exist for obsolete attributes)

## Data Storage Patterns

Card entity uses JSON columns for flexible data:
- `SocialLinksJson` - Social media links
- `WebsiteLinksJson` - Custom website links
- `GalleryImagesJson` - Portfolio images
- `AppearanceStyleJson` - Theme tokens

## GitFlow

- `main` - Production (protected, PR only from develop)
- `develop` - Development (protected, PR required)
- `feature/*`, `fix/*`, `refactor/*`, `docs/*` - Work branches

Commit prefixes: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`
