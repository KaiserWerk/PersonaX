using System;
using System.Collections.Generic;

namespace KaiserWerk.PersonaX.Persistence.Models;

public class Person
{
    public int Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public DateTimeOffset BirthDate { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public int BodyHeight { get; set; }
    public int BodyWeight { get; set; }

    public ICollection<Photo> Photos { get; set; }
}
