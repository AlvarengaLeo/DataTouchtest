# Entidades del Dominio (18)

Actualizado: 2026-02-16

## Entidades Core

| Entidad | Archivo | Notas |
|---------|---------|-------|
| Organization | Domain/Entities/Organization.cs | Tenant principal |
| User | Domain/Entities/User.cs | Auth + perfil |
| Card | Domain/Entities/Card.cs | Tarjeta digital + JSON columns (AppearanceStyleJson, QuoteSettingsJson, SocialLinksJson, WebsiteLinksJson, GalleryImagesJson) |
| Lead | Domain/Entities/Lead.cs | Leads capturados |
| LeadNote | Domain/Entities/LeadNote.cs | Notas en leads |
| Activity | Domain/Entities/Activity.cs | Timeline de auditoría |

## Entidades Card System

| Entidad | Archivo | Notas |
|---------|---------|-------|
| CardTemplate | Domain/Entities/CardTemplate.cs | Plantillas (2 rows seed, galería usa in-memory) |
| CardStyle | Domain/Entities/CardStyle.cs | Estilos de tarjeta |
| CardComponent | Domain/Entities/CardComponent.cs | Componentes de tarjeta |
| CardAnalytics | Domain/Entities/CardAnalytics.cs | Tracking: page_view, cta_click, etc. |

## Entidades Booking System

| Entidad | Archivo | Notas |
|---------|---------|-------|
| Service | Domain/Entities/Service.cs | Servicios ofrecidos (UseGlobalSchedule bool) |
| Appointment | Domain/Entities/Appointment.cs | Citas (5 estados: Pending→Confirmed→Completed/Cancelled/NoShow) |
| AvailabilityRule | Domain/Entities/AvailabilityRule.cs | Reglas de disponibilidad (BreakStartTime/EndTime, ServiceId FK) |
| AvailabilityException | Domain/Entities/AvailabilityException.cs | Excepciones/bloqueos |
| BookingSettings | Domain/Entities/BookingSettings.cs | Config de booking (TimeZoneId, etc.) |
| QuoteRequest | Domain/Entities/QuoteRequest.cs | Cotizaciones (8 estados: New→InReview→...→Won/Lost→Archived) |

## Entidades Reservations System (Template 5)

| Entidad | Archivo | Notas |
|---------|---------|-------|
| ReservationRequest | Domain/Entities/ReservationRequest.cs | Solicitudes de reserva (ReservationStatus enum) |
| ReservationResource | Domain/Entities/ReservationResource.cs | Recursos reservables |

## Enums

| Enum | Archivo | Valores |
|------|---------|---------|
| AppointmentStatus | Appointment.cs | Pending, Confirmed, Completed, Cancelled, NoShow |
| QuoteStatus | QuoteRequest.cs | New, InReview, NeedsInfo, Quoted, Negotiation, Won, Lost, Archived |
| ReservationStatus | ReservationRequest.cs | (estados de reservación) |
| ActivityType | Activity.cs | Created, StatusChange, Note, EmailSent, EmailReceived, Call, Assignment, Conversion, AttachmentAdded, SlaAlert, Merge, Automation |

