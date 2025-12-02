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
                try
                {   
                    var user = await context.Request.ReadFromJsonAsync<User>(); 

                    if (user == null)
                        return Results.BadRequest(new { error = "Invalid user data." });

                    // Optional: check required fields
                    if (string.IsNullOrWhiteSpace(user.Firstname) || string.IsNullOrWhiteSpace(user.Lastname))
                        return Results.BadRequest(new { error = "Firstname and Lastname are required." });

                    if (user.Password != user.ConfirmPassword)
                        return Results.BadRequest(new { error = "Passwords do not match." });

                    if (User.GetByEmail(user.Email) != null)
                        return Results.BadRequest(new { error = "Email already exists." });

                    user.Create();

                    var result = new
                    {
                        user.Id,
                        user.Firstname,
                        user.Lastname,
                        user.Email,
                        user.CreatedAt
                    };

                    return Results.Json(result, statusCode: 201);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
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