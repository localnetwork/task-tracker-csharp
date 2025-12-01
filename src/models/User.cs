using MySql.Data.MySqlClient;
using TaskOrganizer.Config;

namespace TaskOrganizer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = ""; 
        public DateTime CreatedAt { get; set; }

        // Create method to insert this user into the database
        public void Create()
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = "INSERT INTO users (username, password, email, created_at) " +
                           "VALUES (@u, @p, @e, @c)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", this.Username);
            cmd.Parameters.AddWithValue("@p", this.Password);
            cmd.Parameters.AddWithValue("@e", this.Email);
            cmd.Parameters.AddWithValue("@c", DateTime.UtcNow);

            cmd.ExecuteNonQuery();

            // Optionally, get the inserted Id
            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            this.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
