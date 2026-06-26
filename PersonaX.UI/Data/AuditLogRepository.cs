using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PersonaX.UI.Models;

namespace PersonaX.UI.Data
{
    /// <summary>
    /// Repository for managing AuditLog entries.
    /// </summary>
    public class AuditLogRepository
    {
        private readonly PiiDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<AuditLogRepository> _logger;

        public AuditLogRepository(PiiDatabaseConnectionFactory connectionFactory, ILogger<AuditLogRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves recent audit log entries.
        /// </summary>
        public async Task<List<AuditLog>> ListRecentAsync(int limit = 100)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM AuditLog ORDER BY Timestamp DESC LIMIT @limit";
            selectCmd.Parameters.AddWithValue("@limit", limit);

            var logs = new List<AuditLog>();
            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new AuditLog
                {
                    ID = reader.GetInt32(0),
                    Action = (AuditAction)reader.GetInt32(1),
                    EntityType = reader.GetString(2),
                    EntityID = reader.GetInt32(3),
                    Details = reader.GetString(4),
                    Timestamp = DateTime.Parse(reader.GetString(5))
                });
            }

            return logs;
        }

        /// <summary>
        /// Retrieves audit log entries for a specific entity.
        /// </summary>
        public async Task<List<AuditLog>> ListByEntityAsync(string entityType, int entityId)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT * FROM AuditLog 
                WHERE EntityType = @entityType AND EntityID = @entityId 
                ORDER BY Timestamp DESC";
            selectCmd.Parameters.AddWithValue("@entityType", entityType);
            selectCmd.Parameters.AddWithValue("@entityId", entityId);

            var logs = new List<AuditLog>();
            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new AuditLog
                {
                    ID = reader.GetInt32(0),
                    Action = (AuditAction)reader.GetInt32(1),
                    EntityType = reader.GetString(2),
                    EntityID = reader.GetInt32(3),
                    Details = reader.GetString(4),
                    Timestamp = DateTime.Parse(reader.GetString(5))
                });
            }

            return logs;
        }

        /// <summary>
        /// Logs an audit entry.
        /// </summary>
        public async Task LogAsync(AuditAction action, string entityType, int entityId, string details = "")
        {
            try
            {
                await _connectionFactory.InitializeDatabaseAsync();
                await using var connection = await _connectionFactory.CreateConnectionAsync();

                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO AuditLog (Action, EntityType, EntityID, Details, Timestamp)
                    VALUES (@Action, @EntityType, @EntityID, @Details, @Timestamp)";

                insertCmd.Parameters.AddWithValue("@Action", (int)action);
                insertCmd.Parameters.AddWithValue("@EntityType", entityType);
                insertCmd.Parameters.AddWithValue("@EntityID", entityId);
                insertCmd.Parameters.AddWithValue("@Details", details);
                insertCmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("o"));

                await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log entry");
            }
        }

        /// <summary>
        /// Deletes audit logs older than specified date.
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM AuditLog WHERE Timestamp < @cutoff";
            deleteCmd.Parameters.AddWithValue("@cutoff", cutoffDate.ToString("o"));

            return await deleteCmd.ExecuteNonQueryAsync();
        }
    }
}
