using System;

namespace KaiserWerk.PersonaX.Persistence.Models;

public class Photo
{
    public int Id { get; set; }
    public string LocalPath { get; set; }
    public string LocalFilename { get; set; }
    public string MimeType { get; set; }
    public DateTimeOffset TakenAt { get; set; }

    public Person? Person { get; set; }
}
