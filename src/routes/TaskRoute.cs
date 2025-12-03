using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskOrganizer.Controllers;
using TaskOrganizer.Models;
using TaskOrganizer.Middlewares;
using System.Collections.Generic;

namespace TaskOrganizer.Routes
{
    public static class TaskRoute
    {
        public static void Register(WebApplication app)
        {
            var controller = new TasksController();

            app.MapPost("/api/tasks", async (HttpContext context) =>
            {
                // ---------------------------
                // Authenticate user
                // ---------------------------
                var authMiddleware = context.RequestServices.GetRequiredService<AuthMiddleware>();
                var authResult = await authMiddleware.ValidateAsync(context);

                if (!authResult.IsValid) 
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = authResult.ErrorMessage });
                    return;
                }

                try
                {
                    // Read task from JSON
                    var task = await context.Request.ReadFromJsonAsync<TodoTask>();
                    if (task == null)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = new Dictionary<string, string>
                            {
                                { "task", "Invalid task data." }
                            }
                        });
                        return;
                    }
                    Console.WriteLine("Auth Result", authResult); 
                    // Set the logged-in user's ID
                    task.UserId = int.Parse(authResult.UserId);

                    // Call controller to handle validation and creation
                    var result = controller.CreateTask(task); 

                    context.Response.StatusCode = result.StatusCode;
                    await context.Response.WriteAsJsonAsync(result);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = new Dictionary<string, string>
                        {
                            { "error", ex.Message }
                        }
                    });
                } 
            });
 
            app.MapGet("/api/tasks", async (HttpContext context) =>
                {
                    // ---------------------------
                    // Authenticate user
                    // ---------------------------
                    var authMiddleware = context.RequestServices.GetRequiredService<AuthMiddleware>();
                    var authResult = await authMiddleware.ValidateAsync(context);

                    if (!authResult.IsValid)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { error = authResult.ErrorMessage });
                        return;
                    }

                    try
                    {
                        // ---------------------------
                        // Read and parse due_date param
                        // ---------------------------
                        string? dueDateParam = context.Request.Query["due_date"];

                        DateTime? dueDate = null;
                        if (!string.IsNullOrEmpty(dueDateParam))
                        {
                            if (DateTime.TryParse(dueDateParam, out var parsedDate))
                            {
                                dueDate = parsedDate;
                            }
                        }

                        // ---------------------------
                        // Pass due_date to controller
                        // ---------------------------
                        var result = controller.GetTasks(
                            userId: int.Parse(authResult.UserId),
                            dueDate: dueDate
                        );

                        context.Response.StatusCode = result.StatusCode;
                        await context.Response.WriteAsJsonAsync(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = new Dictionary<string, string>
                            {
                                { "error", ex.Message }
                            }
                        });
                    }
                });
        }
    }
}
