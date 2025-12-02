namespace TaskOrganizer.Schema
{
    public static class TaskSchema
    {
        public static string TableName => "tasks";

        public static Dictionary<string, string> Definition => new()
        {
            { "title", "VARCHAR(255) NOT NULL" },
            { "description", "TEXT" },
            { "is_completed", "TINYINT(1) NOT NULL DEFAULT 0" },
            { "user_id", "INT NOT NULL" },
            { "created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP" },
            { "completed_at", "DATETIME NULL" }
        };
    }
}
