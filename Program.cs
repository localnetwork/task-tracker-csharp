using TaskOrganizer.Config;
using TaskOrganizer.Migration;
using TaskOrganizer;  
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using TaskOrganizer.Middlewares;

// -------------------------
// Check for migration command 
// -------------------------
if (args.Length > 0 && args[0].ToLower() == "migrate")
{ 
    Migrator.RunAll();
    return;  
}

Console.WriteLine("✔ App running normally..."); 

// -------------------------
// Start Web API
// -------------------------
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AuthMiddleware>();

// -------------------------
// Configure CORS (allow all)
// -------------------------

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Use CORS middleware
app.UseCors();
 
// Register all routes dynamically from src/routes
Router.RegisterAll(app);

// Run the app 
app.Run("http://localhost:5001");
