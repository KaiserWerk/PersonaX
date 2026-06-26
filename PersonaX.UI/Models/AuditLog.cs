namespace PersonaX.UI.Models
{
    /// <summary>
    /// Represents an audit log entry for tracking access and modifications.
    /// </summary>
    public class AuditLog
    {
        public int ID { get; set; }

        /// <summary>
        /// Type of action performed.
        /// </summary>
        public AuditAction Action { get; set; }

        /// <summary>
        /// Entity type affected (e.g., "Person", "MediaItem").
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the affected entity.
        /// </summary>
        public int EntityID { get; set; }

        /// <summary>
        /// Additional details about the action.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString() => $"{Action} on {EntityType}#{EntityID} at {Timestamp:g}";
    }

    public enum AuditAction
    {
        Created = 0,
        Modified = 1,
        Deleted = 2,
        Viewed = 3,
        Exported = 4,
        Imported = 5
    }
}
