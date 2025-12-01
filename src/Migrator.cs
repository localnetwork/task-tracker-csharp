using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;
using TaskOrganizer.Config;
using TaskOrganizer.Schema;

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

            // AUTO-DISCOVER ALL SCHEMA CLASSES
            var schemas = DiscoverSchemas();

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

        private static List<(string TableName, Dictionary<string, object> Def)> DiscoverSchemas()
        {
            var schemas = new List<(string, Dictionary<string, object>)>();
            var assembly = Assembly.GetExecutingAssembly();

            // Find all types in TaskOrganizer.Schema namespace
            var schemaTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "TaskOrganizer.Schema" 
                         && t.IsClass 
                         && t.IsPublic);

            foreach (var type in schemaTypes)
            {
                // Look for TableName property
                var tableNameProp = type.GetProperty("TableName", 
                    BindingFlags.Public | BindingFlags.Static);
                
                // Look for Definition property
                var definitionProp = type.GetProperty("Definition", 
                    BindingFlags.Public | BindingFlags.Static);

                if (tableNameProp != null && definitionProp != null)
                {
                    var tableName = tableNameProp.GetValue(null) as string;
                    var definition = definitionProp.GetValue(null) as Dictionary<string, object>;

                    if (tableName != null && definition != null)
                    {
                        schemas.Add((tableName, definition));
                        Console.WriteLine($"📋 Discovered schema: {type.Name} -> {tableName}");
                    }
                }
            }

            return schemas;
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