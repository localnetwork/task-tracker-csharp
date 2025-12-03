using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskOrganizer.Controllers;
using TaskOrganizer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskOrganizer.Middlewares;

using TaskOrganizer.Validators; 

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
                    {
                        return Results.BadRequest(new
                        {
                            error = new Dictionary<string, string>
                            {
                                { "email", "Invalid user data." },
                                { "password", "Invalid user data." }
                            }
                        });
                    }

                    // Run validation 
                    var validator = new UserRegisterValidator();
                    var validationResult = validator.Validate(user);

                    if (!validationResult.IsValid)
                    {
                        // Convert FluentValidation errors → { field: "error message" }
                        var errors = validationResult.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key[0].ToString().ToLower() + g.Key.Substring(1), // camelCase
                                g => g.First().ErrorMessage
                            );

                        return Results.BadRequest(new { error = errors });
                    }

                    // Create user
                    user.Create();
                    string token = user.GenerateJwtToken();

                    return Results.Json(new
                    {
                        token = token,
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            firstname = user.Firstname,
                            lastname = user.Lastname,
                            created_at = user.CreatedAt
                        }
                    }, statusCode: 201);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    // ArgumentException from a setter (single field)
                    if (ex is ArgumentException argEx) 
                    {
                        var param = argEx.ParamName ?? "error";
                        // convert to camelCase: "Firstname" -> "firstname"
                        var field = char.ToLowerInvariant(param[0]) + param.Substring(1);

                        var errors = new Dictionary<string, string>
                        {
                            { field, argEx.Message }
                        };

                        return Results.BadRequest(new { error = errors });
                    }

                    // fallback
                    return Results.BadRequest(new
                    {
                        error = new Dictionary<string, string>
                        {
                            { "error", ex.Message }  
                        }
                    });  
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