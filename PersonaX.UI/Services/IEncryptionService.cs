namespace PersonaX.UI.Services
{
    public sealed record EncryptedPayload(byte[] Ciphertext, byte[] Iv, byte[] Tag);

    /// <summary>
    /// Service for cryptographic operations including key derivation and AES-GCM encryption.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Derives a master key from a PIN and salt using PBKDF2.
        /// </summary>
        /// <param name="pin">User PIN</param>
        /// <param name="salt">Random salt (should be 16+ bytes)</param>
        /// <param name="iterations">PBKDF2 iterations (default 100,000+)</param>
        /// <returns>Derived key bytes (32 bytes for AES-256)</returns>
        byte[] DeriveMasterKeyFromPin(string pin, byte[] salt, int iterations = 100000);

        /// <summary>
        /// Derives a sub-key from the master key for a specific purpose.
        /// </summary>
        /// <param name="masterKey">Master key bytes</param>
        /// <param name="purpose">Purpose identifier (e.g., "db", "file", "export")</param>
        /// <returns>Derived sub-key (32 bytes)</returns>
        byte[] DeriveSubKey(byte[] masterKey, string purpose);

        /// <summary>
        /// Encrypts data using AES-256-GCM.
        /// </summary>
        /// <param name="plaintext">Data to encrypt</param>
        /// <param name="key">Encryption key (32 bytes)</param>
        /// <returns>Tuple of (ciphertext, iv, tag)</returns>
        (byte[] ciphertext, byte[] iv, byte[] tag) EncryptAesGcm(byte[] plaintext, byte[] key);

        /// <summary>
        /// Decrypts data using AES-256-GCM.
        /// </summary>
        /// <param name="ciphertext">Encrypted data</param>
        /// <param name="key">Decryption key (32 bytes)</param>
        /// <param name="iv">Initialization vector</param>
        /// <param name="tag">Authentication tag</param>
        /// <returns>Decrypted plaintext</returns>
        byte[] DecryptAesGcm(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag);

        /// <summary>
        /// Encrypts a file stream using AES-GCM.
        /// </summary>
        Task<EncryptedPayload> EncryptFileAsync(Stream input, byte[] key);

        /// <summary>
        /// Decrypts a file stream using AES-GCM.
        /// </summary>
        Task DecryptFileAsync(Stream input, Stream output, byte[] key, byte[] iv, byte[] tag);

        /// <summary>
        /// Generates a cryptographically secure random salt.
        /// </summary>
        byte[] GenerateSalt(int size = 16);

        /// <summary>
        /// Generates a cryptographically secure random key.
        /// </summary>
        byte[] GenerateRandomKey(int size = 32);
    }
}
