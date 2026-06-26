using KaiserWerk.PersonaX.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaiserWerk.PersonaX.Core.Services
{
    public class DummyDataService : IDataService
    {
        private List<Person> persons;

        public DummyDataService()
        {
            this.persons = this.GetDummyData();
        }

        public IEnumerable<Person> GetAllPersons()
        {
            return this.persons.ToList();
        }

        public void AddPerson(Person person)
        {
            this.persons.Add(person);
        }

        public void UpdatePerson(Person person)
        {
            var p = this.persons.FirstOrDefault(p => p.Id == person.Id);
            if (p == null)
                return;
            var i = this.persons.IndexOf(p);

            if (i != -1)
                this.persons[i] = person;

        }

        public void RemovePerson(Person person)
        {
            this.persons.Remove(person);
        }

        public void UploadPhoto(Person person, Photo photo)
        {
        }

        public void RemovePhoto(Person person, Photo photo)
        {
        }

        private List<Person> GetDummyData()
        {
            return new List<Person>() 
            {
                
            };
        }

        public Task<IEnumerable<Person>> GetAllPersonsAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddPersonAsync(Person person)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePersonAsync(Person person)
        {
            throw new NotImplementedException();
        }

        public Task RemovePersonAsync(Person person)
        {
            throw new NotImplementedException();
        }

        public Task UploadPhotoAsync(Person person, Photo photo)
        {
            throw new NotImplementedException();
        }

        public Task RemovePhotoAsync(Person person, Photo photo)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
