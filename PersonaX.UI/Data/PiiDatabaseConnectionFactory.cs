using Microsoft.Data.Sqlite;

namespace PersonaX.UI.Data
{
    /// <summary>
    /// Factory for creating SQLCipher encrypted database connections.
    /// </summary>
    public class PiiDatabaseConnectionFactory
    {
        private readonly IKeyStoreService _keyStoreService;
        private bool _isInitialized = false;

        public PiiDatabaseConnectionFactory(IKeyStoreService keyStoreService)
        {
            _keyStoreService = keyStoreService;
        }

        /// <summary>
        /// Creates and opens a new SQLCipher connection using the database password from KeyStoreService.
        /// </summary>
        public async Task<SqliteConnection> CreateConnectionAsync()
        {
            // Ensure database password is available
            var dbPassword = await _keyStoreService.GetDatabasePasswordAsync();
            if (string.IsNullOrEmpty(dbPassword))
            {
                throw new InvalidOperationException("Database password not available. User must unlock the app first.");
            }

            var connectionString = $"Data Source={Constants.PiiDatabasePath}";
            var connection = new SqliteConnection(connectionString);

            await connection.OpenAsync();

            // Set SQLCipher key (password) using PRAGMA
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA key = '{dbPassword}';";
                await cmd.ExecuteNonQueryAsync();
            }

            // Verify the key is correct by running a simple query
            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA cipher_version;";
                    await cmd.ExecuteScalarAsync();
                }
            }
            catch (SqliteException)
            {
                await connection.DisposeAsync();
                throw new InvalidOperationException("Invalid database password or corrupted database.");
            }

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
                        EncryptedFilePath TEXT NOT NULL,
                        MimeType TEXT NOT NULL,
                        IV TEXT NOT NULL,
                        Tag TEXT NOT NULL,
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
