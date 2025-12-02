using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskOrganizer.Controllers;
using TaskOrganizer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskOrganizer.Middlewares;

namespace TaskOrganizer.Routes
{
    public static class UserRoute
    {
        public static void Register(WebApplication app)
        {
            var controller = new UsersController();

            app.MapPost("/api/register", async (HttpContext context) =>
            { 
                var user = await context.Request.ReadFromJsonAsync<User>();

                if (user == null)
                    return Results.BadRequest(new { error = "Invalid user data." });

                var response = controller.CreateUser(user);

                if (response.Error != null)
                    return Results.Json(new { error = response.Error }, statusCode: response.StatusCode);

                return Results.Json(response.Data, statusCode: response.StatusCode);
            });

            app.MapPost("/api/login", async (HttpContext context) =>
            {
                var user = await context.Request.ReadFromJsonAsync<User>();

                if (user == null)
                    return Results.BadRequest(new { error = "Invalid user data." });
 
                var response = controller.Login(user.Email, user.Password);

                if (response.Error != null)
                    return Results.Json(new { error = response.Error }, statusCode: response.StatusCode);

                return Results.Json(response.Data, statusCode: response.StatusCode);
            });

            app.MapGet("/api/profile", async (HttpContext context) =>
            {
                var authMiddleware = context.RequestServices.GetRequiredService<AuthMiddleware>();
                var authResult = await authMiddleware.ValidateAsync(context); 

                if (!authResult.IsValid) 
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = authResult.ErrorMessage });
                    return;
                }

                Console.WriteLine($"Authenticated user: {authResult.UserId}");

                // Now run your controller 
                var controller = new UsersController();
                var result = controller.Profile(context); 

                context.Response.StatusCode = result.StatusCode;
                await context.Response.WriteAsJsonAsync(result);
            });
        }
    }  
}