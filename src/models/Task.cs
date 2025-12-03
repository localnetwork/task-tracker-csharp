using MySql.Data.MySqlClient;
using TaskOrganizer.Config;

namespace TaskOrganizer.Models 
{
    public class TodoTask
    { 
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsCompleted { get; set; } = false;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TodoTask() { }

        public TodoTask(string title, string description, int userId, DateTime? dueDate)
        {
            Title = title;
            Description = description; 
            UserId = userId;
            DueDate = dueDate; 
        }

        public void Create() 
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO tasks (title, description, user_id, created_at, due_date, is_completed)
                VALUES (@title, @description, @userId, @createdAt, @dueDate, @isCompleted)
            "; 

            Console.WriteLine("Hello World" + this.DueDate.Value); 

            using var cmd = new MySqlCommand(query, conn); 
            cmd.Parameters.AddWithValue("@title", this.Title);
            cmd.Parameters.AddWithValue("@description", this.Description);
            cmd.Parameters.AddWithValue("@userId", this.UserId);
            cmd.Parameters.AddWithValue("@createdAt", this.CreatedAt);
            cmd.Parameters.AddWithValue("@dueDate", this.DueDate.HasValue ? this.DueDate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isCompleted", this.IsCompleted);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            this.Id = Convert.ToInt32(cmd.ExecuteScalar()); 
        }


        public (List<TodoTask> CompletedTasks, List<TodoTask> PendingTasks) GetAllTasks(DateTime? dueDate = null)
        {
            List<TodoTask> completedTasks = new List<TodoTask>();
            List<TodoTask> pendingTasks = new List<TodoTask>();
 
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = "SELECT * FROM tasks WHERE user_id = @userId";
            
            if (dueDate.HasValue)
            {
                query += " AND DATE(due_date) = @dueDate";  // <-- FIXED
            }

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", this.UserId);

            if (dueDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@dueDate", dueDate.Value.Date); // <-- FIXED (date only)
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var task = new TodoTask
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    IsCompleted = reader.GetBoolean("is_completed"),
                    UserId = reader.GetInt32("user_id"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    DueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date"),
                    CompletedAt = reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime("completed_at")
                };

                if (task.IsCompleted)
                    completedTasks.Add(task);
                else
                    pendingTasks.Add(task);
            }

            return (completedTasks, pendingTasks);
        }


    }
}
 