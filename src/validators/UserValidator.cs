using FluentValidation;
using TaskOrganizer.Models;

namespace TaskOrganizer.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Username)
                .NotEmpty().WithMessage("Username cannot be empty.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Email is not valid.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    } 
} 
