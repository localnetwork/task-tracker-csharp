using FluentValidation;
using FluentValidation.Results;
using TaskOrganizer.Models;
using TaskOrganizer.Validators;

namespace TaskOrganizer.Controllers
{     public class UsersController
    {
        private readonly UserValidator _validator = new UserValidator();

        public void CreateUser(User user)
        {
            // Validate user using FluentValidation 
            ValidationResult result = _validator.Validate(user);

            if (!result.IsValid)
            {
                // Combine all error messages
                string errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            // Save to database
            user.Create();
        }
    }
}
