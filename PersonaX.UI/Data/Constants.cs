namespace PersonaX.UI.Data
{
    public static class Constants
    {
        // Existing app database
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        // PII-specific database (SQLCipher encrypted)
        public const string PiiDatabaseFilename = "persona.db";

        public static string PiiDatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, PiiDatabaseFilename);

        // Media storage root for encrypted files
        public static string MediaRootPath =>
            Path.Combine(FileSystem.AppDataDirectory, "Media");

        // Export directory for encrypted backups
        public static string ExportPath =>
            Path.Combine(FileSystem.AppDataDirectory, "Exports");
    }
}