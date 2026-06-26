using KaiserWerk.PersonaX.Persistence;
using KaiserWerk.PersonaX.Persistence.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using KaiserMVVMCore;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace KaiserWerk.PersonaX.App.ViewModels;

public class CaptureMediaViewModel : ViewModelBase
{
    private readonly string appData = Path.Join(FileSystem.Current.AppDataDirectory, "PersonaX");
    private readonly AppDbContext dbContext;
    public int personId;
    public int PersonId { get => this.personId; set => base.Set(ref this.personId, value); }

    public void SetPersonId(int id)
    {
        this.PersonId = id;
    }

    public Command CapturePhotoCommand { get; }
    public Command CaptureVideoCommand { get; }

    public CaptureMediaViewModel(AppDbContext dbContext)
    {
        if (!Directory.Exists(this.appData))
            Directory.CreateDirectory(this.appData);

        this.dbContext = dbContext;
        this.CapturePhotoCommand = new Command(async () => await this.CapturePhotoCommandExecute());
        this.CaptureVideoCommand = new Command(async () => await this.CaptureVideoCommandExecute());
    }

    private async Task CapturePhotoCommandExecute()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                string localFilePath = Path.Combine(appData, photo.FileName);
                using Stream sourceStream = await photo.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);
                await sourceStream.CopyToAsync(localFileStream);

                var photoEntry = new Photo()
                {
                    LocalFilename = photo.FileName,
                    LocalPath = appData,
                    MimeType = photo.ContentType,
                    TakenAt = DateTimeOffset.Now,
                };
                if (this.PersonId > 0)
                {
                    var person = await this.dbContext.Persons.FirstOrDefaultAsync(e => e.Id == this.PersonId);
                    if (person != null) 
                    {
                        if (person.Photos == null)
                            person.Photos = new List<Photo>();
                        person.Photos.Add(photoEntry);
                        await this.dbContext.SaveChangesAsync();
                    }
                }
               
            }
        }
    }

    private async Task CaptureVideoCommandExecute()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult video = await MediaPicker.Default.CaptureVideoAsync();
            if (video != null)
            {
                string localFilePath = Path.Combine(appData, video.FileName);
                if (!Directory.Exists(localFilePath))
                    Directory.CreateDirectory(localFilePath);

                using Stream sourceStream = await video.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);
            }
        }
    }
}
