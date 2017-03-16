using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;

namespace ImapTray
{
    static class ConfigurationManager
    {
        private const string FileName = "Credentials.dat";
        private const IsolatedStorageScope Scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

        public static event OnConfigurationChanged onConfigurationChanged;

        public delegate void OnConfigurationChanged(Configuration configuration);

        private static byte[] Decrypt(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        }

        private static Configuration _cache = null;

        public static Configuration Load()
        {
            if (_cache != null)
            {
                return _cache;
            }

            using (var storage = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                if (!storage.FileExists(FileName))
                {
                    return new Configuration(new Account[] {});
                }

                byte[] encryptedData;
                using (var stream = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Read, storage))
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    encryptedData = memoryStream.ToArray();                   
                }

                byte[] data = Decrypt(encryptedData);
                using (var memoryStream = new MemoryStream(data))
                {
                    var serializer = new DataContractJsonSerializer(typeof(Configuration));
                    var result = (Configuration) serializer.ReadObject(memoryStream);
                    _cache = result;
                    return result;
                }
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

            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, configuration);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(memoryStream))
                {                  
                    string json = reader.ReadToEnd();
                    byte[] data = Encrypt(json);

                    using (var storage = IsolatedStorageFile.GetStore(Scope, null, null))
                    using (var stream = new IsolatedStorageFileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, storage))
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
