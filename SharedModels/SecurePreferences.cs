using NAudio.Wave.Compression;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;

namespace uk.JohnCook.dotnet.StreamController.SharedModels
{
    public class SecurePreferences
    {
        // Byte length constants for AES-256 GCM Mode
        private const int KEY_LENGTH = 32;
        private const int NONCE_LENGTH = 12;
        private const int TAG_LENGTH = 16;

        static readonly CspParameters rsaCspParams = new CspParameters()
        {
            KeyContainerName = "user-preferences"
        };
        private static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private readonly string rsaEncryptedAesKey;

        public SecurePreferences()
        {
            // Create a new AES-256 key on instantiation.
            rsaEncryptedAesKey = CreateNewAesKey();
        }

        /// <summary>
        /// Stores a user preference encrypted using AES-256.
        /// Method is not static to ensure an instance of the class (and a new AES key) is created.
        /// </summary>
        /// <param name="preference">The user preference/setting string (i.e. where to store it).</param>
        /// <param name="secret">A reference to the data to store. The original data should be zeroed out by this method.</param>
        /// <param name="associatedData">Optional associated data. It will need to be stored separately.</param>
        /// <returns>True on success.</returns>
        public bool StoreString(ref string preference, ref char[] secret, byte[] associatedData = null)
        {
            if (secret == null) { throw new ArgumentNullException(nameof(secret)); }

            // This is not a static method.
            // The only way to store a new password is by creating an instance of the class, which creates a new AES key.
            using RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(4096, rsaCspParams);
            Span<byte> aesKey = AsymmetricDecryptFromBase64(rsaProvider, rsaEncryptedAesKey);

            // NIST recommends only using a 12-byte/96-bit nonce for AES-GCM. It shouldn't be reused.
            byte[] nonce = new byte[NONCE_LENGTH];
            rngCsp.GetBytes(nonce);
            // Note: Associated data that is passed in on encryption must also be passed in for decryption. Default is null.
            //        This data will not be stored with the other data needed to decrypt the message.
            // AES-GCM generates output that is the same length as input.
            byte[] cipherText = new byte[secret.Length];
            // AES-GCM generated tag can be truncated, but its full size is 16-byte/128-bit.
            byte[] tag = new byte[TAG_LENGTH];

            using AesGcm aesGcm = new AesGcm(aesKey);
            // Use UTF-8 Encoding for the secret.
            aesGcm.Encrypt(nonce, Encoding.UTF8.GetBytes(secret), cipherText, tag, associatedData);
            // Store the length of the original secret, and then wipe the data.
            int secretLength = secret.Length;
            Array.Clear(secret, 0, secretLength);

            // Create a user preference string. The key, nonce, associated data, tag, and cipher text are needed to decrypt.
            // Format: Base64(RsaEncrypt(concat(key, nonce, tag, cipherText)))
            // Zero out each array once it is no longer needed.
            byte[] preferenceString = new byte[secretLength + KEY_LENGTH + NONCE_LENGTH + TAG_LENGTH];
            Buffer.BlockCopy(aesKey.ToArray(), 0, preferenceString, 0, aesKey.Length);
            aesKey.Clear();
            Buffer.BlockCopy(nonce, 0, preferenceString, KEY_LENGTH, NONCE_LENGTH);
            Array.Clear(nonce, 0, NONCE_LENGTH);
            Buffer.BlockCopy(tag, 0, preferenceString, KEY_LENGTH + NONCE_LENGTH, TAG_LENGTH);
            Array.Clear(tag, 0, TAG_LENGTH);
            Buffer.BlockCopy(cipherText, 0, preferenceString, KEY_LENGTH + NONCE_LENGTH + TAG_LENGTH, cipherText.Length);
            Array.Clear(cipherText, 0, cipherText.Length);

            preference = AsymmetricEncryptToBase64(rsaProvider, preferenceString);

            // TODO: Return false on failure.
            return true;
        }

