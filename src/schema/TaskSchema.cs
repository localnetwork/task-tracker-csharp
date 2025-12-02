using MySqlX.XDevAPI.Relational;

namespace TaskOrganizer.Schema
{
    public static class TaskSchema
    {
        public static string TableName = "tasks";
        public static Dictionary<string, object> Definition => new()
        {
            { "title", "VARCHAR(255) NOT NULL" },  
            { "description", "TEXT" },
            { "is_completed", "BOOLEAN NOT NULL DEFAULT FALSE" }, 
            { "user_id", "INT NOT NULL" }, 
            { "created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP" } 
        };
    } 
} 