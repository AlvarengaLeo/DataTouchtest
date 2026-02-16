ROL
Eres un Senior UX/UI + Frontend Engineer. Tu tarea es implementar un REDISEÑO RESPONSIVE (SOLO MOBILE) del módulo “Mi Tarjeta” en /cards/mine (archivo: Components/Pages/MyCard.razor). El resultado debe sentirse limpio, moderno, premium y enterprise. NO cambies lógica de negocio, endpoints, modelos, ni flujo de guardado/carga: SOLO UI/layout/CSS responsive.

REGLAS TÉCNICAS (NO NEGOCIABLES)
1) NO tocar DB/concurrencia: no agregues nuevas operaciones async, no fire-and-forget, no cambies compuertas/patrón de guardado.
2) NO hardcodear colores/hex ni estilos inline con tokens. Todo debe usar CSS variables var(--dt-*) (con fallbacks existentes si el proyecto ya los define).
3) El tema debe seguir aplicándose vía ThemeHelper.GenerateCssVariables() sobre el contenedor del preview (.phone-card-content). No inventes un pipeline alterno.
4) NO romper Desktop: en >=1025px el layout actual debe quedar igual. Todo cambio debe ir encapsulado a MOBILE (<=600px) mediante reglas responsive (clases + media queries).

OBJETIVO UX MOBILE (<=600px)
- 1 columna real, sin compresión “desktop”.
- Edición por secciones (progresive disclosure): solo 1 sección abierta a la vez.
- Vista previa bajo demanda (overlay full-screen), no ocupando espacio fijo.
- Acciones accesibles para el pulgar: Guardar/Vista previa sin scroll infinito.
- Percepción enterprise: jerarquía, aire, consistencia, micro-interacciones suaves.

BREAKPOINTS
- MOBILE: <=600px (principal)
- TABLET: 601–1024px (no romper; comportamiento intermedio opcional)
- DESKTOP: >=1025px (mantener como está)

IMPLEMENTAR REDISEÑO MOBILE (ESPECIFICACIÓN EXACTA)

A) HEADER + LINK PÚBLICO (MOBILE)
1) Crear una barra superior STICKY (solo mobile):
   - Izquierda: volver (solo si ya existe; no inventar navegación).
   - Centro: “MI TARJETA” (o “PERFIL PÚBLICO”, elegir uno y mantenerlo).
   - Derecha: chip de estado “GUARDADO / SIN GUARDAR” (usar señal existente de cambios) + icono/botón “VISTA PREVIA”.
2) Debajo del header, bloque compacto del link público:
   - 1 línea: URL truncada (ellipsis) + botón “COPIAR”.
   - 2 línea: microcopy “Este link es público.”
   - Nunca debe empujar el layout ni hacer overflow.

B) PATRÓN DE SECCIONES (MOBILE)
1) Convertir el contenido del editor en secciones colapsables (orden fijo):
   1) APARIENCIA
   2) IDENTIDAD
   3) INFORMACIÓN DE CONTACTO
   4) BOTONES DE ACCIÓN
   5) PERFILES SOCIALES
   6) GALERÍA
2) Comportamiento:
   - Por defecto, SOLO 1 sección abierta.
   - Al abrir una, las demás se cierran (reduce saturación).
3) Cada sección CERRADA debe mostrar un resumen de 1 línea (solo lectura) con datos actuales (ejemplos):
   - Apariencia: “Tema: {preset} • Fondo: {tipo}”
   - Identidad: “Nombre • Cargo • Empresa”
   - Contacto: “Email • Teléfono • WhatsApp”
   - Acciones: “{N} activos”
   - Redes: “{N} configuradas”
   - Galería: “Fotos {N} • Videos {N}”

C) BARRA INFERIOR STICKY (MOBILE, SOLO CUANDO HAY CAMBIOS)
- Si hay cambios pendientes (dirty state real):
  - Mostrar barra inferior sticky con:
    - Primario (ancho dominante): “GUARDAR CAMBIOS”
    - Secundario: “VISTA PREVIA”
- Si NO hay cambios:
  - No mostrar barra inferior (UI limpia).
- Reutiliza handlers existentes (no crear un flujo nuevo).

D) VISTA PREVIA COMO OVERLAY FULL-SCREEN (MOBILE)
- En <=600px, la vista previa NO debe vivir fija en pantalla.
- Al tocar “Vista previa”:
  - Abrir overlay full-screen (sin navegar a otra ruta):
    - Header: “VISTA PREVIA” + cerrar (X) o “Volver al editor”.
    - Body: render del preview existente (reubicar/mostrar el contenedor actual; NO duplicar lógica).
    - Footer sticky: “VER TARJETA PÚBLICA” + “COPIAR LINK”.
  - Bloquear scroll del fondo mientras overlay esté abierto.
  - Al cerrar: volver al editor manteniendo posición de scroll (no reset).
- Importante: el preview mantiene .phone-card-content + ThemeHelper.GenerateCssVariables().

E) MICRO-REDISEÑO POR SECCIÓN (MOBILE)

