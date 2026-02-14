ROL
Actúa como Arquitecto Senior .NET/Blazor + Senior UX/UI (enfoque enterprise/premium) y con obsesión por “single source of truth” entre:
1) Template Preview (/templates)
2) Live Preview del editor (/cards/mine)
3) Tarjeta pública (/p/{org}/{slug})

REGLA #0 (OBLIGATORIA)
Antes de planificar o tocar código, LEE el archivo CLAUDE.md del repo y respeta el pipeline/patrones ya documentados (tokens, estilos, componentes compartidos, contratos de sincronización). No dupliques markup ni estilos entre superficies.

CONTEXTO
- Estamos trabajando en la PLANTILLA 2 — PORTAFOLIO OPERATIVO (Portafolio Creativo).
- Ya existe una sección en el editor llamada “Galería de Proyectos” donde actualmente se adjuntan imágenes (fotos).
- Necesitamos extender esa sección para soportar también VIDEOS, pero manteniendo UX minimalista y sincronización perfecta entre Template Preview, Live Preview y Public Card.

OBJETIVO FUNCIONAL (LO QUE SE QUIERE LOGRAR)
En la sección “Galería de Proyectos” del editor, implementar una configuración “dividida” en 2 columnas (subsecciones):
- Columna A: FOTOS (adjuntar/gestionar fotos)
- Columna B: VIDEOS (adjuntar/gestionar videos)

Y agregar un control claro para habilitar o deshabilitar cada columna:
- Se permite: (Fotos ON / Videos OFF) o (Fotos OFF / Videos ON) o (Fotos ON / Videos ON)
- NO se permite que ambas estén OFF (si el usuario intenta apagar ambas, bloquear y mostrar mensaje).
- Importante: el usuario NO debe elegir un “modo Ambos” como opción única; debe ser por toggles individuales (habilitar/deshabilitar cada una).

REQUERIMIENTO UI/UX (EDITOR)
1) Dentro del card “Galería de Proyectos”:
   - Mantener el título y subtítulo actual.
   - Debajo del header, colocar un bloque de configuración “Contenido de galería” con:
     - Toggle “Fotos” (ON/OFF)
     - Toggle “Videos” (ON/OFF)
     - Microcopy compacto: “Activa solo lo que quieras mostrar en tu tarjeta.”
2) Layout:
   - Desktop/tablet: dos columnas (Fotos | Videos) cuando ambas están ON.
   - Mobile: una columna apilada (Fotos arriba, Videos abajo).
3) Cada columna debe tener:
   - Un área de carga (dropzone/botón) consistente con el estilo enterprise actual.
   - Empty state propio (si está ON pero no hay items).
   - Lista/preview de items ya cargados con acciones mínimas:
     - Reordenar (drag o flechas) (opcional si ya existe para fotos)
     - Eliminar
     - (Videos) mostrar preview: thumbnail + duración (si está disponible) o ícono.
4) El botón “Guardar Cambios” no debe cambiar. El comportamiento debe ser coherente con el resto del editor.

REGLA DE VISIBILIDAD (PUBLIC CARD / TEMPLATE PREVIEW / LIVE PREVIEW)
- Si “Fotos” está OFF:
  - NO debe renderizarse la sección de fotos en la tarjeta pública ni en previews (sin título, sin contenedor, sin espacio).
- Si “Videos” está OFF:
  - NO debe renderizarse la sección de videos en la tarjeta pública ni en previews.
- Si ambas están ON:
  - Renderizar ambas secciones como carruseles separados:
    - Carrusel de FOTOS
    - Carrusel de VIDEOS
  - Orden recomendado: Fotos primero, Videos después.
- Si una sección está ON pero sin contenido:
  - En Public Card: mostrar empty state minimal (o no mostrar la sección, define regla y aplícala igual en las 3 superficies).
  - En Template Preview: si se usa mock data, solo se inyecta mock data para la(s) sección(es) activas.

COMPORTAMIENTO VISUAL (CARRUSELES)
- En Public Card:
  - FOTOS: carrusel horizontal con snap, flechas discretas en desktop, swipe en mobile.
  - VIDEOS: carrusel horizontal similar, cada item es un “video card” (thumbnail/placeholder).
- En Live Preview:
  - Debe verse exactamente igual a Public Card, solo que con datos del editor (o mocks si no hay datos).
- En Template Preview:
  - Igual estructura y mismos componentes, con mocks si no hay contenido real.

DATOS / PERSISTENCIA (SIN INVENTAR, VALIDAR EN CÓDIGO)
- Identifica dónde se guarda actualmente la galería (JSON, modelo, entidad, etc.).
- Extiende el contrato para soportar:
  - enabledPhotos (bool)
  - enabledVideos (bool)
  - photos[] (lista existente)
  - videos[] (nueva lista)
- Mantén compatibilidad: si ya existen tarjetas con fotos, deben seguir funcionando sin romperse.
- Las decisiones de “ON/OFF” deben persistirse y reflejarse al recargar.

SINCRONIZACIÓN / SINGLE SOURCE OF TRUTH (OBLIGATORIO)
- NO crear markup duplicado entre:
  - TemplateLibrary preview
  - MyCard live preview
  - PublicCard
- Extraer/usar un componente compartido para el bloque “Portfolio Gallery” que reciba:
  - dataset (fotos/videos)
  - flags enabledPhotos/enabledVideos
  - RenderingContext (Public / TemplatePreview / EditorPreview) si ya existe o si CLAUDE.md indica cómo resolverlo hoy
- Cualquier diferencia entre superficies solo se permite vía “RenderingContext” (ej: deshabilitar navegación real en preview), NO vía HTML/CSS duplicado.

CHECKLIST DE DIAGNÓSTICO (ANTES DE DARLO POR LISTO)
1) Al apagar Videos en editor → desaparece la sección de videos en:
   - Template Preview
   - Live Preview
   - Public Card
2) Al apagar Fotos en editor → desaparece la sección de fotos en las 3 superficies.
3) No quedan espacios en blanco ni separadores “huérfanos”.
4) El estilo visual (radius, botones, sombras, tokens) coincide exactamente con el resto del sistema (según CLAUDE.md).
5) No hay colores hardcodeados; todo sale de tokens/vars.

DONE (DEFINICIÓN DE TERMINADO)
- “Galería de Proyectos” en editor permite administrar FOTOS y VIDEOS en subsecciones (2 columnas en desktop).
- El usuario puede habilitar/deshabilitar cada una (no ambas OFF).
- La tarjeta pública y ambos previews reflejan exactamente lo configurado.
- Sin duplicación de markup/CSS entre superficies.
- Persistencia funcionando y compatible con tarjetas existentes.
