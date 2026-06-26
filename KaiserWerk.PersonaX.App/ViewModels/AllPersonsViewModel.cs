using KaiserMVVMCore;
using KaiserWerk.PersonaX.Core.Services;
using KaiserWerk.PersonaX.Persistence;
using KaiserWerk.PersonaX.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KaiserWerk.PersonaX.App.ViewModels;

public class AllPersonsViewModel : ViewModelBase
{
    private AppDbContext dbContext;
    private List<Person> allPersons;
    public List<Person> AllPersons { get => this.allPersons; set => base.Set(ref this.allPersons, value); }

    public Command LoadDataCommand { get; }
    public Command<object> DeleteCommand { get; }
    public Command<object> ShowDetailsCommand { get; }
    public AllPersonsViewModel(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.LoadDataCommand = new Command(async () => await this.LoadDataCommandExecute());
        this.DeleteCommand = new Command<object>(async o => await this.DeleteCommandExecute(o));
        this.ShowDetailsCommand = new Command<object>(async o => await this.ShowDetailsCommandExecute(o));
    }

    private async Task LoadDataCommandExecute()
    {
        //await this.dbContext.Persons.AddAsync(new Person()
        //{
        //    Firstname = "Robin",
        //    Lastname = "Kaiser",
        //    BirthDate = DateTimeOffset.Parse("1989-09-19"),
        //    BodyHeight = new Random().Next(150, 190),
        //    BodyWeight = new Random().Next(50, 300),
        //    Photos = new List<Photo>()
        //    {
        //       new Photo()
        //       {
        //           LocalFilename = "some-photo-u4ezhsw.png",
        //           LocalPath = "",
        //           MimeType = "image/png",
        //           TakenAt = DateTimeOffset.Now,
        //       }
        //    },
        //});
        //await this.dbContext.SaveChangesAsync();
        this.AllPersons = await dbContext.Persons.ToListAsync();
    }

    private async Task DeleteCommandExecute(object obj)
    {
        if (obj == null)
            return;

        if (!(obj is Person p))
            return;
        
        try
        {
            p.Photos?.Clear();
            this.dbContext.Persons.Remove(p);
            await this.dbContext.SaveChangesAsync();
            this.AllPersons.Remove(p);
        }
        catch (Exception ex)
        {

          
        }
        
    }

    private async Task ShowDetailsCommandExecute(object obj)
    {
        if (obj == null)
            return;

        if (!(obj is Person p))
            return;

        // for now, navigate to media capture page
        await Shell.Current.GoToAsync($"//CaptureMedia?PersonId={p.Id}");
    }
}
