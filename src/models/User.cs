using MySql.Data.MySqlClient;
using TaskOrganizer.Config;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv; 

namespace TaskOrganizer.Models
{
    public class User : Person
    {
        public int Id { get; set; }
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public User() : base() { } // for JSON binding

        public User(string firstname, string lastname, string email, string password)
            : base(firstname, lastname)
        {
            Email = email; 
            Password = password;
            CreatedAt = DateTime.UtcNow;
        }


        private static bool _loaded = false;

        private static void LoadEnv()
        {
            if (!_loaded)
            {
                Env.Load();
                _loaded = true;
            }
        }

        // CONSTRUCTOR REQUIRED FOR Person INHERITANCE
        // public User() : base("", "") { } 
 
        // public User(string firstname, string lastname, string email, string password)
        //     : base(firstname, lastname)
        // {
        //     Email = email; 
        //     Password = password;
        //     CreatedAt = DateTime.UtcNow;
        // }

        // Create user in database
        public void Create()
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(this.Password);

            string query = @"
                INSERT INTO users (firstname, lastname, password, email, created_at)
                VALUES (@firstname, @lastname, @password, @email, @createdAt)
            ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@firstname", this.Firstname);
            cmd.Parameters.AddWithValue("@lastname", this.Lastname);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.Parameters.AddWithValue("@email", this.Email);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);

            cmd.ExecuteNonQuery(); 

            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            this.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool VerifyPassword(string passwordToCheck)
        {
            return BCrypt.Net.BCrypt.Verify(passwordToCheck, this.Password);
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
                    CreatedAt = reader.GetDateTime("created_at"),
                };
            }

            return null;
        }

        public string GenerateJwtToken()
        {
            LoadEnv();

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "";
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
                throw new Exception("JWT_SECRET must be at least 32 characters.");

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
                issuer: Environment.GetEnvironmentVariable("APP_NAME") ?? "TaskOrganizer",
                audience: Environment.GetEnvironmentVariable("APP_NAME") ?? "TaskOrganizer",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds 
            );  

            return new JwtSecurityTokenHandler().WriteToken(token);
        } 

        public static User? GetProfile(string email)
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
                CreatedAt = reader.GetDateTime("created_at"),
            };
        }

        return null;
    }

    }
}
