using DataTouch.Infrastructure.Data;
using DataTouch.Web.Components;
using DataTouch.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add DbContext - SQL Server with pooling for concurrent query support
// Priority: 1. DB_CONNECTION env var, 2. ConnectionStrings__DefaultConnection, 3. appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Validate connection string
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("${"))
{
    throw new InvalidOperationException(
        "Database connection string is not configured. " +
        "Set the DB_CONNECTION environment variable with a valid SQL Server connection string.");
}

builder.Services.AddPooledDbContextFactory<DataTouchDbContext>(options =>
    options.UseSqlServer(connectionString));

// Also register DbContext directly for services that don't need factory pattern
builder.Services.AddScoped<DataTouchDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<DataTouchDbContext>>().CreateDbContext());

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Add application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CardAnalyticsService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<CountryPhoneService>();

// Add GeoLocation service with caching
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<GeoLocationService>();

// ═══════════════════════════════════════════════════════════════
// BOOKING SYSTEM Services
// ═══════════════════════════════════════════════════════════════
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<QuoteService>();

// Quote Automations (SLA alerts, reminders)
builder.Services.AddQuoteAutomations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// FIX: Serve uploaded files from wwwroot/uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(app.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// Login endpoint - handles cookie auth before Blazor starts
app.MapPost("/api/auth/login", async (HttpContext context, DataTouchDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    
    var passwordHash = AuthService.HashPassword(password);
    var user = await db.Users
        .Include(u => u.Organization)
        .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive);
    
    if (user == null)
    {
        context.Response.Redirect("/login?error=1");
        return;
    }
    
    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(System.Security.Claims.ClaimTypes.Email, user.Email),
        new(System.Security.Claims.ClaimTypes.Name, user.FullName),
        new(System.Security.Claims.ClaimTypes.Role, user.Role),
        new("OrganizationId", user.OrganizationId.ToString()),
        new("OrganizationName", user.Organization?.Name ?? "")
    };
    
    var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
    
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    context.Response.Redirect("/");
});

// Logout endpoint
app.MapGet("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/login");
});

// Health check endpoint for Railway/Docker
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataTouchDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

await app.RunAsync();
