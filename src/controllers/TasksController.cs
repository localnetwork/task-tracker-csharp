using FluentValidation;
using FluentValidation.Results;
using TaskOrganizer.Models;
using TaskOrganizer.Validators;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System;

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

            Console.WriteLine("DUE DATE" + task.DueDate);

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

        public TaskControllerResult<TodoTask> CompleteTask(int userId, int taskId)
        {
            try 
            {
                // Fetch the task first 
                var task = TodoTask.GetTaskById(taskId); 

                if (task == null) 
                {
                    return new TaskControllerResult<TodoTask>
                    {
                        StatusCode = 404,
                        Message = "Task not found.",
                        Data = null
                    };
                }

                // Check if the task belongs to the user
                if (task.UserId != userId)
                {
                    return new TaskControllerResult<TodoTask>  
                    {
                        StatusCode = 403, 
                        Message = "You are not allowed to update this task.",
                        Data = null
                    };
                }

                // Mark as completed
                var completedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Manila");

                Console.WriteLine("COMPLETED AT: " + task.Title);
                task.MarkAsCompleted(completedAt); // pass the timestamp 
                task.IsCompleted = true;        
                task.CompletedAt = completedAt; // update in-memory
                task.Title = task.Title;
                task.Description = task.Description; 

                return new TaskControllerResult<TodoTask>
                {
                    StatusCode = 200,
                    Message = "Task marked as completed successfully.",
                    Data = task
                };
            }
            catch (Exception ex)
            {
                return new TaskControllerResult<TodoTask>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                };
            }
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
