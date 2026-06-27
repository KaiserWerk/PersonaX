using PersonaX.UI.Models;

namespace PersonaX.UI.Data
{
    public class PersonSeedDataService
    {
        private readonly PeopleRepository _peopleRepository;
        private readonly MediaRepository _mediaRepository;
        private readonly IEncryptionService _encryptionService;

        public PersonSeedDataService(
            PeopleRepository peopleRepository,
            MediaRepository mediaRepository,
            IEncryptionService encryptionService)
        {
            _peopleRepository = peopleRepository;
            _mediaRepository = mediaRepository;
            _encryptionService = encryptionService;
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
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9sYx0xAAAAAASUVORK5CYII=");
 

            var personDirectory = Path.Combine(Constants.MediaRootPath, personId.ToString());
            Directory.CreateDirectory(personDirectory);

            var fileName = "seed-photo.enc";
            var fullPath = Path.Combine(personDirectory, fileName);
            await File.WriteAllBytesAsync(fullPath, imageBytes);

            await _mediaRepository.SaveItemAsync(new MediaItem
            {
                PersonID = personId,
                Type = MediaType.Photo,
                OriginalFileName = "seed-photo.png",
                FilePath = Path.GetRelativePath(Constants.MediaRootPath, fullPath),
                MimeType = "image/png",
                CreatedAt = DateTime.UtcNow
            });
        }
#endif
    }
}
