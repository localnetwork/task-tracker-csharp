using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace TaskOrganizer
{
    public static class Router
    { 
        public static void RegisterAll(WebApplication app)
        {
            // Find all static classes in TaskOrganizer.Routes namespace
            var routeTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "TaskOrganizer.Routes" && t.IsClass && t.IsAbstract && t.IsSealed); // static classes

            foreach (var type in routeTypes)
            {  
                // Call static Register method
                var method = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, new object[] { app });
            }
            
            // Add global filter to only accept application/json for requests with body
            app.Use(async (context, next) =>
            {
                // Check if request has a body (Content-Length > 0 or Transfer-Encoding: chunked)
                if (context.Request.ContentLength > 0 || 
                    context.Request.Headers.ContainsKey("Transfer-Encoding"))
                {
                    // if (context.Request.ContentType?.Contains("application/json") != true)
                    // {
                    //     context.Response.StatusCode = 415; // Unsupported Media Type
                    //     context.Response.ContentType = "application/json";
                    //     await context.Response.WriteAsJsonAsync(new { error = "Only application/json content type is accepted" });
                    //     return;
                    // }
                }
                
                // Ensure all responses are JSON
                context.Response.ContentType = "application/json";
                
                await next();
            });
        }  
    }
} 