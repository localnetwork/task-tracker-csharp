using FluentValidation;
using MySql.Data.MySqlClient;
using TaskOrganizer.Models;
using TaskOrganizer.Config;

namespace TaskOrganizer.Validators
{
    // Registration Validator
    public class UserRegisterValidator : AbstractValidator<User>
    {
        public UserRegisterValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Email is not valid.")
                .Must(EmailNotExists).WithMessage("Email already exists.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(user => user.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password cannot be empty.") 
                .Equal(user => user.Password).WithMessage("Passwords do not match.");

            RuleFor(user => user.Firstname)
                .NotEmpty().WithMessage("Firstname cannot be empty.");
 
            RuleFor(user => user.Lastname)
                .NotEmpty().WithMessage("Lastname cannot be empty.");
        }

        private bool EmailNotExists(string email)
        {
            const string query = "SELECT COUNT(*) FROM users WHERE email = @e";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0; // valid only if email is available
                }
            }
        }
    }

    // Login Validator
    public class UserLoginValidator : AbstractValidator<User>
    {
        public UserLoginValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Email is not valid.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password cannot be empty.");
        }
    }
}
