using KaiserWerk.PersonaX.Persistence.Models;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace KaiserWerk.PersonaX.App.Converters;

public class PersonAddressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (!(value is Person p))
            throw new ArgumentException($"{nameof(value)} must be of type {typeof(Person)}, was {value.GetType()} instead");

        return $"{p.Street} {p.HouseNumber}, {p.ZipCode} {p.City} ({p.Country})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
