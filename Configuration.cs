using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;

namespace ImapTray
{
    static class Configuration
    {
        private const string FileName = "Credentials.dat";
        private const IsolatedStorageScope Scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

        private static byte[] Decrypt(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        }

        public static Credential[] Load()
        {
            using (var storage = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                if (!storage.FileExists(FileName))
                {
                    return new Credential[] {};
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
                    var serializer = new DataContractJsonSerializer(typeof(Credential[]));
                    var result = (Credential[]) serializer.ReadObject(memoryStream);
                    return result;
                }
            }
        }

        private static byte[] Encrypt(string data)
        {
            return ProtectedData.Protect(Encoding.Unicode.GetBytes(data), null, DataProtectionScope.CurrentUser);
        }

        public static void Save(Credential[] credentials)
        {
            var serializer = new DataContractJsonSerializer(typeof(Credential[]));

            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, credentials);
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
        }
    }
}
