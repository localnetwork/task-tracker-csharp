using FluentValidation;
using FluentValidation.Results;
using TaskOrganizer.Models;
using TaskOrganizer.Validators;

namespace TaskOrganizer.Controllers
{
    // Generic response wrapper
    public class ControllerResult<T>
    {
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }

    // DTOs for login response
    public class UserData
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public UserData User { get; set; } = new UserData();
    }

    public class UsersController 
    {
        private readonly UserRegisterValidator _registerValidator = new UserRegisterValidator();
        private readonly UserLoginValidator _loginValidator = new UserLoginValidator();

        // Registration
        public ControllerResult<User> CreateUser(User user)
        {
            ValidationResult result = _registerValidator.Validate(user);

            if (!result.IsValid)
            {
                string errors = string.Join(". ", 
                    result.Errors.Select(e => e.ErrorMessage.TrimEnd('.'))
                ) + ".";

                return new ControllerResult<User>
                {
                    StatusCode = 400,
                    Error = errors
                };
            }

            // Save user into DB
            user.Create();

            return new ControllerResult<User>
            {
                StatusCode = 201,
                Data = new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        // Login
        public ControllerResult<LoginResponse> Login(string email, string password)
        {
            var tempUser = new User { Email = email, Password = password };
            ValidationResult result = _loginValidator.Validate(tempUser);

            if (!result.IsValid)
            {
                string errors = string.Join(". ",
                    result.Errors.Select(e => e.ErrorMessage.TrimEnd('.'))
                ) + ".";

                return new ControllerResult<LoginResponse>
                {
                    StatusCode = 400,
                    Error = errors
                };
            }

            // Authenticate user
            User? dbUser = User.GetByEmail(email);
            if (dbUser == null || !dbUser.VerifyPassword(password))
            {
                return new ControllerResult<LoginResponse>
                {
                    StatusCode = 401,
                    Error = "Invalid email or password."
                };
            }

            // Return token + user info
            return new ControllerResult<LoginResponse>
            {
                StatusCode = 200,
                Data = new LoginResponse
                {
                    Token = dbUser.GenerateJwtToken(),
                    User = new UserData
                    {
                        Id = dbUser.Id,  
                        Email = dbUser.Email,
                        Firstname = dbUser.Firstname,
                        Lastname = dbUser.Lastname,
                        CreatedAt = dbUser.CreatedAt
                    }
                }
            };
        }
    }
}
