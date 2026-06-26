using KaiserWerk.PersonaX.Persistence;
using KaiserWerk.PersonaX.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaiserWerk.PersonaX.Core.Services;

internal class SqLiteDataService : IDataService
{
    private readonly AppDbContext dbContext;

    public SqLiteDataService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<Person>> GetAllPersonsAsync()
    {
        return await this.dbContext.Persons.ToListAsync();
    }

    public async Task AddPersonAsync(Person person)
    {
        await this.dbContext.Persons.AddAsync(person);
    }

    public async Task UpdatePersonAsync(Person person)
    {
        // TODO: evtl. unnötig
    }

    public async Task RemovePersonAsync(Person person)
    {
        this.dbContext.Remove<Person>(person);
    }

    public async Task UploadPhotoAsync(Person person, Photo photo)
    { 
        // TODO. upload file
        person.Photos.Add(photo);
        await this.UpdatePersonAsync(person);
        await this.SaveChangesAsync();
        
    }

    public async Task RemovePhotoAsync(Person person, Photo photo)
    {
        // TODO. remove physical file
        person.Photos.Remove(photo);
        await this.UpdatePersonAsync(person);
        await this.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        
    }
}
