using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskOrganizer.Controllers;
using TaskOrganizer.Models;

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
        }
    } 
}
