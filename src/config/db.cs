using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
namespace TaskOrganizer.Config
{ 
    public class DatabaseConnection
    {
        private static bool _loaded = false;

        private static void LoadEnv() 
        {
            if (!_loaded)
            {
                Env.Load();   // Loads .env from root
                _loaded = true;
            }
        }

        public static MySqlConnection GetConnection()
        {
            try 
            {
                LoadEnv();

                string connectionString =
                    $"Server={Env.GetString("DB_SERVER")};" +
                    $"Port={Env.GetString("DB_PORT")};" +  
                    $"Database={Env.GetString("DB_NAME")};" +
                    $"Uid={Env.GetString("DB_USER")};" +
                    $"Pwd={Env.GetString("DB_PASS")};"; 
                Console.WriteLine("DB Connected Successfully!");
                return new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error connecting to database: {ex.Message}");
            }
        }
    }
}
