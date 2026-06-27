namespace PersonaX.UI.Data
{
    public static class Constants
    {
        // Existing app database
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        // Lokale Personen-Datenbank
        public const string PiiDatabaseFilename = "persona.db";

        public static string PiiDatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, PiiDatabaseFilename);

        // Media storage root
        public static string MediaRootPath =>
            Path.Combine(FileSystem.AppDataDirectory, "Media");

        // Export directory for backups
        public static string ExportPath =>
            Path.Combine(FileSystem.AppDataDirectory, "Exports");
    }
}