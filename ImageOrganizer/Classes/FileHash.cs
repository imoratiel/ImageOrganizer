using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ImageOrganizer.Classes
{
    public class FileHash
    {
        private SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        public string GetHashSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                return BytesToString(Sha256.ComputeHash(stream));
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }
    }
}
