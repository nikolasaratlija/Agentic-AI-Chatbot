using System.ComponentModel;

namespace AIChatbot.Util;

public class DatabaseAgentTools
{
    private readonly DatabaseToolsService _dbService;

    public DatabaseAgentTools(DatabaseToolsService dbService)
    {
        _dbService = dbService;
    }

    [Description("Lists all available tables and views in the database.")]
    public string GetDatabaseSchema()
    {
        return _dbService.GetDatabaseSchema();
    }

    [Description("Gets column names, data types, and primary keys for a specific table.")]
    public string GetTableSchema(
        [Description("The exact name of the table to inspect (e.g., 'artists', 'tracks')")] string tableName)
    {
        return _dbService.GetTableSchema(tableName);
    }

    [Description("Executes a read-only SQL SELECT query against the SQLite database and returns the data.")]
    public string ExecuteDatabaseQuery(
        [Description("The valid SQLite SELECT statement to run.")] string sqlQuery)
    {
        return _dbService.ExecuteQuery(sqlQuery);
    }
}