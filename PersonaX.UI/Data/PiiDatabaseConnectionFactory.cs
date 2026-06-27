using Microsoft.Data.Sqlite;

namespace PersonaX.UI.Data
{
    /// <summary>
    /// Factory for creating local SQLite database connections.
    /// </summary>
    public class PiiDatabaseConnectionFactory
    {
        private bool _isInitialized = false;

        public PiiDatabaseConnectionFactory()
        {}

        /// <summary>
        /// Creates and opens a new SQLite connection.
        /// </summary>
        public async Task<SqliteConnection> CreateConnectionAsync()
        {
            var connectionString = $"Data Source={Constants.PiiDatabasePath}";
            var connection = new SqliteConnection(connectionString);

            await connection.OpenAsync();

            return connection;
        }

        /// <summary>
        /// Initializes the database schema if not already created.
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            if (_isInitialized) return;

            await using var connection = await CreateConnectionAsync();

            // Create Person table
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Person (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirstName TEXT NOT NULL,
                        LastName TEXT NOT NULL,
                        Email TEXT NOT NULL,
                        PhoneNumber TEXT NOT NULL,
                        DateOfBirth TEXT NOT NULL,
                        Notes TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        ModifiedAt TEXT NOT NULL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            // Create Address table
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Address (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PersonID INTEGER NOT NULL,
                        Street TEXT NOT NULL,
                        City TEXT NOT NULL,
                        State TEXT NOT NULL,
                        PostalCode TEXT NOT NULL,
                        Country TEXT NOT NULL,
                        FOREIGN KEY (PersonID) REFERENCES Person(ID) ON DELETE CASCADE
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            // Create MediaItem table
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MediaItem (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PersonID INTEGER NOT NULL,
                        Type INTEGER NOT NULL,
                        OriginalFileName TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        MimeType TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (PersonID) REFERENCES Person(ID) ON DELETE CASCADE
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            // Create AuditLog table
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AuditLog (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Action INTEGER NOT NULL,
                        EntityType TEXT NOT NULL,
                        EntityID INTEGER NOT NULL,
                        Details TEXT NOT NULL,
                        Timestamp TEXT NOT NULL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            _isInitialized = true;
        }
    }
}
