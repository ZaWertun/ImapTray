using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System;

namespace ImapTray
{
    static class ConfigurationManager
    {
        private const string DirName = "ImapTray";
        private const string FileName = "Credentials.dat";

        public static event OnConfigurationChanged onConfigurationChanged;

        public delegate void OnConfigurationChanged(Configuration configuration);

        private static byte[] Decrypt(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        }

        private static Configuration _cache = null;

        private static string GetDirPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DirName);
            Debug.WriteLine("Application configuration dir: " + path);
            return path;
        }

        public static Configuration Load()
        {
            if (_cache != null)
            {
                return _cache;
            }

            var filePath = Path.Combine(GetDirPath(), FileName);
            if (!File.Exists(filePath))
            {
                return new Configuration(new Account[] { });
            }

            byte[] encryptedData;
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                encryptedData = memoryStream.ToArray();
            }

            byte[] data = Decrypt(encryptedData);
            using (var memoryStream = new MemoryStream(data))
            {
                var serializer = new DataContractJsonSerializer(typeof(Configuration));
                var result = (Configuration)serializer.ReadObject(memoryStream);
                _cache = result;
                return result;
            }
        }

        private static byte[] Encrypt(string data)
        {
            return ProtectedData.Protect(Encoding.Unicode.GetBytes(data), null, DataProtectionScope.CurrentUser);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void Save(Configuration configuration)
        {
            var serializer = new DataContractJsonSerializer(typeof(Configuration));

            var dirPath = GetDirPath();
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = Path.Combine(dirPath, FileName);
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, configuration);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(memoryStream))
                {                  
                    string json = reader.ReadToEnd();
                    byte[] data = Encrypt(json);

                    using (var stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Flush(true);
                    }
                }
            }

            _cache = configuration;

            onConfigurationChanged(configuration);
        }
    }
}
