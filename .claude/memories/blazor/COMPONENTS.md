# Componentes Compartidos (18)

Actualizado: 2026-02-16

## Componentes Synced (usados en 3 superficies: PublicCard, MyCard Preview, TemplateLibrary)

| Componente | Parámetros clave | Sync |
|------------|-----------------|------|
| AppointmentBookingBlock | `Compact`, `Services`, `OnBookClick` | ✅ SYNCED |
| PortfolioGalleryBlock | `EnablePhotos`, `EnableVideos`, `Photos`, `Videos` | ✅ SYNCED |
| QuoteRequestBlock | `Compact`, `ContainerStyle` | ✅ SYNCED |
| ReservationBookingBlock | `Compact` | ✅ SYNCED |
| SocialLinksRow | `Compact`, `IsPreview`, `ContainerStyle`, `LinkedIn`, etc. | ✅ SYNCED |

## Modales Públicos

| Componente | Template | Wizard |
|------------|----------|--------|
| PublicAppointmentModal | `appointments` | 4 pasos: Service → Date → Hour → Confirm |
| PublicQuoteRequestModal | `quote-request` | Formulario de cotización |
| PublicReservationModal | `reservations-range` | 4 pasos: Fechas → Huéspedes → Extras → Confirmar |

## Componentes de UI General

| Componente | Propósito |
|------------|-----------|
| AppointmentDetailsDrawer | Panel lateral de detalles de cita |
| CardPreview | Preview de tarjeta |
| ChannelBreakdownDialog | Analytics: desglose por canal |
| CountryPhoneInput | Input de teléfono con selector de país |
| CreateAppointmentDialog | Diálogo para nueva cita |
| DesignCustomizer | Panel editor de tema/apariencia |
| IconRegistry | Registro SVG de iconos |
| QrCustomizer | Personalización de QR code |
| QuoteRequestModal | Modal interno de cotización |
| TemplateSelector | Widget de selección de template |

## Componentes Propuestos (aún no creados)

| Componente | Reemplazaría |
|------------|-------------|
| ActionButtonsRow | `cta-row` (Public) + `phone-cta-secondary-row` (MyCard) + `qrp-cta-row` (Templates) |
| SaveContactButton | `btn-primary-cta` (Public) + `phone-cta-primary` (MyCard) + `qrp-cta-save` (Templates) |
| StatusChips | `status-chip` (Public) + `phone-chip` (MyCard) + `qrp-chip` (Templates) |

