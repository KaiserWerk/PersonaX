using PersonaX.UI.Models;

namespace PersonaX.UI.Data
{
    public class PersonSeedDataService
    {
        private readonly PeopleRepository _peopleRepository;
        private readonly MediaRepository _mediaRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IKeyStoreService _keyStoreService;

        public PersonSeedDataService(
            PeopleRepository peopleRepository,
            MediaRepository mediaRepository,
            IEncryptionService encryptionService,
            IKeyStoreService keyStoreService)
        {
            _peopleRepository = peopleRepository;
            _mediaRepository = mediaRepository;
            _encryptionService = encryptionService;
            _keyStoreService = keyStoreService;
        }

        public async Task SeedDebugDataAsync()
        {
#if !DEBUG
            await Task.CompletedTask;
            return;
#else
            var existingPeople = await _peopleRepository.ListAsync();
            if (existingPeople.Count > 0)
            {
                return;
            }

            var samplePeople = new[]
            {
                new Person
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Email = "ada@example.local",
                    PhoneNumber = "+49 30 1234567",
                    DateOfBirth = new DateTime(1815, 12, 10),
                    Notes = "Debug-Datensatz für Demo-Zwecke."
                },
                new Person
                {
                    FirstName = "Alan",
                    LastName = "Turing",
                    Email = "alan@example.local",
                    PhoneNumber = "+44 20 7654321",
                    DateOfBirth = new DateTime(1912, 6, 23),
                    Notes = "Beispielperson mit verschlüsselter Mediendatei."
                }
            };

            foreach (var person in samplePeople)
            {
                await _peopleRepository.SaveItemAsync(person);
            }

            await SeedSampleMediaAsync(samplePeople[1].ID);
#endif
        }

#if DEBUG
        private async Task SeedSampleMediaAsync(int personId)
        {
            var key = await _keyStoreService.GetFileEncryptionKeyAsync();
            if (key is null)
            {
                return;
            }

            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9sYx0xAAAAAASUVORK5CYII=");
            var encrypted = _encryptionService.EncryptAesGcm(imageBytes, key);

            var personDirectory = Path.Combine(Constants.MediaRootPath, personId.ToString());
            Directory.CreateDirectory(personDirectory);

            var fileName = "seed-photo.enc";
            var fullPath = Path.Combine(personDirectory, fileName);
            await File.WriteAllBytesAsync(fullPath, encrypted.ciphertext);

            await _mediaRepository.SaveItemAsync(new MediaItem
            {
                PersonID = personId,
                Type = MediaType.Photo,
                OriginalFileName = "seed-photo.png",
                EncryptedFilePath = Path.GetRelativePath(Constants.MediaRootPath, fullPath),
                MimeType = "image/png",
                IV = Convert.ToBase64String(encrypted.iv),
                Tag = Convert.ToBase64String(encrypted.tag),
                CreatedAt = DateTime.UtcNow
            });
        }
#endif
    }
}