E1) APARIENCIA (reducir densidad)
- Evitar grids enormes en mobile.
- “OSCUROS / CLAROS” como selector compacto (solo una vista activa).
- Presets en carrusel horizontal (2–3 visibles) con selección clara (borde/check).
- Opciones avanzadas (fondo/imagen/variantes) como pills en una fila horizontal con scroll; abrir solo 1 panel a la vez.
- Mantener funcionalidad intacta (mismos presets/selección).

E2) IDENTIDAD (aire + legibilidad)
- Avatar + “Cambiar foto” arriba.
- Inputs apilados 1 columna con spacing consistente.
- Biografía: altura inicial razonable en mobile + contador alineado a la derecha (discreto).
- No alterar validaciones ni bindings.

E3) INFORMACIÓN DE CONTACTO (FIX CRÍTICO: nada de 2 columnas)
PROBLEMA ACTUAL: los campos se truncan y se cortan (“Teléfo…”, “What…”, valores “7765’”), porque el patrón 2 columnas no cabe en mobile.
SOLUCIÓN OBLIGATORIA EN MOBILE:
- Eliminar el patrón de 2 columnas para Teléfono/WhatsApp.
- Implementar patrón “GRUPO” por campo:

  TELÉFONO
  - Label único arriba: “Teléfono”
  - Dentro del grupo:
    - Selector “Código país” compacto
    - Input “Número” flexible (ocupa el resto)

  WHATSAPP
  - Label único arriba: “WhatsApp”
  - Dentro:
    - Selector “Código país” compacto
    - Input “Número” flexible

- Reglas responsive internas:
  - <=380px: apilar (Código 100% arriba, Número 100% abajo).
  - 381–600px: en fila (Código ancho fijo compacto; Número flex:1).
- Evitar cortes:
  - El input de Número debe poder encogerse (sin min-width rígidos).
  - Labels sin ellipsis en mobile (label del grupo arriba, no “País / Teléfono” por columna).
- Mantener mismos datos/bindings/guardado.

E4) BOTONES DE ACCIÓN (2 columnas, pero enterprise)
- Mantener grid 2 columnas.
- Misma altura para tiles.
- Labels largos (ej. “Guardar Contacto”) deben permitir wrap a 2 líneas máximo (no truncar feo).
- Estados activo/inactivo claros, alineación consistente, gap entre filas 12–16px.
- No cambiar funcionalidad de toggles/acciones.

E5) PERFILES SOCIALES (compactar para evitar scroll infinito)
- En mobile, NO mostrar todas las URLs completas en una lista larga.
- Cambiar a patrón “lista compacta + editar por ítem”:
  - Cada red: icono + nombre + estado (“Configurado / Vacío”) + acción “Editar”.
  - Al tocar “Editar”: abrir un editor simple (sheet/overlay) con campo URL + guardar/cancelar.
- En lista principal, si muestras URL, debe ir truncada a 1 línea (ellipsis).
- No cambiar lógica de guardado, solo presentación.

E6) GALERÍA (gestión tipo app moderna)
- Tabs “FOTOS / VIDEOS” en la sección.
- Grid 2 columnas:
  - Primer tile: “Agregar”
  - Luego miniaturas
- Acciones en cada item: editar/eliminar (y reordenar si existe), sin recargar toda la pantalla.
- Mantener límites y lógica existente.

F) SAFE AREA / PADDING FINAL (IMPORTANTE)
- Agregar padding inferior suficiente para que el contenido no choque con la navegación inferior del sistema ni con la barra sticky (cuando aparezca).
- Garantizar 0 overflow horizontal en 320–600px.

CALIDAD VISUAL (PREMIUM/ENTERPRISE)
- Spacing consistente (12–16px) y aire entre secciones.
- Touch targets >=44px.
- Transiciones suaves (150–200ms) en: expand/collapse y overlay open/close.
- Focus visible y accesible.
- No introducir sombras/agresivas ni animaciones llamativas: sobrio, enterprise.

DELIVERABLES
1) Ajustes en MyCard.razor: secciones colapsables, header sticky mobile, barra inferior sticky condicional, overlay full-screen de vista previa, compactación de redes, y refactor de Contacto a “grupo”.
2) CSS responsive encapsulado para <=600px (desktop intacto).
3) Accesibilidad mínima: focus, scroll-lock del fondo en overlay, cierre con ESC si aplica.

ACCEPTANCE CRITERIA (QA)
- En <=600px: 1 columna real, sin compresión; solo 1 sección abierta; preview solo overlay; no hay preview fijo.
- “Guardar cambios” accesible cuando hay cambios (sticky bottom).
- Contacto: no hay labels truncados ni valores cortados; Teléfono/WhatsApp legibles y funcionales.
- Redes: lista compacta (no URLs largas en pantalla principal); edición por ítem.
- No hay overflow horizontal en 320–600px.
- Desktop no cambia.
- No hardcoded hex; todo con var(--dt-*).
- No se agregan operaciones DB async nuevas.
- Compila y pasa tests.

SALIDA DEL AGENTE (EN SU RESPUESTA)
- 1–2 párrafos: qué cambió (solo UI/layout responsive).
- Lista de archivos tocados.
- Pasos para validar en móvil (<=600px) y confirmar que desktop no cambió.
- Sin pegar bloques largos de código en la respuesta: el cambio debe quedar implementado.
