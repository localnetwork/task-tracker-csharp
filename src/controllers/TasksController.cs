using FluentValidation;
using FluentValidation.Results;
using TaskOrganizer.Models;
using TaskOrganizer.Validators;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System;
using TaskOrganizer.Validators;
namespace TaskOrganizer.Controllers 
{ 
    
    // Generic response wrapper
    public class TaskControllerResult<T>
    {
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; } // Can be string or dictionary
        public string Message { get; set; } = "";
    }

    // DTOs for task response
    public class TaskData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsCompleted { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class TaskCreateResponse
    {
        public TaskData Task { get; set; } = new TaskData();
    }

    public class TasksGetResponse
    {
        public List<TaskData> CompletedTasks { get; set; } = new List<TaskData>();
        public List<TaskData> PendingTasks { get; set; } = new List<TaskData>();
    }

    public class TasksController
    {
        private readonly TaskCreateValidator _taskCreateValidator = new TaskCreateValidator();

        // --------------------
        // Create Task
        // -------------------- 
        public TaskControllerResult<TaskCreateResponse> CreateTask(TodoTask task)
        {
            // Validate task

            Console.WriteLine(task); 
            ValidationResult result = _taskCreateValidator.Validate(task);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key.ToLower(),
                        g => g.First().ErrorMessage
                    );

                return new TaskControllerResult<TaskCreateResponse>
                {
                    StatusCode = 400,
                    Error = errors
                };
            }

            // Save task
            task.Create();

            // Prepare response 
            return new TaskControllerResult<TaskCreateResponse>
            {
                StatusCode = 201,
                Data = new TaskCreateResponse
                {
                    Task = new TaskData
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Description = task.Description,
                        IsCompleted = task.IsCompleted,
                        UserId = task.UserId,
                        CreatedAt = task.CreatedAt,
                        DueDate = task.DueDate,
                        CompletedAt = task.CompletedAt
                    }
                },
                Message = "Task created successfully."
            };
        }

        public TaskControllerResult<TasksGetResponse> GetTasks(int userId, DateTime? dueDate = null)
        {
            var tempTask = new TodoTask { UserId = userId };
            var (completedTasks, pendingTasks) = tempTask.GetAllTasks(dueDate);

            return new TaskControllerResult<TasksGetResponse> 
            {
                StatusCode = 200,
                Data = new TasksGetResponse
                {
                    CompletedTasks = completedTasks.Select(t => new TaskData
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.IsCompleted,
                        UserId = t.UserId,
                        CreatedAt = t.CreatedAt,
                        DueDate = t.DueDate,
                        CompletedAt = t.CompletedAt
                    }).ToList(),

                    PendingTasks = pendingTasks.Select(t => new TaskData
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.IsCompleted,
                        UserId = t.UserId,
                        CreatedAt = t.CreatedAt,
                        DueDate = t.DueDate,
                        CompletedAt = t.CompletedAt
                    }).ToList()
                },
                Message = "Tasks retrieved successfully."
            };
        }
 
    } 
}
