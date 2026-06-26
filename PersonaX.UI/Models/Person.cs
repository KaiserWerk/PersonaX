namespace PersonaX.UI.Models
{
    /// <summary>
    /// Represents a person with personally identifiable information (PII).
    /// </summary>
    public class Person
    {
        public int ID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for associated address.
        /// </summary>
        public Address? Address { get; set; }

        /// <summary>
        /// Navigation property for associated media items.
        /// </summary>
        public List<MediaItem> MediaItems { get; set; } = [];

        public string FullName => $"{FirstName} {LastName}".Trim();

        public override string ToString() => FullName;
    }
}
