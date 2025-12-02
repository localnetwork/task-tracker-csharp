namespace TaskOrganizer.Schema
{
    public static class UserSchema
    {
        public static string TableName => "users";

        public static Dictionary<string, string> Definition => new()
        { 
            { "email", "VARCHAR(255) NOT NULL" },
            { "firstname", "VARCHAR(255) NOT NULL" }, 
            { "lastname", "VARCHAR(255) NOT NULL" }, 
            { "password", "VARCHAR(255) NOT NULL" },
            { "created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP" }
        };
    } 
}