        /// <summary>
        /// Decrypt an AES-256 encrypted user preference.
        /// Method is static as class doesn't need instantiation.
        /// </summary>
        /// <param name="preference">The user preference/setting string (i.e. where it is stored).</param>
        /// <param name="decryptedData">A reference to where to store the decrypted string.</param>
        /// <param name="associatedData">Optional associated data that was used during encryption.</param>
        public static void GetString(string preference, ref char[] decryptedData, byte[] associatedData = null)
        {
            if (preference == null) { throw new ArgumentNullException(nameof(preference)); }
            if (decryptedData == null) { throw new ArgumentNullException(nameof(decryptedData)); }

            using RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(4096, rsaCspParams);

            ReadOnlyMemory<byte> preferenceString = AsymmetricDecryptFromBase64(rsaProvider, preference).AsMemory<byte>();

            // Slice the decrypted preference string to extract the key, nonce, and tag .
            using AesGcm aesGcm = new AesGcm(preferenceString.Slice(0, KEY_LENGTH).ToArray());
            ReadOnlyMemory<byte> nonce = preferenceString.Slice(KEY_LENGTH, NONCE_LENGTH);
            ReadOnlyMemory<byte> tag = preferenceString.Slice(KEY_LENGTH + NONCE_LENGTH, TAG_LENGTH);
            // Calculate the length of the encrypted data.
            int dataLength = preferenceString.Length - KEY_LENGTH - NONCE_LENGTH - TAG_LENGTH;
            int cipherTextStart = preferenceString.Length - dataLength;
            // Create a temporary array to store the decrypted data.
            byte[] dataTemp = new byte[dataLength];

            // Decrypt the data.
            aesGcm.Decrypt(nonce.ToArray(), preferenceString.Slice(cipherTextStart, dataLength).ToArray(), tag.ToArray(), dataTemp, associatedData);
            // Use UTF-8 encoding for the decrypted data.
            decryptedData = Encoding.UTF8.GetString(dataTemp).ToArray();
            // Zero the temporary array.
            Array.Clear(dataTemp, 0, dataLength);
        }

        /// <summary>
        /// Creates a new AES-256 Symmetric Key encrypted by a 4096 bit RSA key.
        /// </summary>
        /// <returns>A base64 string of the RSA-encrypted AES key.</returns>
        private static string CreateNewAesKey()
        {
            using RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(4096, rsaCspParams);

            // 32 bytes * 8 = AES-256
            byte[] SuperRandomSymmetricKey = new byte[KEY_LENGTH];
            rngCsp.GetBytes(SuperRandomSymmetricKey);

            using SHA256 sha256 = SHA256.Create();

            // SHA256 hash to check consistency
            string unencryptedAesKeyHash = BitConverter.ToString(sha256.ComputeHash(SuperRandomSymmetricKey)).ToUpperInvariant().Replace("-", String.Empty, StringComparison.Ordinal);

            // SPKI of asymmetric keypair formatted in same way as HTTP-Public-Key-Header to check it doesn't change
            X509SubjectKeyIdentifierExtension spki = new X509SubjectKeyIdentifierExtension(rsaProvider.ExportSubjectPublicKeyInfo(), false);

            // Encrypted AES key in base 64 should be impossible to decrypt even with the IV
            string encryptedAesKeyBase64 = AsymmetricEncryptToBase64(rsaProvider, SuperRandomSymmetricKey);
            Array.Clear(SuperRandomSymmetricKey, 0, SuperRandomSymmetricKey.Length);

            // SHA256 hash of decrypted AES key to check consistency
            Span<byte> decryptedAesKey = AsymmetricDecryptFromBase64(rsaProvider, encryptedAesKeyBase64);
            string aesKeyHash = BitConverter.ToString(sha256.ComputeHash(decryptedAesKey.ToArray())).ToUpperInvariant().Replace("-", String.Empty, StringComparison.Ordinal);

            return encryptedAesKeyBase64;
        }

        /// <summary>
        /// Encrypts data using RSA.
        /// </summary>
        /// <param name="rsaProvider">The RSA provider including a private key.</param>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>A base64 encoded string of the encrypted data.</returns>
        private static string AsymmetricEncryptToBase64(RSACryptoServiceProvider rsaProvider, byte[] data)
        {
            byte[] encryptedBytes = rsaProvider.Encrypt(data, true);
            Span<byte> base64 = ArrayPool<byte>.Shared.Rent(encryptedBytes.Length * 4);
            Base64.EncodeToUtf8(encryptedBytes, base64, out int _, out int written, true);
            string base64String = Encoding.UTF8.GetString(base64.Slice(0, written));
            ArrayPool<byte>.Shared.Return(base64.ToArray());
            return base64String;
        }

