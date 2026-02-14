# CLAUDE.md — DATATOUCH CRM: CANONICAL TECHNICAL REFERENCE

> **Single source of truth** for architecture, card rendering pipeline, theming, and template synchronization.
> Replaces the former `docs/Claude.md`. All documentation lives here.

---

## BUILD & RUN COMMANDS

```bash
dotnet build                                        # Build entire solution
dotnet run --project src/DataTouch.Web               # Dev (InMemory DB)
dotnet watch run --project src/DataTouch.Web         # Hot reload
dotnet test                                          # Run tests
dotnet test --collect:"XPlat Code Coverage"          # Tests + coverage
```

**Dev credentials:** `admin@demo.com` / `admin123` · **URLs:** `https://localhost:5001` / `http://localhost:5000`

---

## ARCHITECTURE OVERVIEW

DataTouch is a Blazor Server CRM for digital business cards with booking and quote management.

```
src/
├── DataTouch.Domain/           # Entities (16 classes): Card, Organization, User, Lead, etc.
├── DataTouch.Infrastructure/   # EF Core DbContext (DataTouchDbContext)
├── DataTouch.Web/              # Blazor Server app
│   ├── Components/Pages/       # Razor pages (14 main pages)
│   ├── Components/Shared/      # Reusable components (13 files)
│   ├── Services/               # Business logic (11 services)
│   └── Models/                 # ThemeTokens, PresetRegistry, ThemeHelper, QuoteSettingsModel
└── DataTouch.Api/              # Future REST API (placeholder)
```

**KEY PATTERNS:**
- Clean Architecture: Domain → Infrastructure → Web
- Service Layer: all business logic in `Web/Services/`
- InMemory DB for dev, SQL Server for production
- Cookie auth via `CustomAuthStateProvider`
- MudBlazor 8.x for UI components

---

## KEY SERVICES & THEIR RESPONSIBILITIES

| Service | Purpose |
|---|---|
| `AuthService` | User authentication, session management |
| `AppointmentService` | CRUD for appointments, slot calculation |
| `QuoteService` | Quote lifecycle (8 states), lead deduplication |
| `AvailabilityService` | Weekly rules, exceptions, slot generation |
| `DashboardService` | KPIs, analytics, chart data |
| `CardAnalyticsService` | Event tracking (`page_view`, `cta_click`, etc.) |
| `ThemeService` | Dark/light mode toggle (admin UI, not card theming) |
| `CardTemplateSeeder` | Seeds system `CardTemplate` rows on startup |
| `GeoLocationService` | Visitor geolocation (IP-based) |
| `CountryPhoneService` | Phone number validation by country |

---

## STATE MACHINES

- **QuoteStatus:** New → InReview → NeedsInfo → Quoted → Negotiation → Won/Lost → Archived
- **AppointmentStatus:** Pending → Confirmed → Completed/Cancelled/NoShow

---

## ROUTES

**Public (EmptyLayout):**
- `/p/{org}/{slug}` — Public card (`PublicCard.razor`)
- `/book/{org}/{slug}/{serviceId}` — Public booking
- `/login` — Login

**Protected (MainLayout):**
- `/` — Dashboard
- `/cards/mine` — Card editor with live preview (`MyCard.razor`)
- `/templates` — Template gallery (`TemplateLibrary.razor`)
- `/appointments` — Appointment CRM (3 tabs)
- `/quotes` — Quote CRM
- `/leads`, `/leads/{id}` — Lead management

---

## MUDBLAZOR CONVENTIONS

- Use `T="Type"` on `MudSelect` to avoid `InvalidCastException`
- For nullable Guid selects: `T="Guid?"` with cast `(Guid?)value`
- MudBlazor 8.x — some `MUD0002` warnings for obsolete attributes

---

## DATA STORAGE PATTERNS

`Card` entity (`src/DataTouch.Domain/Entities/Card.cs`) uses JSON columns:

| Column | Content |
|---|---|
| `AppearanceStyleJson` | Theme preset + layout prefs (`CardStyleModel`) |
| `QuoteSettingsJson` | Quote block config (`QuoteSettingsModel`) |
| `SocialLinksJson` | Social media links |
| `WebsiteLinksJson` | Custom website links |
| `GalleryImagesJson` | Portfolio images |

---

## GITFLOW

