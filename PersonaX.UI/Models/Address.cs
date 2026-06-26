namespace PersonaX.UI.Models
{
    /// <summary>
    /// Represents a physical address associated with a person.
    /// </summary>
    public class Address
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string FullAddress => 
            string.Join(", ", new[] { Street, City, State, PostalCode, Country }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        public override string ToString() => FullAddress;
    }
}
