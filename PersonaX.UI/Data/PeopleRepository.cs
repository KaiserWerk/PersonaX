using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PersonaX.UI.Models;

namespace PersonaX.UI.Data
{
    /// <summary>
    /// Repository for managing Person entities with encrypted storage.
    /// </summary>
    public class PeopleRepository
    {
        private readonly PiiDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<PeopleRepository> _logger;

        public PeopleRepository(PiiDatabaseConnectionFactory connectionFactory, ILogger<PeopleRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all persons from the database.
        /// </summary>
        public async Task<List<Person>> ListAsync()
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Person ORDER BY LastName, FirstName";
            var persons = new List<Person>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                persons.Add(new Person
                {
                    ID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PhoneNumber = reader.GetString(4),
                    DateOfBirth = DateTime.Parse(reader.GetString(5)),
                    Notes = reader.GetString(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7)),
                    ModifiedAt = DateTime.Parse(reader.GetString(8))
                });
            }

            foreach (var person in persons)
            {
                person.Address = await GetAddressAsync(connection, person.ID);
                person.MediaItems = await GetMediaItemsAsync(connection, person.ID);
            }

            return persons;
        }

        /// <summary>
        /// Retrieves a specific person by ID.
        /// </summary>
        public async Task<Person?> GetAsync(int id)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Person WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var person = new Person
                {
                    ID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PhoneNumber = reader.GetString(4),
                    DateOfBirth = DateTime.Parse(reader.GetString(5)),
                    Notes = reader.GetString(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7)),
                    ModifiedAt = DateTime.Parse(reader.GetString(8))
                };

                person.Address = await GetAddressAsync(connection, person.ID);
                person.MediaItems = await GetMediaItemsAsync(connection, person.ID);
                return person;
            }

            return null;
        }

        /// <summary>
        /// Saves a person to the database. Creates new if ID is 0, updates otherwise.
        /// </summary>
        public async Task<int> SaveItemAsync(Person item)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            item.ModifiedAt = DateTime.UtcNow;

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                item.CreatedAt = DateTime.UtcNow;
                saveCmd.CommandText = @"
                    INSERT INTO Person (FirstName, LastName, Email, PhoneNumber, DateOfBirth, Notes, CreatedAt, ModifiedAt)
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @DateOfBirth, @Notes, @CreatedAt, @ModifiedAt);
                    SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                    UPDATE Person
                    SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                        PhoneNumber = @PhoneNumber, DateOfBirth = @DateOfBirth, Notes = @Notes, 
                        ModifiedAt = @ModifiedAt
                    WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@FirstName", item.FirstName);
            saveCmd.Parameters.AddWithValue("@LastName", item.LastName);
            saveCmd.Parameters.AddWithValue("@Email", item.Email);
            saveCmd.Parameters.AddWithValue("@PhoneNumber", item.PhoneNumber);
            saveCmd.Parameters.AddWithValue("@DateOfBirth", item.DateOfBirth.ToString("o"));
            saveCmd.Parameters.AddWithValue("@Notes", item.Notes);
            saveCmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt.ToString("o"));
            saveCmd.Parameters.AddWithValue("@ModifiedAt", item.ModifiedAt.ToString("o"));

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            await SaveAddressAsync(connection, item);

            return item.ID;
        }

        /// <summary>
        /// Deletes a person from the database.
        /// </summary>
        public async Task<int> DeleteItemAsync(Person item)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Person WHERE ID = @ID";
            deleteCmd.Parameters.AddWithValue("@ID", item.ID);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Searches persons by name, email or phone.
        /// </summary>
        public async Task<List<Person>> SearchAsync(string query)
        {
            await _connectionFactory.InitializeDatabaseAsync();
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT * FROM Person 
                WHERE FirstName LIKE @query 
                   OR LastName LIKE @query 
                   OR Email LIKE @query 
                   OR PhoneNumber LIKE @query
                ORDER BY LastName, FirstName";
            selectCmd.Parameters.AddWithValue("@query", $"%{query}%");

            var persons = new List<Person>();
            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                persons.Add(new Person
                {
                    ID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PhoneNumber = reader.GetString(4),
                    DateOfBirth = DateTime.Parse(reader.GetString(5)),
                    Notes = reader.GetString(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7)),
                    ModifiedAt = DateTime.Parse(reader.GetString(8))
                });
            }

            foreach (var person in persons)
            {
                person.Address = await GetAddressAsync(connection, person.ID);
                person.MediaItems = await GetMediaItemsAsync(connection, person.ID);
            }

            return persons;
        }

        private static async Task<Address?> GetAddressAsync(SqliteConnection connection, int personId)
        {
            var addressCmd = connection.CreateCommand();
            addressCmd.CommandText = "SELECT * FROM Address WHERE PersonID = @personId LIMIT 1";
            addressCmd.Parameters.AddWithValue("@personId", personId);

            await using var reader = await addressCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Address
            {
                ID = reader.GetInt32(0),
                PersonID = reader.GetInt32(1),
                Street = reader.GetString(2),
                City = reader.GetString(3),
                State = reader.GetString(4),
                PostalCode = reader.GetString(5),
                Country = reader.GetString(6)
            };
        }

        private static async Task<List<MediaItem>> GetMediaItemsAsync(SqliteConnection connection, int personId)
        {
            var mediaCmd = connection.CreateCommand();
            mediaCmd.CommandText = "SELECT * FROM MediaItem WHERE PersonID = @personId ORDER BY CreatedAt DESC";
            mediaCmd.Parameters.AddWithValue("@personId", personId);

            var mediaItems = new List<MediaItem>();
            await using var reader = await mediaCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                mediaItems.Add(new MediaItem
                {
                    ID = reader.GetInt32(0),
                    PersonID = reader.GetInt32(1),
                    Type = (MediaType)reader.GetInt32(2),
                    OriginalFileName = reader.GetString(3),
                    EncryptedFilePath = reader.GetString(4),
                    MimeType = reader.GetString(5),
                    IV = reader.GetString(6),
                    Tag = reader.GetString(7),
                    CreatedAt = DateTime.Parse(reader.GetString(8))
                });
            }

            return mediaItems;
        }

        private static async Task SaveAddressAsync(SqliteConnection connection, Person item)
        {
            if (item.Address is null)
            {
                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM Address WHERE PersonID = @personId";
                deleteCmd.Parameters.AddWithValue("@personId", item.ID);
                await deleteCmd.ExecuteNonQueryAsync();
                return;
            }

            item.Address.PersonID = item.ID;

            var existingAddress = await GetAddressAsync(connection, item.ID);
            var addressCmd = connection.CreateCommand();
            if (existingAddress is null)
            {
                addressCmd.CommandText = @"
                    INSERT INTO Address (PersonID, Street, City, State, PostalCode, Country)
                    VALUES (@PersonID, @Street, @City, @State, @PostalCode, @Country);
                    SELECT last_insert_rowid();";
            }
            else
            {
                addressCmd.CommandText = @"
                    UPDATE Address
                    SET Street = @Street, City = @City, State = @State, PostalCode = @PostalCode, Country = @Country
                    WHERE PersonID = @PersonID";
            }

            addressCmd.Parameters.AddWithValue("@PersonID", item.Address.PersonID);
            addressCmd.Parameters.AddWithValue("@Street", item.Address.Street);
            addressCmd.Parameters.AddWithValue("@City", item.Address.City);
            addressCmd.Parameters.AddWithValue("@State", item.Address.State);
            addressCmd.Parameters.AddWithValue("@PostalCode", item.Address.PostalCode);
            addressCmd.Parameters.AddWithValue("@Country", item.Address.Country);

            var result = await addressCmd.ExecuteScalarAsync();
            if (existingAddress is null && result is not null)
            {
                item.Address.ID = Convert.ToInt32(result);
            }
            else if (existingAddress is not null)
            {
                item.Address.ID = existingAddress.ID;
            }
        }
    }
}
