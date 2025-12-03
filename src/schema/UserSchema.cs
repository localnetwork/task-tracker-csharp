namespace TaskOrganizer.Schema
{
    public static class UserSchema
    {
        public static string TableName => "users";

        public static int Order => 1; // must be created first

        public static Dictionary<string, string> Definition => new()
        {  
            { "firstname", "VARCHAR(255) NOT NULL" },
            { "lastname", "VARCHAR(255) NOT NULL" },
            { "email", "VARCHAR(255) NOT NULL UNIQUE" },
            { "password", "VARCHAR(255) NOT NULL" },
            { "created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP" }
        };
    }
}
