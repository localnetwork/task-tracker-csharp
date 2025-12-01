using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskOrganizer.Controllers;
using TaskOrganizer.Models;
using FluentValidation;

namespace TaskOrganizer.Routes 
{
    public static class UserRoute
    {
        // This is the method Router.cs will call
        public static void Register(WebApplication app) 
        {
            var controller = new UsersController();

            // POST /api/users -> create a new user
            app.MapPost("/users", async (HttpContext context) =>
            {
                try
                {
                    // Parse request body to User model
                    var user = await context.Request.ReadFromJsonAsync<User>();

                    if (user == null)
                    {
                        context.Response.StatusCode = 400; // Bad Request
                        await context.Response.WriteAsync("Invalid user data.");
                        return;
                    }

                    // Validate and create user
                    controller.CreateUser(user);

                    context.Response.StatusCode = 201; // Created
                    await context.Response.WriteAsJsonAsync(new { message = "User created successfully", userId = user.Id });
                }
                catch (ValidationException ex)
                {
                    context.Response.StatusCode = 400; // Bad Request
                    await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500; // Internal Server Error
                    await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                }
            });
            app.MapGet("/api/hello", () =>
            {
                return new { message = "Hello, World!" }; 
            });
        }   
    } 
} 
