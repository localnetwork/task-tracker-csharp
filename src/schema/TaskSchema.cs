namespace TaskOrganizer.Schema
{
    public static class TaskSchema
    {
        public static string TableName => "tasks";
        public static int Order => 10; // create after users

        public static Dictionary<string, string> Definition => new()
        {
            { "title", "VARCHAR(255) NOT NULL" },
            { "description", "TEXT" },
            { "is_completed", "TINYINT(1) NOT NULL DEFAULT 0" },
            { "user_id", "INT NOT NULL" },  // match users.id type exactly
            { "created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP" },
            { "completed_at", "DATETIME NULL" },
            { "due_date", "DATETIME NOT NULL" }
        };

        public static List<(string Column, string RefTable, string RefColumn, string OnDelete, string OnUpdate)> ForeignKeys => new()
        {
            ("user_id", "users", "id", "CASCADE", "CASCADE")
        };
    }
} 
 