// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace LMS.CRM.Core;

public class PasswordHasher
{
    private const string StaticKey = "DeviM@N";

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            // Concatenate password with static key
            var combinedBytes = Encoding.UTF8.GetBytes(password + StaticKey);

            // Compute hash
            var hashBytes = sha256.ComputeHash(combinedBytes);

            // Convert hash to hexadecimal string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // Compute hash of the provided password
        string computedHash = HashPassword(password);

        // Compare computed hash with the stored hash
        return computedHash.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
    }
}
public class PasswordEncryptor
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("TivaS@N@TKey8822"); // Must be 16 bytes for AES-128, 24 bytes for AES-192, and 32 bytes for AES-256
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("InitializationIV"); // Must be 16 bytes for AES

    public static string EncryptPassword(string password)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(password);
                    }
                }

                var encryptedBytes = ms.ToArray();
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }

    public static string DecryptPassword(string encryptedPassword)
    {
        if (string.IsNullOrEmpty(encryptedPassword))
        {
            return null;
        }

        var encryptedBytes = Convert.FromBase64String(encryptedPassword);

        using (var aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream(encryptedBytes))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}