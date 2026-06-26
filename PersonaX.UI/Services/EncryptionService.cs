using System.Security.Cryptography;
using System.Text;

namespace PersonaX.UI.Services
{
    /// <summary>
    /// Implementation of cryptographic operations using .NET's System.Security.Cryptography APIs.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private const int Pbkdf2Iterations = 100000;
        private const int AesKeySize = 32; // 256 bits
        private const int AesNonceSize = 12; // 96 bits for GCM
        private const int AesTagSize = 16; // 128 bits

        public byte[] DeriveMasterKeyFromPin(string pin, byte[] salt, int iterations = Pbkdf2Iterations)
        {
            if (string.IsNullOrEmpty(pin))
                throw new ArgumentException("PIN cannot be empty", nameof(pin));
            if (salt == null || salt.Length < 16)
                throw new ArgumentException("Salt must be at least 16 bytes", nameof(salt));

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(pin),
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(AesKeySize);
        }

        public byte[] DeriveSubKey(byte[] masterKey, string purpose)
        {
            if (masterKey == null || masterKey.Length != AesKeySize)
                throw new ArgumentException("Master key must be 32 bytes", nameof(masterKey));
            if (string.IsNullOrEmpty(purpose))
                throw new ArgumentException("Purpose cannot be empty", nameof(purpose));

            // Use HKDF to derive a purpose-specific key
            var purposeBytes = Encoding.UTF8.GetBytes(purpose);
            var hkdf = new HKDFSHA256(masterKey, purposeBytes, null);
            return hkdf.Expand(AesKeySize);
        }

        public (byte[] ciphertext, byte[] iv, byte[] tag) EncryptAesGcm(byte[] plaintext, byte[] key)
        {
            if (plaintext == null || plaintext.Length == 0)
                throw new ArgumentException("Plaintext cannot be empty", nameof(plaintext));
            if (key == null || key.Length != AesKeySize)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));

            var nonce = GenerateSalt(AesNonceSize);
            var tag = new byte[AesTagSize];
            var ciphertext = new byte[plaintext.Length];

            using var aesGcm = new AesGcm(key, AesTagSize);
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

            return (ciphertext, nonce, tag);
        }

        public byte[] DecryptAesGcm(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
        {
            if (ciphertext == null || ciphertext.Length == 0)
                throw new ArgumentException("Ciphertext cannot be empty", nameof(ciphertext));
            if (key == null || key.Length != AesKeySize)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));
            if (iv == null || iv.Length != AesNonceSize)
                throw new ArgumentException($"IV must be {AesNonceSize} bytes", nameof(iv));
            if (tag == null || tag.Length != AesTagSize)
                throw new ArgumentException($"Tag must be {AesTagSize} bytes", nameof(tag));

            var plaintext = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(key, AesTagSize);
            aesGcm.Decrypt(iv, ciphertext, tag, plaintext);

            return plaintext;
        }

        public async Task<EncryptedPayload> EncryptFileAsync(Stream input, byte[] key)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (key == null || key.Length != AesKeySize)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));

            // Read entire file into memory (for large files, consider chunked encryption)
            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            var plaintext = ms.ToArray();

            var (ciphertext, ivResult, tagResult) = EncryptAesGcm(plaintext, key);
            return new EncryptedPayload(ciphertext, ivResult, tagResult);
        }

        public async Task DecryptFileAsync(Stream input, Stream output, byte[] key, byte[] iv, byte[] tag)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));

            // Read entire encrypted file
            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            var ciphertext = ms.ToArray();

            var plaintext = DecryptAesGcm(ciphertext, key, iv, tag);

            await output.WriteAsync(plaintext);
            await output.FlushAsync();
        }

        public byte[] GenerateSalt(int size = 16)
        {
            if (size < 8)
                throw new ArgumentException("Salt size must be at least 8 bytes", nameof(size));

            return RandomNumberGenerator.GetBytes(size);
        }

        public byte[] GenerateRandomKey(int size = AesKeySize)
        {
            if (size < 16)
                throw new ArgumentException("Key size must be at least 16 bytes", nameof(size));

            return RandomNumberGenerator.GetBytes(size);
        }
    }

    /// <summary>
    /// HKDF implementation using SHA256 for key derivation.
    /// </summary>
    internal class HKDFSHA256
    {
        private readonly byte[] _prk;
        private readonly byte[] _info;

        public HKDFSHA256(byte[] ikm, byte[] salt, byte[]? info)
        {
            // Extract step: HMAC-SHA256(salt, ikm)
            using var hmac = new HMACSHA256(salt);
            _prk = hmac.ComputeHash(ikm);
            _info = info ?? Array.Empty<byte>();
        }

        public byte[] Expand(int length)
        {
            var result = new byte[length];
            var n = (int)Math.Ceiling((double)length / 32); // SHA256 output is 32 bytes

            var t = Array.Empty<byte>();
            var resultOffset = 0;

            for (byte i = 1; i <= n; i++)
            {
                using var hmac = new HMACSHA256(_prk);
                var input = new byte[t.Length + _info.Length + 1];
                Buffer.BlockCopy(t, 0, input, 0, t.Length);
                Buffer.BlockCopy(_info, 0, input, t.Length, _info.Length);
                input[^1] = i;

                t = hmac.ComputeHash(input);
                var copyLength = Math.Min(t.Length, length - resultOffset);
                Buffer.BlockCopy(t, 0, result, resultOffset, copyLength);
                resultOffset += copyLength;
            }

            return result;
        }
    }
}