- `main` — Production (PR only from develop)
- `develop` — Development (PR required)
- `feature/*`, `fix/*`, `refactor/*`, `docs/*` — Work branches
- Prefixes: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`

---
---

# TEMPLATE & CARD RENDERING PIPELINE (SINGLE SOURCE OF TRUTH)

> Complete technical reference for the template system, theming pipeline, and visual synchronization contract.
> **Last rewritten:** 2025-02-11. All 5 desynchronization causes resolved. 9 sync contract tests passing.

---

## A) ARQUITECTURA DE RENDER: 3 SUPERFICIES, 1 FUENTE DE VERDAD VISUAL

### SURFACE MATRIX

| CONTEXT | ROUTE | RAZOR FILE | DATA SOURCE | THEME INJECTION | CONTAINER |
|---|---|---|---|---|---|
| **Public Card** | `/p/{org}/{slug}` | `Components/Pages/PublicCard.razor` | DB (`Card` entity, deserialized JSON) | `ThemeHelper.GenerateCssVariables()` on `.landing-wrapper` | Full-page div |
| **Editor Live Preview** | `/cards/mine` | `Components/Pages/MyCard.razor` | In-memory `_card` + `_themeTokens` + `_cardStyle` | `ThemeHelper.GenerateCssVariables()` on `.phone-card-content` | iPhone mockup frame |
| **Template Gallery** | `/templates` | `Components/Pages/TemplateLibrary.razor` | Static mock data + `_userCard` (if logged in) | `ThemeHelper.GenerateCssVariables()` via static `_skyLightCssVars` on `.quote-request-preview-full` | iPhone carousel |

All 3 surfaces now use `ThemeHelper.GenerateCssVariables()` as the single CSS variable generator.

### SOURCE OF TRUTH

**PublicCard is the visual source of truth.** It is what the end-user's visitors see. Template Preview and Live Preview exist solely to give the card owner a faithful approximation of what the public card will look like. Any visual divergence between these surfaces is a bug.

### INJECTED SERVICES PER SURFACE

| Surface | Injected Services |
|---|---|
| **PublicCard** | `DbContext`, `AnalyticsService`, `QuoteService`, `DialogService`, `Logger` |
| **MyCard** | `DbContext`, `AuthService`, `JSRuntime`, `Environment` |
| **TemplateLibrary** | `DbContext`, `AuthService` |

### RENDERING CONTEXT (CONCEPTUAL — NOT IN CODE YET)

Each surface uses boolean flags (`_isServicesTemplate`, `_isQuoteRequestTemplate`, etc.) and `@if` chains. There is **no shared `RenderingContext` enum**. Recommended future addition:

```csharp
// Proposed: src/DataTouch.Web/Models/RenderingContext.cs
public enum RenderingContext { Public, EditorPreview, TemplatePreview }
```

Would be passed as `[CascadingParameter]` to shared components, enabling them to disable click handlers in previews and inject mock data without duplicating markup.

### COMPONENT HIERARCHY PER SURFACE (COMPACT)

**PublicCard** → `.landing-wrapper` → `.profile-card` → `.card-content` → avatar, name, `<SocialLinksRow>`, status-chips, `<QuoteRequestBlock>`, cta-section, lead-form.

**MyCard Preview** → `.phone-card-content` → phone-avatar, phone-name, `<SocialLinksRow Compact>`, `<QuoteRequestBlock Compact>`, phone-status-chips, phone-cta-container, phone-footer.

**TemplateLibrary** → `.quote-request-preview-full` (with `_skyLightCssVars`) → qrp-hero, `<SocialLinksRow Compact>`, qrp-chips, `<QuoteRequestBlock Compact>`, qrp-cta-section, qrp-footer.

---

## B) THEME TOKENS: MODELO / SERIALIZACIÓN / PRECEDENCIA / APLICACIÓN A CSS

### THEMETOKENS → CSS VARS PIPELINE

```
ThemeTokens (C# record, 43+ properties in 7 groups)
  → PresetRegistry.GetById(presetId)         // Static lookup, 17 presets
  → ThemeHelper.GenerateCssVariables(tokens)  // ~60 CSS custom properties
  → Applied as inline style="..." on wrapper div
  → Child elements consume via var(--dt-*)
```

### KEY FILES

| FILE | ROLE |
|---|---|
| `Models/ThemeTokens.cs` | Record with 7 groups: Background (4), Surfaces (9), Typography (4), Accent (3), Buttons (5), Focus/States (3), Semantic (4) |
| `Models/PresetRegistry.cs` | Static registry, 17 presets (9 dark + 8 light), `GetById()`, `Default`, `DarkPresets`, `LightPresets` |
| `Models/ThemeHelper.cs` | `GenerateCssVariables()` (~48 `--dt-*` vars + 12 `--surface-*` bridge aliases), `GetBackgroundStyle()`, `GetGlassBlur()`, `CreateFromLegacy()` |
| `Models/CardStyleModel.cs` | Shared model for card appearance (preset tracking, layout prefs), serialized to `AppearanceStyleJson` |
| `Services/CardService.cs` | Static helpers: `DeserializeStyle()`, `SerializeStyle()`, `GetDefaultPresetForTemplate()`, `GetThemeTokens()` |

### PRESET REGISTRY (ALL 17 IDS)

| DARK PRESETS (9) | LIGHT PRESETS (8) |
|---|---|
| `premium-dark`, `midnight-blue`, `charcoal-gold` | `minimal-white`, `soft-cream`, `sky-light` |
| `emerald-night`, `rose-noir`, `high-contrast` | `mint-breeze`, `pearl-gray`, `rose-blush` |
| `obsidian`, `aurora`, `amber-fire` | `ocean-mist`, `lavender-cloud` |

### CSS VARIABLE SYSTEM

`ThemeHelper.GenerateCssVariables()` outputs two variable families from the same `ThemeTokens` source:

1. **Primary `--dt-*` variables** (~48 vars): `--dt-bg-value`, `--dt-surface-card`, `--dt-text-primary`, `--dt-accent-primary`, `--dt-button-primary-bg`, `--dt-semantic-whatsapp`, `--dt-modal-*`, `--dt-button-radius`, `--dt-input-radius`, etc.
2. **Bridge `--surface-*` aliases** (12 vars): `--surface-text-primary`, `--surface-input-bg`, `--surface-chip-bg`, etc. — resolved from the same tokens for backward compat with `.contrast-dark`/`.contrast-light` blocks.

### MUDTHEMEPROVIDER MAPPING

Both `PublicCard.razor` and `MyCard.razor` include:
```razor
<MudThemeProvider IsDarkMode="@_themeTokens.BgIsDark" Theme="@_theme" />
```
`BuildMudTheme()` maps `ThemeTokens` → `PaletteDark`/`PaletteLight`. This affects **MudBlazor components only** (`MudButton`, `MudText`, `MudTextField`, etc.). Custom HTML elements (`<button>`, `<span>`) are **not affected** — they must use `var(--dt-*)` CSS vars explicitly.

### PRECEDENCIA EXACTA (LOWEST → HIGHEST)

1. **ThemeTokens record defaults** — Fallback values baked into the record init (premium-dark look)
2. **User-chosen preset** — Stored in `CardStyleModel.PresetId` → `Card.AppearanceStyleJson` → loaded on page init
3. **Template default preset** — Forced in `DeserializeJsonFields()`: `services-quotes` → `emerald-night`, `quote-request` → `sky-light`
4. **CSS var fallbacks** — `var(--dt-accent-primary, #7C3AED)` in stylesheets provide last-resort defaults

**Critical note:** Step 3 runs **after** step 2 in `DeserializeJsonFields()`, meaning template-specific presets override user selection for forced templates. This is intentional.

### WHERE THEME IS CALCULATED

| SURFACE | METHOD | LOCATION |
|---|---|---|
| PublicCard | `DeserializeJsonFields()` → `PresetRegistry.GetById()` | `PublicCard.razor` ~line 2692-2702 |
| MyCard | `DeserializeJsonFields()` → preset forcing + `PresetRegistry.GetById()` | `MyCard.razor` ~line 4514-4589 |
| MyCard (query) | `HandleTemplateQueryParam()` → `ApplyPreset()` | `MyCard.razor` ~line 4429-4478 |
| MyCard (render) | `GetPreviewBackgroundStyle()` → `ThemeHelper.GenerateCssVariables()` | `MyCard.razor` ~line 4800-4805 |
| TemplateLibrary | Static field `_skyLightCssVars` = `ThemeHelper.GenerateCssVariables(PresetRegistry.GetById("sky-light"))` | `TemplateLibrary.razor` ~line 1808-1809 |

---

## C) SISTEMA DE TEMPLATES: DEFINICIÓN / REGISTRO / SELECCIÓN / PERSISTENCIA

### TEMPLATE REGISTRY (IN-MEMORY)

Templates are defined as a **private `List<TemplateData>`** inside `TemplateLibrary.razor` (~line 1862). This is **decoupled from the DB** — `CardTemplate` entity exists and is seeded with 2 rows in `CardTemplateSeeder.cs`, but the gallery ignores it.

`TemplateData` is a private record (~line 1795) with:
- `Name`, `Description`, `HeroGradient` (display props)
- `IsDefault`, `IsPortfolio`, `IsServices`, `IsQuoteRequest` (bool flags)
- `TemplateType` (string key, matches `Card.TemplateType`)

### TEMPLATE KEY TABLE

| KEY (`Card.TemplateType`) | NAME | DEFAULT PRESET | FLAG |
|---|---|---|---|
| `"default"` | Perfil Profesional | `premium-dark` | `IsDefault` |
| `"portfolio-creative"` | Portafolio Creativo | (none forced) | `IsPortfolio` |
| `"services-quotes"` | Servicios y Cotizaciones | `emerald-night` | `IsServices` |
| `"quote-request"` | Cotizaciones (Solicitud) | `sky-light` | `IsQuoteRequest` |

Additional gallery-only templates (`creative`, `minimal`, `professional`, `corporate`) exist in the carousel but use the default preview and have no forced preset.

**Persistence:** `Card.TemplateType` is a `string` column (default `"default"`) in `src/DataTouch.Domain/Entities/Card.cs`.

### "USAR ESTE DISEÑO" HANDLER

`TemplateLibrary.razor:UseThisDesign()` (~line 1907):
```csharp
var url = $"/cards/mine?template={SelectedTemplate.TemplateType}";
if (SelectedTemplate.IsQuoteRequest)  url += "&appearance=sky-light";
else if (SelectedTemplate.IsServices) url += "&appearance=emerald-night";
Navigation.NavigateTo(url);
```

### CARD ENTITY JSON FIELDS

| JSON COLUMN | MODEL CLASS | SERIALIZED BY |
|---|---|---|
| `AppearanceStyleJson` | `CardStyleModel` (`Models/CardStyleModel.cs`, shared) | `MyCard.razor:SaveCard()` ~line 4645 |
| `QuoteSettingsJson` | `QuoteSettingsModel` (`Models/QuoteSettingsModel.cs`) | `MyCard.razor:SaveCard()` ~line 4648 (always persisted) |
| `SocialLinksJson` | `SocialLinksModel` (private in MyCard/PublicCard) | `MyCard.razor:SaveCard()` ~line 4616 |
| `GalleryImagesJson` | `List<GalleryImageModel>` | `MyCard.razor:SaveCard()` ~line 4637 |

### MEMORY VS DB STATE IN MYCARD

| STATE | STORAGE | WHEN PERSISTED |
|---|---|---|
| `_cardStyle` (CardStyleModel) | In-memory | On `SaveCard()` → serialized to `AppearanceStyleJson` |
| `_themeTokens` (ThemeTokens) | In-memory only | **Never persisted directly** — derived from `_cardStyle.PresetId` via `PresetRegistry` |
| `_card.TemplateType` | DB | Persisted immediately on template selection AND on `SaveCard()` |
| `_quoteSettings` | In-memory | On `SaveCard()` → always serialized to `QuoteSettingsJson` |
| `_selectedPreset` (string) | In-memory | Synced to `_cardStyle.PresetId` via `ApplyPreset()`, persisted on save |

On page refresh, `DeserializeJsonFields()` (~line 4481) rehydrates `_cardStyle` from `AppearanceStyleJson`, then loads `_themeTokens` from `PresetRegistry` using `_cardStyle.PresetId`.

### AUTOMATIC DEFAULTS ON TEMPLATE SELECTION

In `DeserializeJsonFields()` (MyCard.razor ~line 4567):

| TEMPLATE | PRESET FORCED | EXTRA SIDE EFFECTS |
|---|---|---|
| `services-quotes` | `emerald-night` | Loads services list via `LoadServicesAsync()` |
| `quote-request` | `sky-light` | Sets `BackgroundIsDark=false`, `CardIsDark=false` |
| `portfolio-creative` | (none) | Enables gallery section |
| `default` | (none) | Clears gallery if switching away from portfolio |

**Critical note:** Template-specific preset forcing runs **after** `AppearanceStyleJson` deserialization. This means forced templates always override the user's saved preset.

---

## D) CONTRATO DE SINCRONIZACIÓN VISUAL (SYNC CONTRACT)

### SHARED COMPONENTS (IMPLEMENTED)

| COMPONENT | FILE | USED BY | KEY PARAMETERS |
|---|---|---|---|
| `SocialLinksRow` | `Components/Shared/SocialLinksRow.razor` | PublicCard, MyCard, TemplateLibrary | `LinkedIn`, `Instagram`, `Facebook`, `YouTube`, `Twitter`, `Website`, `Compact`, `IsPreview`, `ContainerStyle` |
| `QuoteRequestBlock` | `Components/Shared/QuoteRequestBlock.razor` | PublicCard, MyCard, TemplateLibrary | `Compact`, `ContainerStyle`, plus quote config parameters |

### SYNC STATUS PER BLOCK

| UI BLOCK | PUBLICCARD | MYCARD PREVIEW | TEMPLATELIBRARY | STATUS |
|---|---|---|---|---|
| **Quote Request Block** | `<QuoteRequestBlock>` | `<QuoteRequestBlock Compact>` | `<QuoteRequestBlock Compact>` | ✅ SYNCED (shared component) |
| **Social Icons** | `<SocialLinksRow>` | `<SocialLinksRow Compact>` | `<SocialLinksRow Compact>` | ✅ SYNCED (shared component) |
| **Save Contact Button** | `MudButton.btn-primary-cta` | `<button>.phone-cta-primary` | `<button>.qrp-cta-save` | ⚠ 3 IMPLEMENTATIONS (different HTML) |
| **WhatsApp/Call/Email** | `MudButton.btn-whatsapp/call/email` | `<button>.phone-cta-outline` | `<button>.qrp-cta-outline` | ⚠ 3 IMPLEMENTATIONS |
| **Status Chips** | `<span>.status-chip` + MudIcon | `<span>.phone-chip` + SVG | `<span>.qrp-chip` | ⚠ 3 IMPLEMENTATIONS |
| **Avatar** | `MudAvatar.profile-avatar` | `MudAvatar` in phone-avatar | `<div>.qrp-avatar` initials | ⚠ PARTIAL |
| **Services Section** | Full cards + booking CTA | Compact cards | Compact preview | ⚠ BY DESIGN (different detail levels) |

### REGLAS DE ORO

1. **One component, one CSS class set** — Every visual block should live in `Components/Shared/` and be reused across all three surfaces.
2. **Zero hardcoded hex in synced blocks** — All colors via `var(--dt-*)`. TemplateLibrary injects CSS vars via `_skyLightCssVars`.
3. **Mock data changes the dataset, not the component** — TemplateLibrary passes mock social links to the same `SocialLinksRow` component, not different HTML.
4. **Compact mode via parameter, not duplication** — Use `Compact` bool parameter to scale down, never a separate component.

### PROPOSED SHARED COMPONENTS (NOT YET CREATED)

| COMPONENT | REPLACES |
|---|---|
| `ActionButtonsRow.razor` | `cta-row` (Public) + `phone-cta-secondary-row` (MyCard) + `qrp-cta-row` (Templates) |
| `SaveContactButton.razor` | `btn-primary-cta` (Public) + `phone-cta-primary` (MyCard) + `qrp-cta-save` (Templates) |
| `StatusChips.razor` | `status-chip` (Public) + `phone-chip` (MyCard) + `qrp-chip` (Templates) |

### ALL SHARED COMPONENTS IN `Components/Shared/`

| COMPONENT | PURPOSE |
|---|---|
| `SocialLinksRow.razor` | Social icon row (synced across all 3 surfaces, rounded-rect, CSS vars) |
| `QuoteRequestBlock.razor` | Quote request block (synced across all 3 surfaces) |
| `PublicQuoteRequestModal.razor` | Modal dialog for public quote submission |
| `QuoteRequestModal.razor` | Internal quote request modal |
| `DesignCustomizer.razor` | Theme/appearance editor panel |
| `TemplateSelector.razor` | Template selection widget |
| `CardPreview.razor` | Card preview component |
| `IconRegistry.razor` | SVG icon registry |
| `CountryPhoneInput.razor` | Phone input with country selector |
| `QrCustomizer.razor` | QR code customization |
| `AppointmentDetailsDrawer.razor` | Appointment detail side panel |
| `ChannelBreakdownDialog.razor` | Analytics channel breakdown |
| `CreateAppointmentDialog.razor` | New appointment dialog |

---

## E) PATRONES DE DISEÑO EN EL REPO (CON EVIDENCIA REAL)

### PATTERN 1: STATIC REGISTRY + RECORD TOKENS

`PresetRegistry.cs` defines a static `IReadOnlyList<ThemePreset>` with 17 entries. Each `ThemePreset` contains an immutable `ThemeTokens` record. Lookup is via `PresetRegistry.GetById(string)`. No DI, no DB — pure static data.

**Why:** Theme presets don't change at runtime. Static access avoids service lifetime concerns and ensures all 3 surfaces see identical token values.

```csharp
// PresetRegistry.cs:655
public static ThemePreset? GetById(string id) => All.FirstOrDefault(p => p.Id == id);
```

### PATTERN 2: CSS VARIABLE BRIDGE (DUAL NAMESPACE)

`ThemeHelper.GenerateCssVariables()` (~line 68-81) outputs both `--dt-*` (canonical) and `--surface-*` (legacy bridge) variables from the same `ThemeTokens` source. This allows gradual migration without breaking existing `.contrast-dark`/`.contrast-light` CSS blocks.

```csharp
// ThemeHelper.cs:68-80
/* ── Bridge: legacy --surface-* aliases → resolved from same tokens ── */
--surface-text-primary: {tokens.TextPrimary};
--surface-input-bg: {tokens.SurfaceInput};
// ... 12 aliases total
```

### PATTERN 3: QUERY-PARAM DRIVEN TEMPLATE APPLICATION

`UseThisDesign()` in TemplateLibrary builds a URL with `?template=xxx&appearance=yyy`. MyCard reads these in `DeserializeJsonFields()` (~line 4538-4560) and `HandleTemplateQueryParam()` (~line 4429-4478). Template type is persisted immediately to DB, while appearance is applied in-memory and persisted on explicit save.

```csharp
// TemplateLibrary.razor:1912-1916
var url = $"/cards/mine?template={SelectedTemplate.TemplateType}";
if (SelectedTemplate.IsQuoteRequest)  url += "&appearance=sky-light";
else if (SelectedTemplate.IsServices) url += "&appearance=emerald-night";
```

### PATTERN 4: SHARED COMPONENT WITH COMPACT MODE

`SocialLinksRow.razor` and `QuoteRequestBlock.razor` accept a `Compact` bool parameter. PublicCard uses the full-size variant; MyCard and TemplateLibrary use `Compact=true`. Same markup, different scale — controlled by a CSS class toggle.

```razor
<!-- SocialLinksRow.razor:7 -->
<div class="social-links-row @(Compact ? "social-links-compact" : "")" style="@ContainerStyle">
```

### SEQUENCE: TEMPLATE SELECTION → LIVE PREVIEW → SAVE → PUBLIC CARD

```
USUARIO                TEMPLATELIBRARY.RAZOR          MYCARD.RAZOR              DB (CARD ENTITY)           PUBLICCARD.RAZOR
  │                         │                            │                          │                          │
  ├─ Click "Usar diseño" ──►│                            │                          │                          │
  │                         ├─ UseThisDesign()           │                          │                          │
  │                         │  /cards/mine               │                          │                          │
  │                         │    ?template=quote-request  │                          │                          │
  │                         │    &appearance=sky-light    │                          │                          │
  │                         ├─ NavigateTo(url) ─────────►│                          │                          │
  │                         │                            ├─ OnInitializedAsync()    │                          │
  │                         │                            │  AuthService → Card ─────►│                          │
  │                         │                            │◄── Card entity ──────────┤                          │
  │                         │                            ├─ DeserializeJsonFields() │                          │
  │                         │                            │  Parse JSON → _cardStyle │                          │
  │                         │                            │  Read query["template"]  │                          │
  │                         │                            │  → Force sky-light preset│                          │
  │                         │                            │  → Persist TemplateType ─►│                          │
  │                         │                            ├─ HandleTemplateQueryParam()                         │
  │                         │                            │  → ApplyPreset()         │                          │
  │                         │                            │    Sets _cardStyle.*     │                          │
  │                         │                            │    Sets _themeTokens     │                          │
  │                         │                            ├─ RENDER LIVE PREVIEW     │                          │
  │◄── User sees preview ──┤                            │  ThemeHelper.Generate()  │                          │
  │                         │                            │  → --dt-* on container   │                          │
  ├─ Click "Guardar" ──────────────────────────────────►│                          │                          │
  │                         │                            ├─ SaveCard()              │                          │
  │                         │                            │  Serialize all JSON      │                          │
  │                         │                            │  DbContext.SaveChanges() ►│                          │
  ├─ Visitor opens /p/org/slug ────────────────────────────────────────────────────►│                          │
  │                         │                            │                          │  ├─ OnInitializedAsync() │
  │                         │                            │                          │  │  Deserialize JSON     │
  │                         │                            │                          │  │  PresetRegistry       │
  │                         │                            │                          │  │  ThemeHelper.Generate  │
  │                         │                            │                          │  │  → --dt-* on wrapper  │
  │◄── Visitor sees card ──────────────────────────────────────────────────────────┤  │  RENDER              │
```

### SERVICES PARTICIPATING

| STEP | SERVICE / COMPONENT | MODEL TRAVELING |
|---|---|---|
| Template selection | `TemplateLibrary.razor` (no service) | `TemplateData` (in-memory) |
| Navigation | Blazor `NavigationManager` | Query string: `template` + `appearance` |
| Card load | `DbContext` direct query | `Card` entity |
| Deserialization | Inline `JsonSerializer` | `CardStyleModel`, `QuoteSettingsModel`, social links |
| Preset lookup | `PresetRegistry` (static) | `ThemePreset` → `ThemeTokens` |
| CSS generation | `ThemeHelper` (static) | `string` (CSS custom properties) |
| Save | `DbContext.SaveChangesAsync()` | `Card` entity (mutated in-memory) |
| Public render | `DbContext` + `AnalyticsService` + `QuoteService` | `Card` entity → `CardStyleModel` → `ThemeTokens` |

**Note:** `CardService.cs` exists as a static helper for serialization/defaults, but MyCard and PublicCard still query `DbContext` directly. `QuoteService` is used only in PublicCard for quote submission. `AnalyticsService` tracks page views and CTA clicks.

---

## F) PUNTOS DE EXTENSIÓN

### HOW TO ADD A NEW PRESET

1. Add a `ThemePreset` entry to `PresetRegistry.cs:All` list with unique `Id`.
2. The DesignCustomizer panel auto-discovers presets from `PresetRegistry.All`.
3. No other files need changes — `ThemeHelper.GenerateCssVariables()` works with any `ThemeTokens` instance.

### HOW TO ADD A NEW CSS VARIABLE

1. Add property to `ThemeTokens.cs` record.
2. Add `--dt-xxx: {tokens.NewProp};` line in `ThemeHelper.GenerateCssVariables()`.
3. Add `--surface-xxx: {tokens.NewProp};` bridge alias if consumed by legacy CSS.
4. Use `var(--dt-xxx, fallback)` in CSS where needed.

### HOW TO ADD A PROPERTY TO CARDSTYLEMODEL

1. Add property to `Models/CardStyleModel.cs` — single file.
2. Both MyCard and PublicCard auto-serialize/deserialize it via `AppearanceStyleJson`.
3. Update `CardService.cs` if it needs a default value.

### HOW TO ADD A SHARED UI BLOCK

1. Create `.razor` component in `Components/Shared/`.
2. Add `Compact` bool parameter if needed.
3. Replace markup in all 3 surfaces with `<ComponentName>` tag.
4. Add CSS using `var(--dt-*)` variables — no hardcoded hex.

### HOW TO ADD A NEW TEMPLATE TYPE

1. Add entry to `_templates` list in `TemplateLibrary.razor` (~line 1862).
2. Add `bool IsXxx` flag to `TemplateData` record (~line 1795).
3. Add `else if (template.IsXxx)` preview branch in carousel. Use shared components.
4. In `MyCard.razor`: add `_isXxxTemplate` boolean, set in `DeserializeJsonFields()` and `HandleTemplateQueryParam()`.
5. Add `@if (_isXxxTemplate)` preview section in MyCard.
6. In `PublicCard.razor`: add `_isXxxTemplate` flag, set in `LoadCardData()`, add `@if` section.
7. If forced preset: add `&appearance=` in `UseThisDesign()` and preset forcing in `DeserializeJsonFields()`.
8. Update `CardService.GetDefaultPresetForTemplate()`.
9. Add `else if` in `SaveCard()` for new template type string.
10. `Card.TemplateType` is free-form string — no schema migration needed.

### APPOINTMENTS TEMPLATE ("Citas/Agenda") PIPELINE

**Template key:** `appointments` | **Default preset:** `mint-breeze` | **Shared component:** `AppointmentBookingBlock.razor`

| SURFACE | FILE | SERVICES SOURCE | BLOCK RENDERING |
|---|---|---|---|
| Template Preview | `TemplateLibrary.razor` | `_mockAppointmentServices` (static: Consulta General, Asesoría Premium) | `<AppointmentBookingBlock Compact="true" Services="@_mockAppointmentServices" />` |
| Live Preview | `MyCard.razor` ~line 1082 | `_services` (real DB) → fallback to `_mockAppointmentServices` if empty | Same shared component; mock fallback + hint "Configura tus servicios" |
| Public Card | `PublicCard.razor` ~line 244 | `_services` (real DB via `DbContext.Services`) | `<AppointmentBookingBlock Services="..." OnBookClick="OpenSchedulerModal" />` |

**Modal:** `PublicAppointmentModal.razor` — always centered (flexbox on backdrop), 4-step wizard (Service → Date → Hour → Confirm). Uses `--dt-*` CSS vars. If no services exist, shows empty-state.

**Availability calculation (day selectable rule):**
- A day is **selectable** if `slotsCount(day) > 0` — at least 1 time slot fits (not blocked, not booked).
- A day is **disabled** if: (a) past, (b) no time windows (weekend with no rules or no defaults), (c) all slots booked/blocked.
- **Default schedule fallback:** If `AvailabilityRules` table has 0 rows for the card, `AvailabilityService` uses in-memory Mon-Fri 9:00-17:00 defaults (not persisted). This ensures the public card works without requiring the owner to configure availability first.
- **Batch availability:** `AvailabilityService.GetMonthAvailabilityAsync` loads all rules, exceptions, and booked appointments in 3 DB queries for the whole month (not per-day). Called via `AppointmentService.GetMonthAvailabilityAsync` which resolves service duration first.
- **Calendar UX:** Loading spinner while fetching; empty-month state with "Ver siguiente mes" CTA if 0 days available.

**Key guards (desync prevention):**
- `MyCard.razor` ~line 1288: `else if (_isAppointmentsTemplate)` prevents contact form from rendering.
- `MyCard.razor` ~line 653: services editor gate includes `|| _isAppointmentsTemplate`.
- `PublicCard.razor` ~line 301: lead form hidden for `!_isAppointmentsTemplate`.

**Debug checklist (if desync recurs):**
1. Verify `_isAppointmentsTemplate` is set in `DeserializeJsonFields()` AND `HandleTemplateQueryParam()`.
2. Verify the `else if (_isAppointmentsTemplate)` branch exists in MyCard's template-specific content chain (~line 1288).
3. Verify services editor gate at ~line 653 includes `_isAppointmentsTemplate`.
4. Verify `AppointmentBookingBlock` is the SAME component tag in all 3 surfaces (no inline markup).
5. Verify modal CSS uses `display: flex; align-items: center; justify-content: center` on backdrop (not bottom-sheet).

### SCHEDULE CONFIG ARCHITECTURE (Global + Per-Service)

**Overview:** The schedule system supports a global "Horario de atención" editor and per-service overrides with optional break times.

**Entity changes:**

| Entity | New Fields | Purpose |
|---|---|---|
| `AvailabilityRule` | `BreakStartTime` (TimeSpan?), `BreakEndTime` (TimeSpan?) | Optional mid-day break window |
| `AvailabilityRule` | `ServiceId` (Guid?, FK → Service) | `null` = global rule, set = per-service override |
| `Service` | `UseGlobalSchedule` (bool, default `true`) | When `false`, service uses its own `AvailabilityRule` rows |

**Unique index:** `AvailabilityRules(CardId, DayOfWeek, ServiceId)` — allows both global (`ServiceId=null`) and per-service rules for the same day.

**Schema updates:** Added via `DbInitializer.ApplySchemaUpdatesAsync()` (raw SQL `ALTER TABLE` + index recreation). No EF migrations.

**Global schedule editor UI (MyCard.razor, appointments only):**
- Located above the services list, inside the services section
- Preset chips: "Lun–Vie" / "Lun–Sáb" / "Personalizado"
- 7 day-toggle chips (Lu–Do)
- Time range pickers (Desde / Hasta)
- Break toggle with break time pickers
- Timezone display from `BookingSettings.TimeZoneId`
- Exceptions mini-table with "Agregar bloqueo" button

**Per-service override (MyCard.razor):**
- Each service card has a "Usar horario global" `MudSwitch` in the booking config panel
- Default: ON (inherits global schedule)
- When OFF: service uses `AvailabilityRule` rows where `ServiceId = service.Id`

**Data flow — Loading:**
```
OnInitializedAsync() → LoadScheduleDataAsync()
  → Loads AvailabilityRules (global, ServiceId=null)
  → Loads AvailabilityExceptions
  → Loads BookingSettings
  → SyncScheduleStateFromRules() → populates UI state (_globalScheduleActiveDays, _globalWorkStart/End, _hasBreak, etc.)
```

**Data flow — Saving:**
```
SaveCard() → BuildRulesFromScheduleState()
  → Removes old global rules from DbContext
  → Creates 7 new AvailabilityRule rows (one per day, active or inactive)
  → Each rule carries break times if _hasBreak is true
  → DbContext.SaveChangesAsync() persists all
```

**AvailabilityService slot calculation (break-aware + per-service):**
- `CalculateSlotsForDateAsync(cardId, date, duration, serviceId?)`:
  1. If `serviceId` provided and `Service.UseGlobalSchedule=false`: load service-specific rule
  2. Fallback to global rule (`ServiceId=null`)
  3. Fallback to default Mon-Fri 9-17 if no rules exist
  4. If rule has `BreakStartTime`/`BreakEndTime`: split time window into pre-break and post-break windows
  5. Generate slots from each window, filtering blocked/booked ranges
- `GetMonthAvailabilityAsync`: same break-aware logic, filters global rules via `ServiceId == null`
- `HasAvailabilityAsync`: only checks global rules (`ServiceId == null`)

**Service card UI (enterprise redesign):**
- Header: service name, chips (duration, price, modality), active switch, expand/collapse toggle
- Collapsible body (`MudCollapse`): basic fields + booking config panel
- Duration: `MudSelect` dropdown (15/30/45/60/90/120 min) replacing numeric field
- Booking config uses `@if` conditional (not nested `MudCollapse`) to avoid Razor depth limits
- ConversionType selector hidden for appointments template (always "booking")

**Schedule editor UX notes (editor-only, `MyCard.razor`):**
- The "Horario de atención" heading was removed — editor body is always visible (no collapse).
- The "Personalizado" preset pill was removed (redundant — `ToggleScheduleDay` auto-detects custom state).
- All `MudTimePicker` instances use `AmPm="true"` (12h display with AM/PM; `TimeSpan` persisted in 24h internally).
- This editor is **not** a shared component — it exists only in `MyCard.razor`. PublicCard and TemplateLibrary consume `AvailabilityRule` data via `AvailabilityService`, not the editor UI. No sync risk.

### VERIFICATION CHECKS (FOR ANY TEMPLATE/THEME CHANGE)

1. Template appears in `/templates` gallery with correct preview
2. "Usar este diseño" navigates to `/cards/mine?template=xxx&appearance=yyy`
3. Live preview shows correct theme immediately (no save required)
4. Theme tokens match between live preview and public card (inspect `--dt-*` vars)
5. `SaveCard()` persists `TemplateType` and `AppearanceStyleJson` correctly
6. Public card at `/p/{org}/{slug}` renders identically to live preview
7. Shared components render without errors in all 3 surfaces
8. Switching away from the template clears template-specific data
9. `dotnet build` passes with zero errors
10. No hardcoded hex colors in synced CSS — all via `var(--dt-*)`

---

## G) DIAGNÓSTICO ACTUALIZADO (5 CAUSAS)

> **All 5 causes resolved.** `dotnet build` PASS. 9 sync contract tests PASS.

### CAUSE #1: DUPLICATED COMPONENT MARKUP

- **SÍNTOMA:** Social icons rendered with different HTML in each surface (MudIconButton in PublicCard, inline SVGs in MyCard, hardcoded SVGs in TemplateLibrary).
- **DÓNDE:** PublicCard `/p/{org}/{slug}`, MyCard `/cards/mine`, TemplateLibrary `/templates`.
- **ARCHIVO(S):** `PublicCard.razor`, `MyCard.razor`, `TemplateLibrary.razor` (formerly 3 separate social icon blocks).
- **POR QUÉ:** Each surface was developed independently with its own social icon markup.
- **IMPACTO:** Shape mismatch (circular vs rounded-rect), color divergence, maintenance burden.
- **ESTADO:** ✅ **YA RESUELTO.** Created `Components/Shared/SocialLinksRow.razor` — all 3 surfaces now use `<SocialLinksRow>`.

### CAUSE #2: HARDCODED STYLE BLOCKS IN TEMPLATE PREVIEW

- **SÍNTOMA:** Quote-request preview in TemplateLibrary used hardcoded hex colors in `qrp-*` CSS classes.
- **DÓNDE:** TemplateLibrary `/templates`, quote-request preview carousel.
- **ARCHIVO(S):** `TemplateLibrary.razor` — `qrp-*` CSS blocks.
- **POR QUÉ:** TemplateLibrary was built before ThemeHelper existed; colors were pasted directly.
- **IMPACTO:** Preset changes in `PresetRegistry.cs` did not propagate to template gallery.
- **ESTADO:** ✅ **YA RESUELTO.** All `qrp-*` CSS uses `var(--dt-*)` with fallbacks. `_skyLightCssVars` (~line 1808) generates vars from `PresetRegistry` via `ThemeHelper`.

### CAUSE #3: DIFFERENT TOKEN APPLICATION METHODS

- **SÍNTOMA:** MyCard applied theme colors via inline `style="background: @_themeTokens.AccentPrimary"` instead of CSS vars.
- **DÓNDE:** MyCard `/cards/mine` live preview.
- **ARCHIVO(S):** `MyCard.razor` — ~15 locations with `style="...@_themeTokens..."`.
- **POR QUÉ:** Quick prototyping used direct C# interpolation instead of CSS variable pipeline.
- **IMPACTO:** Two rendering paths (CSS vars in PublicCard, inline in MyCard) produced subtle visual differences.
- **ESTADO:** ✅ **YA RESUELTO.** All inline `_themeTokens` styles removed. CSS rules use `var(--dt-*)`. PublicCard avatar initials also migrated.

### CAUSE #4: TWO PARALLEL CSS VARIABLE SYSTEMS

- **SÍNTOMA:** MyCard's `.contrast-dark`/`.contrast-light` blocks consumed `var(--surface-*)` while PublicCard consumed `var(--dt-*)`.
- **DÓNDE:** MyCard `/cards/mine` — contrast-aware CSS blocks.
- **ARCHIVO(S):** `MyCard.razor` CSS blocks + `ThemeHelper.cs`.
- **POR QUÉ:** `--surface-*` was the original system; `--dt-*` introduced later without migrating consumers.
- **IMPACTO:** If only `--dt-*` was set, `--surface-*` consumers showed fallback colors or nothing.
- **ESTADO:** ✅ **YA RESUELTO.** `ThemeHelper.GenerateCssVariables()` outputs 12 `--surface-*` bridge aliases (~line 68-80). MyCard consumers migrated to `--dt-*`. Bridge kept for safety.

### CAUSE #5: CARDSTYLEMODEL DUPLICATION

- **SÍNTOMA:** `CardStyleModel` defined as a private class inside both `MyCard.razor` and `PublicCard.razor`.
- **DÓNDE:** MyCard + PublicCard code sections.
- **ARCHIVO(S):** Formerly in `MyCard.razor` and `PublicCard.razor` `@code` blocks.
- **POR QUÉ:** Copy-paste during early development.
- **IMPACTO:** Property additions in one file didn't propagate; deserialization mismatches possible.
- **ESTADO:** ✅ **YA RESUELTO.** Extracted to `Models/CardStyleModel.cs`. `grep "class CardStyleModel"` → exactly 1 result.

---

## H) DEBUG CHECKLIST

When a visual desynchronization is reported, check **in this order**:

| # | CHECK | COMMAND / EVIDENCE |
|---|---|---|
| 1 | **Is the component shared?** | `SocialLinksRow`, `QuoteRequestBlock` must appear via `<ComponentName>` tag in all 3 surfaces — never inline SVG/markup. |
| 2 | **Are CSS vars injected?** | MyCard: `GetPreviewBackgroundStyle()` → `ThemeHelper.GenerateCssVariables()` on `.phone-card-content` (~line 4803). TemplateLibrary: `_skyLightCssVars` on `.quote-request-preview-full` (~line 298), `_mintBreezeCssVars` on `.appointments-preview-full`. PublicCard: `GetThemeCssVariables()` on `.landing-wrapper` (~line 39). |
| 3 | **Is the correct preset loaded?** | Log `_selectedPreset` and `_themeTokens.AccentPrimary` in `DeserializeJsonFields()`. |
| 4 | **Are there hardcoded colors in synced blocks?** | Search for bare hex in `<style>` blocks of synced components. All should be `var(--dt-*, #fallback)`. |
| 5 | **Is `CardStyleModel` shared?** | `grep "class CardStyleModel"` → must return exactly 1 result in `Models/CardStyleModel.cs`. |
| 6 | **Is the correct contrast class applied?** | Check `.contrast-dark`/`.contrast-light` on the container — should match `_cardStyle.BackgroundIsDark`. |
| 7 | **Are inline styles overriding CSS vars?** | `grep 'style=".*_themeTokens'` in `*.razor` — must return 0 results. |

---

## I) RIESGOS CONOCIDOS Y ANTIPATRONES

### RESOLVED

1. ~~**`CardStyleModel` duplicado**~~ — ✅ Extracted to `Models/CardStyleModel.cs`.
2. ~~**Hex hardcodeados en TemplateLibrary CSS**~~ — ✅ All `qrp-*` CSS uses `var(--dt-*)`.
3. ~~**Dos sistemas de variables CSS en paralelo**~~ — ✅ Bridge aliases in ThemeHelper, consumers migrated.
4. ~~**Inline styles bypass CSS vars**~~ — ✅ Zero `style=".*_themeTokens"` in codebase.
5. ~~**Sin servicio `CardService`**~~ — ✅ Created `Services/CardService.cs`.
6. ~~**Social icons con shape distinto**~~ — ✅ All 3 surfaces use `SocialLinksRow.razor`.
7. ~~**No hay tests de sincronización**~~ — ✅ `SyncContractTests.cs` with 9 tests.
8. ~~**`QuoteSettingsJson` pérdida en switch**~~ — ✅ `SaveCard()` always serializes `QuoteSettingsJson`.

### ACCEPTED TECH DEBT

9. **Template registry desacoplado de DB** — `TemplateLibrary` uses in-memory `_templates` list; `CardTemplate` DB entity has only 2 rows. Intentional for fast iteration. Future: migrate to DB-driven registry.

10. **Preset forzado sobreescribe preferencia del usuario** — For `quote-request`, `services-quotes`, and `appointments`, preset is forced in `DeserializeJsonFields()`. Intentional — those templates require specific themes (`sky-light`, `emerald-night`, `mint-breeze` respectively).

11. **CTA buttons, status chips, avatar still have 3 implementations** — These are not yet shared components (see section D, PROPOSED). Lower priority because they use `var(--dt-*)` for colors, reducing visual divergence even without shared markup.

12. **Portfolio/default/services template previews use hardcoded hex in gallery chrome** — The hex values in `pp-*`, `dp-*`, `sp-*` CSS classes are for the gallery iPhone frame and static preview, not for synced card content. These previews don't have a matching PublicCard variant, so sync is not applicable.

13. **`SocialLinksModel` is still private/duplicated** — Defined inline in both MyCard and PublicCard. Lower priority since it's a simple DTO with no defaults that could diverge.

### GUARDRAILS

> **These rules must be followed for all future changes to the card rendering pipeline.**

1. **NEVER** duplicate component markup across surfaces. Use shared components in `Components/Shared/`.
2. **NEVER** add `style="...@_themeTokens..."` inline styles. Use CSS classes with `var(--dt-*)`.
3. **NEVER** hardcode hex colors in synced template preview CSS. Use `var(--dt-*, #fallback)`.
4. **NEVER** create a private `CardStyleModel` class. Use `Models/CardStyleModel.cs`.
5. **ALWAYS** add new CSS vars to both `--dt-*` and `--surface-*` bridge in `ThemeHelper.GenerateCssVariables()`.
6. **ALWAYS** run `dotnet test` after changes to card/theme files — 9 sync contract tests must pass.
7. **ALWAYS** update `CardService.GetDefaultPresetForTemplate()` when adding new template types.

---

## EF CORE CONCURRENCY GUARDRAILS

### ROOT CAUSE (FIXED 2025-02-11)

`MyCard.razor` injected a single scoped `DataTouchDbContext` and ran multiple async EF Core operations concurrently on it, causing `InvalidOperationException: A second operation was started on this context instance before a previous operation completed` → Blazor circuit crash ("Connection disconnected").

**Two overlapping patterns caused the crash:**

1. **Fire-and-forget DB calls** — `DeserializeJsonFields()` (sync void method) contained `_ = DbContext.SaveChangesAsync()` at two locations. These launched unawaited async operations. When `LoadServicesAsync()` ran immediately after (called from `OnInitializedAsync`), two operations overlapped on the same context.

2. **Blazor lifecycle overlap** — `OnParametersSetAsync()` called `HandleTemplateQueryParam()` which did `SaveChangesAsync()` + `LoadServicesAsync()`. If `OnInitializedAsync` hadn't completed, both lifecycle methods raced on the same DbContext.

### APPLIED FIX

| MECHANISM | WHERE | PURPOSE |
|---|---|---|
| **`SemaphoreSlim _dbGate(1,1)`** | `MyCard.razor` field | Serializes ALL EF Core operations — only one DB call at a time |
| **`_initCompleted` guard** | `OnParametersSetAsync` | Prevents DB ops before `OnInitializedAsync` finishes |
| **`_pendingDbSave` flag** | `DeserializeJsonFields()` | Replaces fire-and-forget `_ = DbContext.SaveChangesAsync()` — caller flushes after method returns |
| **try/catch on all DB ops** | `OnInitializedAsync`, `HandleTemplateQueryParam`, `SaveCard`, `LoadServicesAsync`, `OnProfileImageUpload` | Prevents unhandled exceptions from killing the Blazor circuit |

### GATED DB OPERATIONS (ALL SITES IN MYCARD.RAZOR)

| METHOD | DB OPERATION | GATED? |
|---|---|---|
| `OnInitializedAsync` | `Cards.FirstOrDefaultAsync` + `Services.ToListAsync` + flush `_pendingDbSave` | ✅ `_dbGate` |
| `HandleTemplateQueryParam` | `SaveChangesAsync` + `Services.ToListAsync` + conditional `SaveChangesAsync` | ✅ `_dbGate` |
| `SaveCard` | `SaveChangesAsync` | ✅ `_dbGate` |
| `LoadServicesAsync` | `Services.ToListAsync` | ✅ `_dbGate` |
| `OnProfileImageUpload` | `SaveChangesAsync` | ✅ `_dbGate` |
| `AddNewService` / `RemoveService` / `DuplicateService` | `Services.Add` / `Services.Remove` (sync, no query) | ⚠ Not gated (sync tracker ops, saved via `SaveCard`) |

### ANTIPATTERNS TO AVOID

1. **NEVER** use `_ = DbContext.SomeAsync()` (fire-and-forget) in Blazor components. Always `await` or set a flag for the caller to flush.
2. **NEVER** call `DbContext` from `OnParametersSetAsync` without checking `_initCompleted`.
3. **NEVER** add a new `await DbContext.*` call in MyCard.razor without wrapping it in `await _dbGate.WaitAsync(); try { ... } finally { _dbGate.Release(); }`.
4. **ALWAYS** wrap DB operations in try/catch to prevent circuit crash.

### NOTE ON DBCONTEXTFACTORY

`IDbContextFactory<DataTouchDbContext>` is registered in `Program.cs:37` and used by `DashboardService.cs` and `AppointmentDashboardService.cs`. MyCard.razor still uses the scoped `@inject DataTouchDbContext` because `_card` entity tracking relies on a single context instance. The `SemaphoreSlim` gate serializes access instead. If future refactoring detaches entity tracking, switching to factory-per-operation would be preferable.

---

## J) CHANGELOG

| DATE | CHANGE | REASON |
|---|---|---|
| 2025-02-11 | **Fix EF Core concurrency crash in MyCard.razor.** Removed fire-and-forget `_ = DbContext.SaveChangesAsync()` in `DeserializeJsonFields()`. Added `SemaphoreSlim _dbGate` to serialize all DB operations. Added `_initCompleted` guard in `OnParametersSetAsync`. Added try/catch around all DB operations. Added "EF Core Concurrency Guardrails" section to CLAUDE.md. | `InvalidOperationException: A second operation was started on this context instance` crashed the Blazor circuit when selecting templates from `/templates` → `/cards/mine?template=...`. Root cause: fire-and-forget saves + lifecycle overlap. |
| 2025-02-11 | **Complete rewrite of pipeline section (A-J).** Replaced stale sections A-F + DIAGNÓSTICO + DEBUG CHECKLIST + EXTENSION POINTS + RIESGOS + GUARDRAILS with accurate, up-to-date 10-section structure. | Previous documentation had stale information (e.g., TemplateLibrary listed as "no ThemeHelper" when `_skyLightCssVars` was already implemented; CardStyleModel listed as "private, duplicated" when already extracted; SocialLinksRow listed as "proposed" when already created; "no CardService" when `CardService.cs` exists). |
| 2025-02-11 | Added sections E (design patterns with evidence), G (diagnostic with SÍNTOMA/DÓNDE/ARCHIVO/POR QUÉ/IMPACTO/ESTADO schema), J (this changelog). | New sections required by Instructions.md for future LLM maintainability. |
| 2025-02-11 | Updated sync status table: Social Icons now ✅ SYNCED (was ❌ 3 IMPL). Updated shared components list to include `SocialLinksRow.razor`. Added `CardService.cs` to key files. | Reflect actual codebase state after all 5 cause fixes. |
| 2025-02-11 | **Implemented Template 4 "Citas (Agenda)" (`appointments`).** Added `AppointmentBookingBlock.razor` shared component (synced across 3 surfaces). Added `PublicAppointmentModal.razor` (4-step booking wizard: Service → Date → Hour → Confirm). Registered template in `TemplateLibrary.razor` with `_mintBreezeCssVars`, `IsAppointments` flag, and preview. Updated `MyCard.razor` with `_isAppointmentsTemplate` flag + live preview block. Updated `PublicCard.razor` with booking block + modal wiring. Updated `CardService.GetDefaultPresetForTemplate()` → `"appointments" => "mint-breeze"`. Added `AppointmentDashboardService.cs` (IDbContextFactory, KPIs + 3 chart queries). Added `echarts-appointments.js` (status donut, daily volume bar, hourly heatmap). Added Analytics tab to `Appointments.razor` with KPIs + 3 ECharts. Added scheduler tracking events to `CardAnalyticsService.cs`. Added script tag in `App.razor`. | Template 4 per Instructions.md — follows CLAUDE.md 10-step checklist (Section F). |
| 2025-02-13 | **Implemented Template 5 "Reservas (Rango de Fechas)" (`reservations-range`).** Added `ReservationRequest` + `ReservationResource` entities + `ReservationStatus` enum. Added `ReservationSettingsModel` (serialized to `Card.ReservationSettingsJson`). Added `ReservationBookingBlock.razor` shared component (synced across 3 surfaces: TemplateLibrary, MyCard, PublicCard). Added `PublicReservationModal.razor` (4-step wizard: Fechas → Huéspedes → Extras/Políticas → Confirmar). Registered template in `TemplateLibrary.razor` with `_softCreamCssVars`, `IsReservations` flag, and preview. Updated `MyCard.razor` with `_isReservationsTemplate` flag + editor section + live preview block + save logic. Updated `PublicCard.razor` with booking block + modal wiring + lead form hidden. Updated `CardService.GetDefaultPresetForTemplate()` → `"reservations-range" => "soft-cream"`. Added `ReservationService.cs` (submit, list, status update, blocked dates). Added `ReservationDashboardService.cs` (KPIs + 3 chart queries). Added `echarts-reservations.js` (status donut, by day of week, by period). Added Reservations tab to `Appointments.razor` with KPIs + 2 ECharts + table + CRUD. Updated `DbContext` with `DbSet<ReservationRequest>` + `DbSet<ReservationResource>` + entity configs. Updated `DbInitializer` with schema SQL for tables + indexes + column. Registered services in `Program.cs`. Added script tag in `App.razor`. | Template 5 per Instructions.md — follows CLAUDE.md 10-step checklist (Section F). Preset: `soft-cream` (Warmth, hospitality). |
| 2025-02-11 | **Fix appointments template desynchronization (4 root causes).** (1) Added `else if (_isAppointmentsTemplate)` branch in MyCard ~line 1288 to prevent contact form bleed. (2) Expanded services editor gate at ~line 653 to include `_isAppointmentsTemplate`. (3) Added mock services fallback (`_mockAppointmentServices`) in MyCard Live Preview when no real services exist. (4) Converted `PublicAppointmentModal` from bottom-sheet to centered modal (matching quote-modal pattern). Added empty-state in modal Step 1 when no services. Added "Appointments Template Pipeline" section to CLAUDE.md. | Desync: Template Preview ≠ Live Preview ≠ Public Card. Contact form appeared in appointments Live Preview; services list empty; modal was bottom-sheet instead of centered. |
