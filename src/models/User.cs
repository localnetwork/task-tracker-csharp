using MySql.Data.MySqlClient;
using TaskOrganizer.Config;
using BCrypt.Net; // Add this 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using DotNetEnv; 
 
namespace TaskOrganizer.Models
{

    
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = ""; 

        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = ""; 
        public DateTime CreatedAt { get; set; }
        public object ConfirmPassword { get; internal set; }

        private static bool _loaded = false;
        private static void LoadEnv() 
        {
            if (!_loaded)
            {
                Env.Load();   // Loads .env from root
                _loaded = true;
            }
        }  

        // Create method to insert this user into the database
        public void Create()
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open(); 

            // Hash the password before storing
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(this.Password);

            string query = "INSERT INTO users (firstname, lastname, password, email, created_at) " +
                           "VALUES (@f, @l, @p, @e, @c)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@f", this.Firstname);
            cmd.Parameters.AddWithValue("@l", this.Lastname);
            cmd.Parameters.AddWithValue("@p", hashedPassword);
            cmd.Parameters.AddWithValue("@e", this.Email);
            cmd.Parameters.AddWithValue("@c", DateTime.UtcNow); 
 
            cmd.ExecuteNonQuery(); 

            // Get the last inserted ID
            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            this.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Optional: Verify password
        public bool VerifyPassword(string passwordToCheck)
        { 
            return BCrypt.Net.BCrypt.Verify(passwordToCheck, this.Password);
        }

        public void Login()  
        { 
            
             
        }

        public static User? GetByEmail(string email)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = "SELECT id, firstname, lastname, password, email, created_at FROM users WHERE email = @e LIMIT 1";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@e", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32("id"),
                    Firstname = reader.GetString("firstname"),
                    Lastname = reader.GetString("lastname"), 
                    Password = reader.GetString("password"),
                    Email = reader.GetString("email"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
            }

            return null; // User not found
        }

        public string GenerateJwtToken() 
        {
            LoadEnv();

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "helllooooo";
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
                throw new Exception("JWT_SECRET must be set and at least 32 characters long.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] 
            {
                new Claim(JwtRegisteredClaimNames.Sub, this.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, this.Email),
                new Claim("firstname", this.Firstname),
                new Claim("lastname", this.Lastname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("APP_NAME") ?? "YourAppName",
                audience: Environment.GetEnvironmentVariable("APP_NAME") ?? "YourAppName",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    } 
}
