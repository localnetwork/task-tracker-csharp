using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;
using TaskOrganizer.Config;

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
                Console.WriteLine("DB Connected Successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Cannot connect to database:");
                Console.WriteLine(ex);
                return;
            }

            var schemas = DiscoverSchemas();

            if (schemas.Count == 0) return;

            // Sort schemas by Order (ascending)
            var sortedSchemas = schemas.OrderBy(s => s.Order).ToList();

            foreach (var schema in sortedSchemas)
            {
                string sql = GenerateCreateTable(schema.TableName, schema.Def, schema.Type);

                try
                {
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    createdTables.Add(schema.TableName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not create table {schema.TableName}: {ex.Message}");
                }
            }

            if (createdTables.Count > 0)
            {
                Console.WriteLine("✅ Tables created:");
                foreach (var table in createdTables)
                    Console.WriteLine($"- {table}");
            }
        }

        private static List<(string TableName, Dictionary<string, string> Def, Type Type, int Order)> DiscoverSchemas()
        {
            var schemas = new List<(string, Dictionary<string, string>, Type, int)>();
            var assembly = Assembly.GetExecutingAssembly();

            var schemaTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "TaskOrganizer.Schema"
                         && t.IsClass
                         && t.IsPublic);

            foreach (var type in schemaTypes)
            {
                var tableNameProp = type.GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
                var definitionProp = type.GetProperty("Definition", BindingFlags.Public | BindingFlags.Static);
                var orderProp = type.GetProperty("Order", BindingFlags.Public | BindingFlags.Static);

                if (tableNameProp != null && definitionProp != null)
                {
                    var tableName = tableNameProp.GetValue(null) as string;
                    var definition = definitionProp.GetValue(null) as Dictionary<string, string>;
                    int order = 10; // default
                    if (orderProp != null)
                        order = (int)orderProp.GetValue(null);

                    if (tableName != null && definition != null)
                        schemas.Add((tableName, definition, type, order));
                }
            }

            return schemas;
        }

        private static string GenerateCreateTable(string table, Dictionary<string, string> def, Type schemaType)
        {
            var columns = new List<string> { "id INT AUTO_INCREMENT PRIMARY KEY" };

            foreach (var kv in def)
                columns.Add($"{kv.Key} {kv.Value}");

            string columnSql = string.Join(",\n    ", columns);

            string createSql = $"CREATE TABLE IF NOT EXISTS {table} (\n    {columnSql}";

            // Add foreign keys if schema defines them
            var fkProp = schemaType.GetProperty("ForeignKeys", BindingFlags.Public | BindingFlags.Static);
            if (fkProp != null)
            {
                var fks = fkProp.GetValue(null) as IEnumerable<(string Column, string RefTable, string RefColumn, string OnDelete, string OnUpdate)>;
                if (fks != null)
                {
                    foreach (var fk in fks)
                    {
                        createSql += $",\n    FOREIGN KEY ({fk.Column}) REFERENCES {fk.RefTable}({fk.RefColumn}) ON DELETE {fk.OnDelete} ON UPDATE {fk.OnUpdate}";
                    }
                }
            }

            createSql += "\n);";
            return createSql;
        }
    }
}
