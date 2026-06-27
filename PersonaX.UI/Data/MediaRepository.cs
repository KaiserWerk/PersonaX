using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PersonaX.UI.Models;

namespace PersonaX.UI.Data
{
    /// <summary>
    /// Repository for managing MediaItem entities.
    /// </summary>
    public class MediaRepository
    {
        private readonly PiiDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<MediaRepository> _logger;

        public MediaRepository(PiiDatabaseConnectionFactory connectionFactory, ILogger<MediaRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all media items for a specific person.
        /// </summary>
        public async Task<List<MediaItem>> ListByPersonAsync(int personId)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM MediaItem WHERE PersonID = @personId ORDER BY CreatedAt DESC";
            selectCmd.Parameters.AddWithValue("@personId", personId);

            var items = new List<MediaItem>();
            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new MediaItem
                {
                    ID = reader.GetInt32(0),
                    PersonID = reader.GetInt32(1),
                    Type = (MediaType)reader.GetInt32(2),
                    OriginalFileName = reader.GetString(3),
                    FilePath = reader.GetString(4),
                    MimeType = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                });
            }

            return items;
        }

        /// <summary>
        /// Retrieves a specific media item by ID.
        /// </summary>
        public async Task<MediaItem?> GetAsync(int id)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM MediaItem WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MediaItem
                {
                    ID = reader.GetInt32(0),
                    PersonID = reader.GetInt32(1),
                    Type = (MediaType)reader.GetInt32(2),
                    OriginalFileName = reader.GetString(3),
                    FilePath = reader.GetString(4),
                    MimeType = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                };
            }

            return null;
        }

        /// <summary>
        /// Saves a media item to the database.
        /// </summary>
        public async Task<int> SaveItemAsync(MediaItem item)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                item.CreatedAt = DateTime.UtcNow;
                saveCmd.CommandText = @"
                    INSERT INTO MediaItem (PersonID, Type, OriginalFileName, EncryptedFilePath, MimeType, IV, Tag, CreatedAt)
                    VALUES (@PersonID, @Type, @OriginalFileName, @EncryptedFilePath, @MimeType, @CreatedAt);
                    SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                    UPDATE MediaItem
                    SET PersonID = @PersonID, Type = @Type, OriginalFileName = @OriginalFileName,
                        EncryptedFilePath = @EncryptedFilePath, MimeType = @MimeType
                    WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@PersonID", item.PersonID);
            saveCmd.Parameters.AddWithValue("@Type", (int)item.Type);
            saveCmd.Parameters.AddWithValue("@OriginalFileName", item.OriginalFileName);
            saveCmd.Parameters.AddWithValue("@EncryptedFilePath", item.FilePath);
            saveCmd.Parameters.AddWithValue("@MimeType", item.MimeType);
            saveCmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt.ToString("o"));

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }

        /// <summary>
        /// Deletes a media item from the database.
        /// Note: Caller is responsible for deleting the encrypted file from disk.
        /// </summary>
        public async Task<int> DeleteItemAsync(MediaItem item)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM MediaItem WHERE ID = @ID";
            deleteCmd.Parameters.AddWithValue("@ID", item.ID);

            return await deleteCmd.ExecuteNonQueryAsync();
        }
    }
}
