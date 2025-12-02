using FluentValidation;
using FluentValidation.Results;
using TaskOrganizer.Models;
using TaskOrganizer.Validators;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace TaskOrganizer.Controllers
{
    // Generic response wrapper
    public class ControllerResult<T>
    {
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; } // Can be string or dictionary
        public string Message { get; internal set; }
        public string Token { get; internal set; }
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

        // --------------------
        // Registration
        // --------------------
        public ControllerResult<object> CreateUser(User user)
        {
            ValidationResult result = _registerValidator.Validate(user);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key.ToLower(),
                        g => g.First().ErrorMessage
                    );

                return new ControllerResult<object>
                {
                    StatusCode = 400,
                    Error = errors
                };
            }

            // Save user
            user.Create();

            User? dbUser = User.GetByEmail(user.Email);
            if (dbUser == null)
            {
                return new ControllerResult<object>
                {
                    StatusCode = 500, 
                    Message = "User creation failed."
                };
            }

            // Build response payload that matches your required structure
            var payload = new
            {
                token = dbUser.GenerateJwtToken(),
                user = new UserData
                {
                    Id = dbUser.Id,
                    Email = dbUser.Email,
                    Firstname = dbUser.Firstname,
                    Lastname = dbUser.Lastname,
                    CreatedAt = dbUser.CreatedAt
                }
            };

            return new ControllerResult<object>
            {
                StatusCode = 200,
                Data = payload
            };
        }

        // --------------------
        // Login
        // --------------------
        public ControllerResult<LoginResponse> Login(string email, string password)
        {
            var tempUser = new User { Email = email, Password = password };
            ValidationResult result = _loginValidator.Validate(tempUser);

            if (!result.IsValid)
            {
                // Convert validation errors into dictionary
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key.ToLower(),
                        g => g.First().ErrorMessage
                    );

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
                    Message = "Invalid email or password.",
                    Error = new { email = "Invalid email or password.", password = "Invalid email or password." } // structured error
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

        public ControllerResult<object> Profile(HttpContext context)
        {
            string? authHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))  
            {
                return new ControllerResult<object>
                {
                    StatusCode = 401,
                    Message = "Missing or invalid token.",
                    Error = new { token = "Token is required." } 
                };
            }

            string token = authHeader.Substring("Bearer ".Length);
            string email;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

                if (string.IsNullOrEmpty(email))
                {
                    return new ControllerResult<object>
                    {
                        StatusCode = 401,
                        Message = "Invalid token payload.",
                        Error = new { token = "Email missing in token." }
                    };
                }
            }
            catch
            {
                return new ControllerResult<object>
                {
                    StatusCode = 401,
                    Message = "Invalid token.",
                    Error = new { token = "Token cannot be decoded." }
                };
            }

            // Fetch user profile
            User? dbUser = User.GetProfile(email);

            if (dbUser == null)
            {
                return new ControllerResult<object>
                {
                    StatusCode = 404,
                    Message = "User not found.",
                    Error = new { user = "User does not exist." }
                };
            }

            // Return fields directly under data
            var payload = new
            {
                id = dbUser.Id,
                email = dbUser.Email,
                firstname = dbUser.Firstname,
                lastname = dbUser.Lastname,
                createdAt = dbUser.CreatedAt
            };

            return new ControllerResult<object>
            {
                StatusCode = 200,
                Data = payload
            };
        } 
    } 
}
 