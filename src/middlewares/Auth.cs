using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

namespace TaskOrganizer.Middlewares
{
    public class AuthResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    public class AuthMiddleware
    {
        private readonly string _secretKey;
        private static bool _envLoaded = false;

        public AuthMiddleware()
        {
            LoadEnv();
            
            _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                throw new InvalidOperationException("JWT_SECRET environment variable is not set");
            
            Console.WriteLine($"JWT_SECRET loaded: {_secretKey.Substring(0, Math.Min(10, _secretKey.Length))}...");
        }

        private static void LoadEnv()
        {
            if (!_envLoaded)
            {
                Env.Load();
                _envLoaded = true;
            }
        }

        public async Task<AuthResult> ValidateAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            Console.WriteLine($"Authorization Header: {authHeader}");

            if (string.IsNullOrEmpty(authHeader))
            {
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "No Authorization header provided" 
                };
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Invalid Authorization header format. Expected 'Bearer <token>'" 
                };
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Token is empty" 
                };
            }

            Console.WriteLine($"Token: {token.Substring(0, Math.Min(20, token.Length))}...");
 
            return await AttachUserToContext(context, token);
        }
 
        private async Task<AuthResult> AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                Console.WriteLine($"Validating token with key length: {key.Length}");

                // Validate the token
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                
                Console.WriteLine($"Token expires at: {jwtToken.ValidTo} UTC");
                Console.WriteLine($"Current time: {DateTime.UtcNow} UTC");

                // Extract userId from claims
                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value 
                             ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
                             ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("Available claims:");
                    foreach (var claim in jwtToken.Claims)
                    {
                        Console.WriteLine($"  - {claim.Type}: {claim.Value}");
                    }
                    
                    return new AuthResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "UserId claim not found in token" 
                    };
                }

                context.Items["UserId"] = userId;
                
                Console.WriteLine($"Token validated successfully for user: {userId}");
                
                return new AuthResult 
                { 
                    IsValid = true, 
                    UserId = userId 
                };
            }
            catch (SecurityTokenExpiredException ex)
            {
                Console.WriteLine($"Token expired: {ex.Message}");
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Token has expired" 
                };
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                Console.WriteLine($"Invalid token signature: {ex.Message}");
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Invalid token signature" 
                };
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Token validation failed: {ex.Message}" 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during token validation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new AuthResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Unexpected error: {ex.Message}" 
                };
            }
        }
    }
}