using System;
using System.Security.Cryptography;
using System.Text;
using Snoop.API.EncryptionService.Services.Interfaces;

namespace Snoop.API.EncryptionService.Services
{
    /// <summary>
    ///  Cryptographically "sound" key generator taken from this stack overflow: https://stackoverflow.com/a/1344255
    /// </summary>

    public class KeyGenerator : IKeyGenerator
    {
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }
}