        /// <summary>
        /// Decrypts data using RSA
        /// </summary>
        /// <param name="rsaProvider">The RSA provider including a private key.</param>
        /// <param name="ciphertext">The base64 encoded encrypted data to decrypt.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        private static byte[] AsymmetricDecryptFromBase64(RSACryptoServiceProvider rsaProvider, string ciphertext)
        {
            if (ciphertext == null) { throw new ArgumentNullException(nameof(ciphertext)); }
            if (ciphertext.Length == 0) { throw new ArgumentException($"{nameof(ciphertext)} has a length of {ciphertext.Length}"); }

            using SHA256 sha256 = SHA256.Create();

            Span<byte> decryptedData = ArrayPool<byte>.Shared.Rent(ciphertext.Length * 8);
            Span<byte> poolRef = decryptedData;
            decryptedData = Encoding.UTF8.GetBytes(ciphertext);
            Base64.DecodeFromUtf8InPlace(decryptedData, out int written);
            ArrayPool<byte>.Shared.Return(poolRef.ToArray());

            return rsaProvider.Decrypt(decryptedData.Slice(0, written).ToArray(), true);
        }

        public static char[] CreateAuthResponse(string preference, string salt, string challenge, byte[] associatedData = null)
        {
            if (preference == null) { throw new ArgumentNullException(nameof(preference)); }

            using SHA256 sha256 = SHA256.Create();

            char[] password = new char[preference.Length];
            GetString(preference, ref password, associatedData);

            char[] secret_string = String.Concat(password.AsSpan(), salt.AsSpan()).ToArray();
            Array.Clear(password, 0, password.Length);
            byte[] secret_hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret_string));
            Array.Clear(secret_string, 0, secret_string.Length);
            byte[] secret = new byte[preference.Length];
            Base64.EncodeToUtf8(secret_hash, secret, out int consumed, out int written);
            Array.Clear(secret_hash, 0, secret_hash.Length);

            char[] auth_response_string = String.Concat(Encoding.UTF8.GetString(secret.AsSpan().Slice(0, written)), challenge).ToArray();
            Array.Clear(secret, 0, written);
            byte[] auth_response_hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(auth_response_string));
            Array.Clear(auth_response_string, 0, auth_response_string.Length);
            byte[] auth_response = new byte[preference.Length];
            Base64.EncodeToUtf8(auth_response_hash, auth_response, out consumed, out written);

            return Encoding.UTF8.GetString(auth_response.AsSpan().Slice(0, written)).ToCharArray();
        }
    }

    /// <summary>
    /// Helper methods for transforming encryption-related variables.
    /// </summary>
    public static class Transform
    {
        /// <summary>
        /// Converts a hexadecimal string into an array of bytes.
        /// </summary>
        /// <param name="hexString">The hexadecimal string to transform, with no separator between bytes.</param>
        /// <returns>The byte[] representation of the hexadecimal string.</returns>
        public static byte[] FromHex(string hexString)
        {
            if (hexString == null) { throw new ArgumentNullException(nameof(hexString)); }

            byte[] inBytes = new byte[hexString.Length / 2];
            int bytes = 0;
            do
            {
                inBytes[bytes] = Convert.ToByte(hexString.Substring(bytes * 2, 2), 16);
                bytes++;
            } while (bytes * 2 < hexString.Length);
            return inBytes;
        }

        /// <summary>
        /// Transforms a byte array into a base64-encoded UTF8 string.
        /// </summary>
        /// <param name="spkiBytes">The byte array to transform, such as an SPKI-formatted public key.</param>
        /// <returns>A string in the same format used for HTTP Public-Key Pinning.</returns>
        public static string ToBase64(byte[] spkiBytes)
        {
            if (spkiBytes == null) { throw new ArgumentNullException(nameof(spkiBytes)); }

            using SHA256 sha256 = SHA256.Create();
            Span<byte> spkiUtf8Bytes = ArrayPool<byte>.Shared.Rent(spkiBytes.Length);
            Base64.EncodeToUtf8(sha256.ComputeHash(spkiBytes), spkiUtf8Bytes, out int consumed, out int written, true);
            return Encoding.UTF8.GetString(spkiUtf8Bytes.Slice(0, written));
        }
    }
}
