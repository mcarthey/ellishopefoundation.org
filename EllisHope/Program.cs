using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EllisHope.Data;
using EllisHope.Services;
using EllisHope.Models;
using EllisHope.Models.Domain;
using EllisHope.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Ellis Hope API", Version = "v1" });

    // Enable annotations (for [SwaggerOperation] attributes)
    options.EnableAnnotations();

    // Enable XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Add DbContext - Skip if Testing environment (will be configured by test factory)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add Identity with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true if you implement email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    
    // Only require HTTPS in production if HTTPS is available
    // SmarterASP.net temporary URL uses HTTP, so we need to allow HTTP cookies
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
        : CookieSecurePolicy.SameAsRequest; // Use HTTPS if available, HTTP if not
    
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax for better compatibility
});

// Configure Unsplash settings
builder.Services.Configure<UnsplashSettings>(
    builder.Configuration.GetSection("Unsplash"));

// Register HttpClient for Unsplash service
builder.Services.AddHttpClient<IUnsplashService, UnsplashService>();

// Register application services
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICauseService, CauseService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IMediaMigrationService, MediaMigrationService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageTemplateService, PageTemplateService>();
builder.Services.AddScoped<IPageContentHelper, PageContentHelper>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IClientApplicationService, ClientApplicationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
builder.Services.AddScoped<IDatabaseLoggerService, DatabaseLoggerService>();

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure reCAPTCHA settings
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("Recaptcha"));
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();

// Configure Contact Form settings
builder.Services.Configure<ContactFormSettings>(builder.Configuration.GetSection("ContactForm"));
  
var app = builder.Build();

// Enable Swagger UI in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ellis Hope API v1");
    options.RoutePrefix = "swagger";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Use custom error handling middleware for all environments
// This logs errors to the database and shows user-friendly error pages
app.UseErrorHandling();

// Use status code pages for 404, 403, etc.
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Only redirect to HTTPS if the request is already HTTPS or in development
// SmarterASP.net temporary URL is HTTP-only
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Blog details route with slug
app.MapControllerRoute(
    name: "blogDetails",
    pattern: "blog/details/{slug}",
    defaults: new { controller = "Blog", action = "Details" });

// Event details route with slug
app.MapControllerRoute(
    name: "eventDetails",
    pattern: "events/details/{slug}",
    defaults: new { controller = "Events", action = "Details" });

// Cause details route with slug
app.MapControllerRoute(
    name: "causeDetails",
    pattern: "causes/details/{slug}",
    defaults: new { controller = "Causes", action = "Details" });

// Admin area route
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Apply migrations and seed database
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Apply pending migrations automatically
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            
            // Seed data (idempotent - safe to run multiple times)
            await DbSeeder.SeedAsync(services);
            logger.LogInformation("Database seeding completed.");
            
            // Initialize default pages
            var pageService = services.GetRequiredService<IPageService>();
            await pageService.InitializeDefaultPagesAsync();
            logger.LogInformation("Default pages initialized.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            
            // In production, you might want to prevent the app from starting if DB setup fails
            if (!app.Environment.IsDevelopment())
            {
                throw; // Re-throw to prevent app from starting with broken database
            }
        }
    }
}

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
