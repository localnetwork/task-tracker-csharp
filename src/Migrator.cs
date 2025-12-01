using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using TaskOrganizer.Config;
using TaskOrganizer.Schema; // <-- make sure this is imported

namespace TaskOrganizer.Migration
{
    public static class Migrator
    {
        public static void RunAll()
        {
            Console.WriteLine("🔧 Running migrations...");

            using var conn = DatabaseConnection.GetConnection();
            try
            {
                conn.Open();
                Console.WriteLine("✅ Connected to database for migration.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Cannot connect to database:");
                Console.WriteLine(ex.Message);
                return;
            }

            // MANUALLY ADD SCHEMAS
            var schemas = new List<(string TableName, Dictionary<string, object> Def)>
            {
                ("users", UserSchema.Definition) // <-- add your schema classes here
            };

            foreach (var schema in schemas)
            {
                string sql = GenerateCreateTable(schema.TableName, schema.Def);
                Console.WriteLine($"🛠 Creating table: {schema.TableName}");

                try
                {
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"✅ Table created: {schema.TableName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to create table {schema.TableName}: {ex.Message}");
                }
            }

            Console.WriteLine("🎉 All migrations finished!");
        }

        private static string GenerateCreateTable(string table, Dictionary<string, object> def)
        {
            var columns = new List<string>();
            foreach (var kv in def)
            {
                columns.Add($"{kv.Key} {kv.Value}");
            }

            string columnSql = string.Join(",\n    ", columns);

            return
$@"CREATE TABLE IF NOT EXISTS {table} (
    id INT AUTO_INCREMENT PRIMARY KEY,
    {columnSql}
);";
        }
    }
}
 