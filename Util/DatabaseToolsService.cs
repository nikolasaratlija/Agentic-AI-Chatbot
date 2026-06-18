using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;

public class DatabaseToolsService
{
    private readonly string _connectionString;

    public DatabaseToolsService(string dbFilePath = "Chinook_Sqlite.sqlite")
    {
        _connectionString = $"Data Source={dbFilePath};";
    }

    /// <summary>
    /// Tool 1: Lists all tables and views in the database.
    /// Helps the agent understand what data is available.
    /// </summary>
    public string GetDatabaseSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
        const string query = @"
            SELECT type, name 
            FROM sqlite_master 
            WHERE type IN ('table', 'view') AND name NOT LIKE 'sqlite_%';";
        
        var results = connection.Query(query);
        if (!results.Any()) return "The database is empty.";

        return string.Join("\n", results.Select(r => $"[{r.type.ToString().ToUpper()}] {r.name}"));
    }

    /// <summary>
    /// Tool 2: Gets the exact columns, types, and primary keys for a specific table.
    /// Prevents the agent from guessing column names.
    /// </summary>
    public string GetTableSchema(string tableName)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        // Use standard PRAGMA for SQLite table info
        // Note: PRAGMA statements don't support standard SQL parameters, so we sanitize the string slightly
        string sanitizedName = tableName.Replace("'", "''");
        var columns = connection.Query($"PRAGMA table_info('{sanitizedName}');");

        if (!columns.Any()) return $"Table '{tableName}' not found.";

        var schemaLines = new List<string> { $"Schema for table: {tableName}" };
        foreach (var col in columns)
        {
            string pk = col.pk == 1 ? " [PRIMARY KEY]" : "";
            string notNull = col.notnull == 1 ? " NOT NULL" : "";
            schemaLines.Add($"  - {col.name} ({col.type}){notNull}{pk}");
        }

        return string.Join("\n", schemaLines);
    }

    /// <summary>
    /// Tool 3: Executes a read-only SQL query and returns the results formatted cleanly.
    /// </summary>
    public string ExecuteQuery(string sqlQuery)
    {
        // Guard rails: Stop the agent from destructive actions
        string lowerQuery = sqlQuery.ToLower();
        if (lowerQuery.Contains("drop ") || lowerQuery.Contains("delete ") || 
            lowerQuery.Contains("update ") || lowerQuery.Contains("insert "))
        {
            return "Error: Only read-only (SELECT) queries are allowed.";
        }

        using var connection = new SqliteConnection(_connectionString);
        try
        {
            // Execute query and fetch as a list of dictionaries for dynamic formatting
            var results = connection.Query(sqlQuery).Cast<IDictionary<string, object>>().ToList();

            if (results.Count == 0) return "Query executed successfully. 0 rows returned.";

            // Format results into a simple, readable string for the LLM
            var output = new List<string>();
            var headers = results.First().Keys;
            output.Add(string.Join(" | ", headers));
            output.Add(new string('-', output[0].Length));

            // Add up to 20 rows to avoid blowing up the LLM's context window
            foreach (var row in results.Take(20))
            {
                output.Add(string.Join(" | ", row.Values.Select(v => v?.ToString() ?? "NULL")));
            }

            if (results.Count > 20)
            {
                output.Add($"... and {results.Count - 20} more rows (truncated for brevity).");
            }

            return string.Join("\n", output);
        }
        catch (Exception ex)
        {
            return $"SQL Error: {ex.Message}";
        }
    }
}