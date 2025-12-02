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
            var createdTables = new List<string>();

            using var conn = DatabaseConnection.GetConnection();
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Cannot connect to database:");
                Console.WriteLine(ex.ToString());
                return;
            }

            var schemas = DiscoverSchemas();
            if (schemas.Count == 0) return;

            foreach (var schema in schemas)
            {
                string sql = GenerateCreateTable(schema.TableName, schema.Def);

                try
                {
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    createdTables.Add(schema.TableName);
                }
                catch
                {
                    // Ignore failures silently
                }
            }

            // Only print names of tables successfully created
            if (createdTables.Count > 0)
            {
                Console.WriteLine("✅ Tables created:");
                foreach (var table in createdTables)
                    Console.WriteLine($"- {table}");
            }
        }

        private static List<(string TableName, Dictionary<string, string> Def)> DiscoverSchemas()
        {
            var schemas = new List<(string, Dictionary<string, string>)>();
            var assembly = Assembly.GetExecutingAssembly();

            var schemaTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "TaskOrganizer.Schema"
                         && t.IsClass
                         && t.IsPublic);

            foreach (var type in schemaTypes)
            {
                var tableNameProp = type.GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
                var definitionProp = type.GetProperty("Definition", BindingFlags.Public | BindingFlags.Static);

                if (tableNameProp != null && definitionProp != null)
                {
                    var tableName = tableNameProp.GetValue(null) as string;
                    var definition = definitionProp.GetValue(null) as Dictionary<string, string>;

                    if (tableName != null && definition != null)
                        schemas.Add((tableName, definition));
                }
            }

            return schemas;
        }

        private static string GenerateCreateTable(string table, Dictionary<string, string> def)
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
