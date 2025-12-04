using FluentValidation;
using TaskOrganizer.Models;

namespace TaskOrganizer.Validators
{ 
    // Task creation validator
    public class TaskCreateValidator : AbstractValidator<TodoTask>
    {
        public TaskCreateValidator()
        { 
            RuleFor(task => task.Title)  
                .NotEmpty() 
                .WithMessage("Title cannot be empty.")
                .MaximumLength(255)
                .WithMessage("Title cannot exceed 255 characters.");

            RuleFor(task => task.Description)
                .NotEmpty()
                .WithMessage("Description cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");

            // Required
            RuleFor(task => task.DueDate) 
                .NotNull()
                .WithMessage("Date cannot be empty.");

            // Must be >= CreatedAt (only if provided)
            RuleFor(task => task.DueDate)
                .GreaterThanOrEqualTo(task => task.CreatedAt)
                .WithMessage("Date cannot be earlier than creation date.")  
                .When(task => task.DueDate.HasValue);
        }  
    } 
}