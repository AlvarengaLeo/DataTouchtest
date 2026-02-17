# 🏗️ ARCHITECTURE.md - Arquitectura del Sistema

## Diagrama de Capas

```mermaid
graph TB
    subgraph "Presentation Layer"
        BZ[Blazor Server]
        API[Minimal API]
    end
    
    subgraph "Application Layer"
        SVC[Services]
        AUTH[AuthService]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        ENUM[Enums]
    end
    
    subgraph "Infrastructure Layer"
        DB[(DbContext)]
        EXT[External Services]
    end
    
    BZ --> SVC
    API --> SVC
    SVC --> ENT
    SVC --> DB
    AUTH --> DB
```

## Flujo de Autenticación

```mermaid
sequenceDiagram
    participant U as Usuario
    participant L as Login.razor
    participant A as /api/auth/login
    participant DB as DbContext
    participant C as Cookie
    
    U->>L: Ingresa credenciales
    L->>A: POST form data
    A->>DB: Valida User
    DB-->>A: User encontrado
    A->>C: SignInAsync (cookie)
    A-->>U: Redirect /
```

## Flujo de Booking

```mermaid
sequenceDiagram
    participant V as Visitante
    participant PC as PublicCard
    participant PB as PublicBooking
    participant AS as AppointmentService
    participant AV as AvailabilityService
    
    V->>PC: Visita /p/{org}/{card}
    PC->>V: Muestra tarjeta
    V->>PB: Click "Reservar"
    PB->>AV: GetAvailableSlotsAsync
    AV-->>PB: Lista de slots
    V->>PB: Selecciona slot
    PB->>AS: CreatePublicAppointmentAsync
    AS-->>V: Confirmación
```

## Machine State: Appointments

```mermaid
stateDiagram-v2
    [*] --> Pending: Nueva cita
    Pending --> Confirmed: Confirmar
    Pending --> Cancelled: Cancelar
    Confirmed --> Completed: Completar
    Confirmed --> NoShow: No asistió
    Confirmed --> Cancelled: Cancelar
    Cancelled --> Pending: Restaurar
```

## Machine State: Quotes

```mermaid
stateDiagram-v2
    [*] --> New: Nueva cotización
    New --> InReview: Revisar
    InReview --> NeedsInfo: Pedir info
    InReview --> Quoted: Enviar cotización
    NeedsInfo --> Quoted: Info recibida
    Quoted --> Negotiation: Negociar
    Negotiation --> Won: Cliente acepta
    Negotiation --> Lost: Cliente rechaza
    Won --> [*]
    Lost --> Archived: Archivar
```

## Flujo de Reservaciones (Template 5)

```mermaid
sequenceDiagram
    participant V as Visitante
    participant PC as PublicCard
    participant RM as PublicReservationModal
    participant RS as ReservationService
    
    V->>PC: Visita /p/{org}/{slug}
    PC->>V: Muestra tarjeta (reservations-range)
    V->>RM: Click "Reservar"
    RM->>V: Wizard 4 pasos (Fechas → Huéspedes → Extras → Confirmar)
    V->>RM: Completa formulario
    RM->>RS: SubmitReservationAsync
    RS-->>V: Confirmación
```

## Pipeline de Theming (3 superficies)

```mermaid
graph LR
    PR[PresetRegistry<br/>17 presets] --> TT[ThemeTokens<br/>43+ props]
    TT --> TH[ThemeHelper<br/>GenerateCssVariables]
    TH --> DT["--dt-* vars<br/>(~60 vars)"]
    TH --> SF["--surface-* bridge<br/>(12 aliases)"]
    DT --> PUB[PublicCard<br/>.landing-wrapper]
    DT --> MY[MyCard Preview<br/>.phone-card-content]
    DT --> TL[TemplateLibrary<br/>.xxx-preview-full]
```

## Decisiones Arquitectónicas

| Decisión | Razón | Fecha |
|----------|-------|-------|
| InMemory para desarrollo | Evitar dependencia de SQL Server | Dic 2025 |
| Cookie Auth vs JWT | Blazor Server maneja sesión | Dic 2025 |
| Services en Web, no Domain | Simplicidad para MVP | Ene 2026 |
| Background Service para SLA | Alertas automáticas | Ene 2026 |
| SemaphoreSlim en MyCard | Serializar DB ops, evitar EF Core concurrency crash | Feb 2026 |
| IDbContextFactory para dashboards | Contextos cortos para read-only queries | Feb 2026 |
| Schema updates via raw SQL (no EF Migrations) | Control explícito de ALTER TABLE | Feb 2026 |
| Static PresetRegistry (no DI/DB) | Presets inmutables, sin lifetime concerns | Feb 2026 |
| Dual CSS var namespace (--dt-* + --surface-*) | Migración gradual sin romper legacy | Feb 2026 |

---

*Última actualización: Febrero 2026*
