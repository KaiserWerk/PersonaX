namespace PersonaX.UI.Models
{
    /// <summary>
    /// Represents a media item (photo, audio, video) associated with a person.
    /// Media files are stored encrypted on disk.
    /// </summary>
    public class MediaItem
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        /// <summary>
        /// Type of media: Photo, Audio, Video.
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Original filename before encryption.
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// Encrypted file path relative to MediaRootPath.
        /// </summary>
        public string EncryptedFilePath { get; set; } = string.Empty;

        /// <summary>
        /// MIME type of the original media.
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Initialization vector used for AES-GCM encryption (Base64).
        /// </summary>
        public string IV { get; set; } = string.Empty;

        /// <summary>
        /// Authentication tag for AES-GCM (Base64).
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public override string ToString() => OriginalFileName;
    }

    public enum MediaType
    {
        Photo = 0,
        Audio = 1,
        Video = 2
    }
}
