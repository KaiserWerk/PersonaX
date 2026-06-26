using KaiserWerk.PersonaX.Persistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaiserWerk.PersonaX.Core.Services;

public interface IDataService
{
    public Task<IEnumerable<Person>> GetAllPersonsAsync();
    public Task AddPersonAsync(Person person);
    public Task UpdatePersonAsync(Person person);
    public Task RemovePersonAsync(Person person);

    public Task UploadPhotoAsync(Person person, Photo photo);
    public Task RemovePhotoAsync(Person person, Photo photo);
    public Task SaveChangesAsync();
}
